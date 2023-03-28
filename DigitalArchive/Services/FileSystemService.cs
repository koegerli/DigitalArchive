using CommunityToolkit.Mvvm.DependencyInjection;
using DigitalArchive.Models;
using DigitalArchive.Options;
using IWshRuntimeLibrary;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Xml.Linq;
using File = System.IO.File;

namespace DigitalArchive.Services;

public class FileSystemService
{
    private readonly string[] _inputFolderPaths;
    private readonly string _outputFolderPath;

    private readonly FileSystemWatcher[] _inputWatchers;
    private readonly FileSystemWatcher _outputWatcher;

    private readonly Dictionary<string, ImageSource?> _iconCache = new();

    public event EventHandler? InputItemsChanged;
    public event EventHandler? OutputItemsChanged;

    public FileSystemService(IOptions<FileSystemOptions> options)
    {
        _inputFolderPaths = options.Value.InputPaths;
        _outputFolderPath = options.Value.OutputPath;

        CreateDirectories();

        _inputWatchers = CreateWatchers(_inputFolderPaths, () => InputItemsChanged?.Invoke(this, EventArgs.Empty));
        _outputWatcher = CreateWatcher(_outputFolderPath, () => OutputItemsChanged?.Invoke(this, EventArgs.Empty));
    }

    private void CreateDirectories()
    {
        foreach (var inputPath in _inputFolderPaths)
        {
            Directory.CreateDirectory(inputPath);
        }
        Directory.CreateDirectory(_outputFolderPath);
    }

    private FileSystemWatcher[] CreateWatchers(string[] paths, Action handler)
    {
        return paths.Select(x => CreateWatcher(x, handler)).ToArray();
    }

    private FileSystemWatcher CreateWatcher(string path, Action handler)
    {
        var watcher = new FileSystemWatcher(path);

        watcher.IncludeSubdirectories = true;
        watcher.Changed += (_, __) => handler();
        watcher.Deleted += (_, __) => handler();
        watcher.Created += (_, __) => handler();
        watcher.Renamed += (_, __) => handler();
        watcher.EnableRaisingEvents = true;

        return watcher;
    }

    public IEnumerable<ExplorerItem> GetInputItems() => GetExplorerItems(_inputFolderPaths, "*.*", includeEmptyFolders: false);
    public IEnumerable<ExplorerItem> GetOutputItems() => GetExplorerItems(new[] { _outputFolderPath }, "*.*", includeEmptyFolders: true);

    public IEnumerable<FileItem> GetArchiveItems(string path)
    {
        return Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories)
            .Select(CreateFileItem);
    }

    public IEnumerable<ArchiveCategory> GetArchiveCategories()
    {
        return Directory.EnumerateDirectories(_outputFolderPath, "*", SearchOption.AllDirectories)
            .Where(x => Directory.EnumerateFiles(x, "*.*", SearchOption.AllDirectories).Any())
            .Select(CreateArchiveCategory);
    }

    public void MoveToOutputFolder(string inputPath, string[] selectedOutputPaths, int index)
    {
        var outputFolder = Path.Combine(_outputFolderPath, DateTime.Now.Year.ToString());
        Directory.CreateDirectory(outputFolder);

        var fileExtension = Path.GetExtension(inputPath);
        var targetFileName = $"{index}{fileExtension}";
        var targetPath = Path.Combine(outputFolder, targetFileName);

        File.Move(inputPath, targetPath);

        foreach (var selectedOutputPath in selectedOutputPaths)
        {
            CreateShortcut(selectedOutputPath, targetPath);
        }
    }

    private void CreateShortcut(string outputPath, string targetPath)
    {
        var fileName = Path.GetFileNameWithoutExtension(targetPath);
        var shortcutPath = Path.Combine(outputPath, $"{fileName}.lnk");

        var shell = new WshShell();
        var shortcut = (IWshShortcut)shell.CreateShortcut(shortcutPath);
        shortcut.TargetPath = targetPath;
        shortcut.Save();
    }

    public int GetHighestNumberInOutput()
    {
        var numberedFiles = Directory.EnumerateFiles(_outputFolderPath, "*.*", SearchOption.AllDirectories)
            .Select(x =>
            {
                var fileName = Path.GetFileNameWithoutExtension(x);
                var isNumber = int.TryParse(fileName, out var number);
                return new { IsNumber = isNumber, Number = number };
            })
            .Where(x => x.IsNumber);

        return numberedFiles.Any()
            ? numberedFiles.Max(x => x.Number)
            : 0;
    }

    private IEnumerable<FolderItem> GetExplorerItems(string[] paths, string filePattern, bool includeEmptyFolders)
    {
        return paths.Select(x => GetExplorerItem(x, filePattern, includeEmptyFolders));
    }

    private FolderItem GetExplorerItem(string path, string filePattern, bool includeEmptyFolders)
    {
        var folders = Directory.GetDirectories(path)
            .Select(x => GetExplorerItem(x, filePattern, includeEmptyFolders))
            .Where(x => x.Children.Any() || includeEmptyFolders);

        var files = Directory.GetFiles(path, filePattern)
            .Select(CreateFileItem);

        var children = folders.Cast<ExplorerItem>().Concat(files);

        return CreateFolderItem(path, children);
    }

    private FileItem CreateFileItem(string path)
    {
        return new FileItem
        {
            Name = Path.GetFileName(path),
            Path = path,
            Icon = GetIconAndStoreInCache(path),
        };
    }

    private ImageSource? GetIconAndStoreInCache(string path)
    {
        var extension = Path.GetExtension(path);
        if (!_iconCache.TryGetValue(extension, out ImageSource? iconSource))
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                using var icon = Icon.ExtractAssociatedIcon(path);
                if (icon is null)
                {
                    _iconCache.Add(extension, null);
                    return;
                }

                iconSource = Imaging.CreateBitmapSourceFromHIcon(icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                _iconCache.Add(extension, iconSource);
            });

        }
        return _iconCache[extension];
    }

    private FolderItem CreateFolderItem(string path, IEnumerable<ExplorerItem> children)
    {
        return new FolderItem
        {
            Name = Path.GetFileName(path),
            Path = path,
            Children = children.ToList(),
        };
    }

    private ArchiveCategory CreateArchiveCategory(string path)
    {
        return new ArchiveCategory
        {
            Name = Path.GetFileName(path),
            Path = path,
        };
    }
}

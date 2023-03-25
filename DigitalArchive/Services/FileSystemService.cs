using DigitalArchive.Models;
using DigitalArchive.Options;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalArchive.Services;

public class FileSystemService
{
    private readonly string _inputFolderPath;
    private readonly string _outputFolderPath;

    private readonly FileSystemWatcher _inputWatcher;
    private readonly FileSystemWatcher _outputWatcher;

    public event EventHandler? InputItemsChanged;
    public event EventHandler? OutputItemsChanged;

    public FileSystemService(IOptions<FileSystemOptions> options)
    {
        _inputFolderPath = options.Value.InputPath;
        _outputFolderPath = options.Value.OutputPath;

        Directory.CreateDirectory(_inputFolderPath);
        Directory.CreateDirectory(_outputFolderPath);

        _inputWatcher = CreateWatcher(_inputFolderPath, () => InputItemsChanged?.Invoke(this, EventArgs.Empty));
        _outputWatcher = CreateWatcher(_outputFolderPath, () => OutputItemsChanged?.Invoke(this, EventArgs.Empty));
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

    public IEnumerable<ExplorerItem> GetInputItems() => GetExplorerItems(_inputFolderPath, "*.pdf", includeEmptyFolders: false);
    public IEnumerable<ExplorerItem> GetOutputItems() => GetExplorerItems(_outputFolderPath, "*.pdf", includeEmptyFolders: true);

    public IEnumerable<ArchiveItem> GetArchiveItems(string path)
    {
        return Directory.EnumerateFiles(path, "*.pdf", SearchOption.AllDirectories)
            .Select(CreateArchiveItem);
    }

    public IEnumerable<ArchiveCategory> GetArchiveCategories()
    {
        return Directory.EnumerateDirectories(_outputFolderPath, "*", SearchOption.AllDirectories)
            .Where(x => Directory.EnumerateFiles(x, "*.pdf", SearchOption.AllDirectories).Any())
            .Select(CreateArchiveCategory);
    }

    public void MoveToOutputFolder(string inputPath, int index)
    {
        var outputFolder = Path.Combine(_outputFolderPath, DateTime.Now.Year.ToString());
        Directory.CreateDirectory(outputFolder);
        var outputPath = Path.Combine(outputFolder, $"{index}.pdf");
        File.Move(inputPath, outputPath);
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

    private IEnumerable<ExplorerItem> GetExplorerItems(string path, string filePattern, bool includeEmptyFolders)
    {
        var folders = Directory.GetDirectories(path)
            .Select(x =>
            {
                var items = GetExplorerItems(x, filePattern, includeEmptyFolders);
                if (items.Any() || includeEmptyFolders)
                {
                    return CreateFolderItem(x, items);
                }
                return null; 
            })
            .Where(x => x is not null);

        var files = Directory.GetFiles(path, filePattern)
            .Select(CreateFileItem);

        return Enumerable.Concat(folders, files);
    }

    private ExplorerItem CreateFileItem(string path)
    {
        return new ExplorerItem
        {
            Name = Path.GetFileName(path),
            Path = path,
            Children = Enumerable.Empty<ExplorerItem>(),
            Type = ExplorerItemType.File,
        };
    }

    private ExplorerItem CreateFolderItem(string path, IEnumerable<ExplorerItem> children)
    {
        return new ExplorerItem
        {
            Name = Path.GetFileName(path),
            Path = path,
            Children = children.ToList(),
            Type = ExplorerItemType.Folder,
        };
    }

    private ArchiveItem CreateArchiveItem(string path)
    {
        return new ArchiveItem
        {
            Name = Path.GetFileName(path),
            Path = path,
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

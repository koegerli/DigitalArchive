using DigitalArchive.Models;
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

    public FileSystemService(string inputFolderPath, string outputFolderPath)
    {
        _inputFolderPath = inputFolderPath;
        _outputFolderPath = outputFolderPath;

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

    public IEnumerable<ExplorerItem> GetInputItems() => GetExplorerItems(_inputFolderPath, "*.pdf");
    public IEnumerable<ExplorerItem> GetOutputItems() => GetExplorerItems(_outputFolderPath, "*.pdf");

    public void MoveToOutputFolder(string inputPath, int index)
    {
        var outputFolder = Path.Combine(_outputFolderPath, DateTime.Now.Year.ToString());
        Directory.CreateDirectory(outputFolder);
        var outputPath = Path.Combine(outputFolder, $"{index}.pdf");
        File.Copy(inputPath, outputPath);
        File.Delete(inputPath);
    }

    public int GetHighestNumberInOutput()
    {
        return Directory.EnumerateFiles(_outputFolderPath, "*.*", SearchOption.AllDirectories)
            .Select(x =>
            {
                var fileName = Path.GetFileNameWithoutExtension(x);
                var isNumber = int.TryParse(fileName, out var number);
                return new { IsNumber = isNumber, Number = number };
            })
            .Where(x => x.IsNumber)
            .Max(x => x.Number);
    }

    private IEnumerable<ExplorerItem> GetExplorerItems(string path, string filePattern)
    {
        var folders = Directory.GetDirectories(path)
            .Select(x =>
            {
                var items = GetExplorerItems(x, filePattern);
                if (items.Any())
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
}

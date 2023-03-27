using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DigitalArchive.Models;
using DigitalArchive.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;

namespace DigitalArchive.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly FileSystemService _fileSystemService;

    [ObservableProperty]
    private ObservableCollection<ExplorerItem> _inputItems = new();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ProcessFileCommand))]
    private ExplorerItem? _selectedInputItem;

    [ObservableProperty]
    private int _nextIndex;

    [ObservableProperty]
    private ObservableCollection<ExplorerItem> _outputItems = new();

    [ObservableProperty]
    private ExplorerItem? _selectedOutputItem;

    [ObservableProperty]
    private string? _displayPath;

    [ObservableProperty]
    private ObservableCollection<FileItem> _archiveItems = new();

    [ObservableProperty]
    private FileItem? _selectedArchiveItem;

    [ObservableProperty]
    private ObservableCollection<ArchiveCategory> _archiveCategories = new();

    [ObservableProperty]
    private ArchiveCategory? _selectedArchiveCategory;

    public MainViewModel(FileSystemService fileSystemService)
    {
        _fileSystemService = fileSystemService;
        _fileSystemService.InputItemsChanged += (_, __) => UpdateInputItems();
        _fileSystemService.OutputItemsChanged += (_, __) => UpdateOutputItems();

        ArchiveCategories = new ObservableCollection<ArchiveCategory>(_fileSystemService.GetArchiveCategories());
        SelectedArchiveCategory = ArchiveCategories.FirstOrDefault();
        UpdateInputItems();
        UpdateOutputItems();
    }

    private void UpdateInputItems()
    {
        InputItems = new ObservableCollection<ExplorerItem>(_fileSystemService.GetInputItems());
    }

    private void UpdateOutputItems()
    {
        OutputItems = new ObservableCollection<ExplorerItem>(_fileSystemService.GetOutputItems());
        UpdateArchiveItems();
        NextIndex = _fileSystemService.GetHighestNumberInOutput() + 1;
    }

    private void UpdateArchiveItems()
    {
        if (SelectedArchiveCategory is null)
            Dispatcher.CurrentDispatcher.Invoke(() => ArchiveItems.Clear());
        else
            ArchiveItems = new ObservableCollection<FileItem>(_fileSystemService.GetArchiveItems(SelectedArchiveCategory.Path));
    }

    [RelayCommand(CanExecute = nameof(CanProcessFile))]
    private void ProcessFile()
    {
        var item = SelectedInputItem;
        SelectedInputItem = null;

        //Dies ist ein Workaround, weil der Webbrowser das File nicht sofort freigibt. Deshalb muss kurz gewartet werden.
        Task.Run(async () =>
        {
            await Task.Delay(100);

            var selectedOutputFolders = GetSelectedOutputFolders(OutputItems).Select(x => x.Path).ToArray();

            try
            {
                _fileSystemService.MoveToOutputFolder(item!.Path, selectedOutputFolders, NextIndex);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        });
    }

    private IEnumerable<FolderItem> GetSelectedOutputFolders(IEnumerable<ExplorerItem> items)
    {
        var selectedFolders = new List<FolderItem>();

        foreach (var item in items)
        {
            if (item is FolderItem folderItem)
            {
                if (folderItem.IsSelected)
                    selectedFolders.Add(folderItem);

                selectedFolders.AddRange(GetSelectedOutputFolders(folderItem.Children));
            }
        }

        return selectedFolders;
    }

    private bool CanProcessFile() => SelectedInputItem is FileItem;

    partial void OnSelectedInputItemChanged(ExplorerItem? value)
    {
        DisplayPath = (value is FileItem) ? value.Path : null;
    }

    partial void OnSelectedOutputItemChanged(ExplorerItem? value)
    {
        DisplayPath = (value is FileItem) ? value.Path : null;
    }

    partial void OnSelectedArchiveItemChanged(FileItem? value)
    {
        DisplayPath = (value is not null) ? value.Path : null;
    }

    partial void OnSelectedArchiveCategoryChanged(ArchiveCategory? value)
    {
        UpdateArchiveItems();
    }
}

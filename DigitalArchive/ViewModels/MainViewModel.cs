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
    private ObservableCollection<ArchiveItem> _archiveItems = new();

    [ObservableProperty]
    private ArchiveItem? _selectedArchiveItem;

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
            ArchiveItems.Clear();
        else
            ArchiveItems = new ObservableCollection<ArchiveItem>(_fileSystemService.GetArchiveItems(SelectedArchiveCategory.Path));
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
            _fileSystemService.MoveToOutputFolder(item!.Path, NextIndex);
        });
    }

    private bool CanProcessFile() => SelectedInputItem is not null && SelectedInputItem.Type == ExplorerItemType.File;

    partial void OnSelectedInputItemChanged(ExplorerItem? value)
    {
        DisplayPath = (value is not null && value.Type == ExplorerItemType.File) ? value.Path : null;
    }

    partial void OnSelectedOutputItemChanged(ExplorerItem? value)
    {
        DisplayPath = (value is not null && value.Type == ExplorerItemType.File) ? value.Path : null;
    }

    partial void OnSelectedArchiveItemChanged(ArchiveItem? value)
    {
        DisplayPath = (value is not null) ? value.Path : null;
    }

    partial void OnSelectedArchiveCategoryChanged(ArchiveCategory? value)
    {
        UpdateArchiveItems();
    }
}

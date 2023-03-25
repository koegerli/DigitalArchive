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
    public ObservableCollection<ExplorerItem> _inputItems = new();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ProcessFileCommand))]
    private ExplorerItem? _selectedInputItem;

    [ObservableProperty]
    private int _nextIndex;

    [ObservableProperty]
    public ObservableCollection<ExplorerItem> _outputItems = new();

    [ObservableProperty]
    private ExplorerItem? _selectedOutputItem;

    [ObservableProperty]
    private ExplorerItem? _fileToDisplay;

    public MainViewModel(FileSystemService fileSystemService)
    {
        _fileSystemService = fileSystemService;
        _fileSystemService.InputItemsChanged += (_, __) => UpdateInputItems();
        _fileSystemService.OutputItemsChanged += (_, __) => UpdateOutputItems();

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
        NextIndex = _fileSystemService.GetHighestNumberInOutput() + 1;
    }

    partial void OnSelectedInputItemChanged(ExplorerItem? value)
    {
        FileToDisplay = (value is not null && value.Type == ExplorerItemType.File) ? value : null;
    }

    partial void OnSelectedOutputItemChanged(ExplorerItem? value)
    {
        FileToDisplay = (value is not null && value.Type == ExplorerItemType.File) ? value : null;
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

}

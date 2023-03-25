using DigitalArchive.Models;
using DigitalArchive.Services;
using DigitalArchive.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Path = System.IO.Path;

namespace DigitalArchive;
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;

    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        _viewModel.PropertyChanged += ViewModel_PropertyChanged;
        
        DataContext = _viewModel;
    }

    private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainViewModel.FileToDisplay))
        {
            PdfViewer.Navigate("about:blank");
            
            if (_viewModel.FileToDisplay is not null)
            {
                PdfViewer.Navigate(_viewModel.FileToDisplay.Path);
            }
        }
    }
}

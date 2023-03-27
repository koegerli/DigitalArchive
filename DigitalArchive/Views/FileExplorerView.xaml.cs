using DigitalArchive.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace DigitalArchive.Views;
/// <summary>
/// Interaction logic for FileExplorerView.xaml
/// </summary>
public partial class FileExplorerView : UserControl
{

    public ObservableCollection<ExplorerItem> Items
    {
        get { return (ObservableCollection<ExplorerItem>)GetValue(ItemsProperty); }
        set { SetValue(ItemsProperty, value); }
    }

    // Using a DependencyProperty as the backing store for Items.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty ItemsProperty =
        DependencyProperty.Register("Items", typeof(ObservableCollection<ExplorerItem>), typeof(FileExplorerView), new PropertyMetadata(null));


    public ExplorerItem SelectedItem
    {
        get { return (ExplorerItem)GetValue(SelectedItemProperty); }
        set { SetValue(SelectedItemProperty, value); }
    }

    // Using a DependencyProperty as the backing store for SelectedItem.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty SelectedItemProperty =
        DependencyProperty.Register("SelectedItem", typeof(ExplorerItem), typeof(FileExplorerView), new PropertyMetadata(null));


    public bool ShowCheckBox
    {
        get { return (bool)GetValue(ShowCheckBoxProperty); }
        set { SetValue(ShowCheckBoxProperty, value); }
    }

    // Using a DependencyProperty as the backing store for ShowCheckBox.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty ShowCheckBoxProperty =
        DependencyProperty.Register("ShowCheckBox", typeof(bool), typeof(FileExplorerView), new PropertyMetadata(false));



    public FileExplorerView()
    {
        InitializeComponent();

        MainGrid.DataContext = this;
    }

    private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        SelectedItem = (ExplorerItem)e.NewValue;
    }
}

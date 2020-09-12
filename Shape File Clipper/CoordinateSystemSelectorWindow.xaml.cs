using ArcGIS.Core.Geometry;
using System.ComponentModel;
using System.Windows;
using Geomo.ArcGisExtension;
using System.Windows.Input;

namespace Geomo.ShapeFileClipper
{

    /// <summary>
    /// Interaction logic for SelectReferenceSystemWindow.xaml
    /// </summary>
    public partial class CoordinateSystemSelectionWindow : ArcGIS.Desktop.Framework.Controls.ProWindow, ICoordinateSystemSelectionWindow
    {
        private ICollectionView _treeViewItems;

        public event ICoordinateSystemSelectionWindow.CoordinateSystemSelectorWindowEventHandler CoordinateSystemChanged;
        public CoordinateSystemSelection Selection { get; private set; }

        public CoordinateSystemSelectionWindow()
        {
            InitializeComponent();
            InitCoordinateSystemTreeView();
        }

        private async void InitCoordinateSystemTreeView()
        {
            _treeViewItems = await CoordinateSystemTreeBuilder.Build(
                CoordinateSystemFilter.GeographicCoordinateSystem | CoordinateSystemFilter.ProjectedCoordinateSystem);
            CoordinateSystemTree.ItemsSource = _treeViewItems;
        }

        private void OnClosing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            this.Visibility = Visibility.Hidden;
        }

        private void OnSelect(object sender, RoutedEventArgs e)
        {
            var _spatialReference = (CoordinateSystemListEntry)((CoordinateSystemItem)CoordinateSystemTree?.SelectedItem)?.NodeObject;
            CoordinateSystemChanged?.Invoke(this, new CoordinateSystemSelectorWindowEventArgs()
            {
                Selection = new CoordinateSystemSelection()
                {
                    Wkid = _spatialReference?.Wkid,
                    Name = _spatialReference?.Name,
                    SpatialReference = _spatialReference
                }
            });
            Close();
        }

        private void OnCancel(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OnClear(object sender, RoutedEventArgs e)
        {
            var selectedNode = CoordinateSystemTree.SelectedItem;
            SearchTextBox.Text = "";
            foreach (var node in _treeViewItems)
            {
                (node as CoordinateSystemCategory)?.ClearFilter();
            }
            OnSelectedCoordinateSystemChanged(this, new RoutedPropertyChangedEventArgs<object>(selectedNode, CoordinateSystemTree.SelectedItem));
        }

        private void OnSearch(object sender, RoutedEventArgs e)
        {
            var selectedNode = CoordinateSystemTree.SelectedItem;
            foreach (var node in _treeViewItems)
            {
                ((CoordinateSystemCategory)node).Filter = SearchTextBox.Text;
            }
            OnSelectedCoordinateSystemChanged(this, new RoutedPropertyChangedEventArgs<object>(selectedNode, CoordinateSystemTree.SelectedItem));
        }

        private void OnSelectedCoordinateSystemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            SelectButton.IsEnabled = CoordinateSystemTree.SelectedItem is CoordinateSystemItem;
        }

        private void OnEnter(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    OnSearch(sender, new RoutedEventArgs());
                    e.Handled = true;
                    break;
                case Key.Escape:
                    OnClear(sender, new RoutedEventArgs());
                    e.Handled = true;
                    break;
                default:
                    break;
            }
            
        }
    }
}

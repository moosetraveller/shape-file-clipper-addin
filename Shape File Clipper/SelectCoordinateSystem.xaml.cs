using ArcGIS.Core.Geometry;
using System.ComponentModel;
using System.Windows;

using Geomo.ArcGisExtension;
using System;
using System.Windows.Input;

namespace Geomo.ShapeFileClipper
{
    public class CoordinateSystemWindowEventArgs : EventArgs
    {
        public CoordinateSystemItem CoordinateSystem { get; set; }
    }

    /// <summary>
    /// Interaction logic for SelectReferenceSystemWindow.xaml
    /// </summary>
    public partial class SelectCoordinateSystem : ArcGIS.Desktop.Framework.Controls.ProWindow
    {

        private ICollectionView _treeViewItems;

        public delegate void CoordinateSystemWindowEventHandler(object source, CoordinateSystemWindowEventArgs args);
        public event CoordinateSystemWindowEventHandler CoordinateSystemChanged;

        public SelectCoordinateSystem()
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
            CoordinateSystemChanged?.Invoke(this, new CoordinateSystemWindowEventArgs()
            {
                CoordinateSystem = (CoordinateSystemItem)CoordinateSystemTree.SelectedItem
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
                ((CoordinateSystemCategory)node).ResetFilter();
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

using ArcGIS.Core.Geometry;
using System.ComponentModel;
using System.Windows;

namespace Geomo.ShapeFileClipper
{
    /// <summary>
    /// Interaction logic for SelectReferenceSystemWindow.xaml
    /// </summary>
    public partial class SelectReferenceSystem : ArcGIS.Desktop.Framework.Controls.ProWindow
    {

        private ICollectionView _treeViewItems;

        public SelectReferenceSystem()
        {
            InitializeComponent();
            InitCoordinateSystemTreeView();
        }

        private async void InitCoordinateSystemTreeView()
        {
            _treeViewItems = await CoordinateSystemCategoryItem.CreateCoordinateSystemTreeView(
                CoordinateSystemFilter.GeographicCoordinateSystem | CoordinateSystemFilter.ProjectedCoordinateSystem);
            CoordinateSystemTree.ItemsSource = _treeViewItems;
        }

        private void OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Visibility = Visibility.Hidden;
        }

    }
}

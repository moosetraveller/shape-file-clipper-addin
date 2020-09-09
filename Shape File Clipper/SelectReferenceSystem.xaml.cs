using ArcGIS.Core.Geometry;
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

namespace Geomo.ShapeFileClipper
{
    /// <summary>
    /// Interaction logic for SelectReferenceSystemWindow.xaml
    /// </summary>
    public partial class SelectReferenceSystem : ArcGIS.Desktop.Framework.Controls.ProWindow
    {

        private List<TreeNode> _treeViewItems;

        public SelectReferenceSystem()
        {
            InitializeComponent();
            InitCoordinateSystemTreeView();
        }

        private async void InitCoordinateSystemTreeView()
        {
            _treeViewItems = await CoordinateSystemCategoryItem.CreateTreeView(
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

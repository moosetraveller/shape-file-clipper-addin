using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework.Controls;
using ArcGIS.Desktop.Mapping.Controls;
using System.ComponentModel;
using System.Windows;

namespace Geomo.ShapeFileClipper
{
    /// <summary>
    /// Interaction logic for ArcCoordinateSystemSelectionWindow.xaml
    /// </summary>
    public partial class ArcCoordinateSystemSelectionWindow : ProWindow, ICoordinateSystemSelectionWindow
    {
        public event ICoordinateSystemSelectionWindow.CoordinateSystemSelectorWindowEventHandler CoordinateSystemChanged;

        public CoordinateSystemSelection Selection { get; private set; }

        public ArcCoordinateSystemSelectionWindow()
        {
            InitializeComponent();
            CoordinateSystemChanged += (source, e) => Selection = e.Selection;
            CoordinateSystemSelector.SelectedSpatialReferenceChanged += OnSelectedSpatialReferenceChanged;
        }

        public new bool? ShowDialog()
        {
            // TODO find a better way to reset the control
            CoordinateSystemSelector.ConfigureControl = new CoordinateSystemsControlProperties()
            {
                SpatialReference = (SpatialReference)Selection?.SpatialReference
            };
            return base.ShowDialog();
        }

        private void OnSelectedSpatialReferenceChanged(object sender, SpatialReferenceChangedEventArgs e)
        {
            SelectButton.IsEnabled = e.SpatialReference != null;
        }

        private void OnClosing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            this.Visibility = Visibility.Hidden;
        }

        private void OnSelectButtonClick(object sender, RoutedEventArgs e)
        {
            var spatialReference = CoordinateSystemSelector.SelectedSpatialReference;
            CoordinateSystemChanged?.Invoke(this, new CoordinateSystemSelectorWindowEventArgs()
            {
                Selection = new CoordinateSystemSelection()
                {
                    Wkid = spatialReference.Wkid,
                    Name = spatialReference.Name.Replace('_', ' '),
                    SpatialReference = spatialReference
                }
            });
            Close();
        }

        private void OnCancelButtonClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}

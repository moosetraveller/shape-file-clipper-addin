using ArcGIS.Core.Geometry;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;

namespace Geomo.ShapeFileClipper
{
    public class CoordinateSystemItem : TreeNode
    {

        private CoordinateSystemListEntry _coordinateSystem;
        public CoordinateSystemListEntry CoordinateSystem
        {
            get
            {
                return _coordinateSystem;
            }
            set
            {
                _coordinateSystem = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(_coordinateSystem)));
            }
        }

        public ICollectionView Children => CollectionViewSource.GetDefaultView(new List<TreeNode>()); // cannot have siblings

        public string Name
        {
            get
            {
                return $"{CoordinateSystem.Name} [EPSG:{CoordinateSystem.Wkid}]";
            }
        }

        public CoordinateSystemItem() { }

        public event PropertyChangedEventHandler PropertyChanged;

    }
}

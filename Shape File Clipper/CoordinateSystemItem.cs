using ArcGIS.Core.Geometry;
using System.Collections.Generic;
using System.ComponentModel;

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

        public List<TreeNode> Children => new List<TreeNode>(); // cannot have siblings

        public string Name
        {
            get
            {
                return CoordinateSystem.Wkid + "\t" + CoordinateSystem.Name;
            }
        }

        public CoordinateSystemItem() { }

        public event PropertyChangedEventHandler PropertyChanged;

    }
}

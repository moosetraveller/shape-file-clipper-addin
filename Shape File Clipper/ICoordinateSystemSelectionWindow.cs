using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geomo.ShapeFileClipper
{
    public class CoordinateSystemSelection
    { 
        public int? Wkid { get; set; }
        public string Name { get; set; }
        public object SpatialReference { get; set; }

        public override string ToString()
        {
            return $"{Name} [EPSG:{Wkid}]";
        }
    }

    public class CoordinateSystemSelectorWindowEventArgs : EventArgs
    {
        public CoordinateSystemSelection Selection { get; set; }
    }

    public interface ICoordinateSystemSelectionWindow 
    {
        delegate void CoordinateSystemSelectorWindowEventHandler(object source, CoordinateSystemSelectorWindowEventArgs args);
        event CoordinateSystemSelectorWindowEventHandler CoordinateSystemChanged;

        bool? ShowDialog();

        CoordinateSystemSelection Selection { get;  }
    }
}

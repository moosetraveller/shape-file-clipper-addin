using ArcGIS.Core.Geometry;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Geomo.ShapeFileClipper
{
    public class CoordinateSystemItem : TreeViewItem
    {

        public CoordinateSystemListEntry CoordinateSystem { get; private set; }

        public CoordinateSystemItem(CoordinateSystemListEntry coordinateSystem)
        {
            CoordinateSystem = coordinateSystem;
            Header = coordinateSystem.Name;
        }

    }
}

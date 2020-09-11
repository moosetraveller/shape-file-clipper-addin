using ArcGIS.Core.Geometry;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Data;
using Geomo.Util;

namespace Geomo.ArcGisExtension
{
    public class CoordinateSystemItem : CoordinateSystemTreeNode
    {
        public override string Name
        {
            get
            {
                if (base.TryGetNodeObject(out CoordinateSystemListEntry cs))
                {
                    return $"{cs.Name} [EPSG:{cs.Wkid}]";
                }
                return null;
            }
        }

        public CoordinateSystemItem(CoordinateSystemCategory parent) : base(parent) { }
    }
}

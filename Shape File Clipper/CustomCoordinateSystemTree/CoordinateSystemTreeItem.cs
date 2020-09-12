using ArcGIS.Core.Geometry;

namespace Geomo.ShapeFileClipper.CustomCoordinateSystemTree
{ 
    public class CoordinateSystemItem : CoordinateSystemTreeNode
    {
        public override string Name
        {
            get
            {
                if (base.NodeObject is CoordinateSystemListEntry cs)
                {
                    return $"{cs.Name} [EPSG:{cs.Wkid}]";
                }
                return null;
            }
        }

        public CoordinateSystemItem(CoordinateSystemCategory parent) : base(parent) { }
    }
}

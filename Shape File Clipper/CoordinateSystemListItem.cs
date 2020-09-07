using ArcGIS.Core.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geomo.Util
{
    class CoordinateSystemListItem
    {

        public CoordinateSystemListEntry CoordinateSystemListEntry { get; private set; }
        public int Wkid
        {
            get
            {
                return CoordinateSystemListEntry.Wkid;
            }
        }
        public string Name
        {
            get
            {
                return CoordinateSystemListEntry.Name;
            }
        }
        public string FullName
        {
            get
            {
                return $"{Wkid} \t| {Name}";
            }
        }

        public CoordinateSystemListItem(CoordinateSystemListEntry coordinateSystemListEntry)
        {
            CoordinateSystemListEntry = coordinateSystemListEntry;
        }

        public override string ToString()
        {
            return FullName;
        }

    }
}

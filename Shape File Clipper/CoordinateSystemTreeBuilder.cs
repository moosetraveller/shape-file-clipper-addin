using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Geomo.ArcGisExtension
{
    class CoordinateSystemTreeBuilder
    {
        public async static Task<ICollectionView> Build(CoordinateSystemFilter filter)
        {
            var root = new CoordinateSystemCategory(null)
            {
                NodeObject = "root"
            };
            var coordinateSystems = await QueuedTask.Run(() =>
            {
                return GeometryEngine.Instance.GetPredefinedCoordinateSystemList(filter);
            });
            foreach (var coordinateSystem in coordinateSystems)
            {
                root.Add(coordinateSystem.Category.Split('/'), coordinateSystem);
            }
            return root.Children; // ignoring root node
        }
    }
}

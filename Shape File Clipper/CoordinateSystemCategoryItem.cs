using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Geomo.ShapeFileClipper
{
    public class CoordinateSystemCategoryItem : TreeViewItem
    {
        public IEnumerable<CoordinateSystemItem> CoordinateSystems
        {
            get
            {
                return Items.SourceCollection
                    .OfType<object>()
                    .Where(o => o.GetType() == typeof(CoordinateSystemItem))
                    .OfType<CoordinateSystemItem>();
            }
        }

        public IEnumerable<CoordinateSystemCategoryItem> Categories
        {
            get
            {
                return Items.SourceCollection
                    .OfType<object>()
                    .Where(o => o.GetType() == typeof(CoordinateSystemCategoryItem))
                    .OfType<CoordinateSystemCategoryItem>();
            }
        }

        private CoordinateSystemCategoryItem(string header)
        {
            Header = header;
        }

        public async static Task<ItemCollection> CreateTreeView(CoordinateSystemFilter filter)
        {
            var root = new CoordinateSystemCategoryItem("root");
            var coordinateSystems = await QueuedTask.Run(() =>
            {
                return GeometryEngine.Instance.GetPredefinedCoordinateSystemList(filter);
            });
            foreach (var coordinateSystem in coordinateSystems)
            {
                root.Add(coordinateSystem.Category.Split('/'), coordinateSystem);
            }
            return root.Items;
        }

        private void Add(string[] categories, CoordinateSystemListEntry coordinateSystem)
        {
            if (categories.Count() == 0)
            {
                AddItem(categories, coordinateSystem);
            }
            else
            {
                AddCategory(categories, coordinateSystem);
            }
        }

        private void AddItem(string[] categories, CoordinateSystemListEntry coordinateSystem)
        {
            if (CoordinateSystems.Any(i => (string)i.Header == coordinateSystem.Name))
            {
                return; // already added
            }
            Items.Add(new CoordinateSystemItem(coordinateSystem));
        }

        private void AddCategory(string[] categories, CoordinateSystemListEntry coordinateSystem)
        {
            var category = Categories.FirstOrDefault(i => (string)i.Header == categories[0]);
            if (category == null)
            {
                category = new CoordinateSystemCategoryItem(categories[0]);
                Items.Add(category);
            }
            category.Add(categories.Skip(1).ToArray(), coordinateSystem);
        }

    }
}

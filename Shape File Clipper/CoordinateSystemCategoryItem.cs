using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Geomo.ShapeFileClipper
{
    public class CoordinateSystemCategoryItem : TreeNode
    {

        public ObservableCollection<CoordinateSystemItem> Items { get; private set; }
        public ObservableCollection<CoordinateSystemCategoryItem> Categories { get; private set; }

        public List<TreeNode> Children => Categories.Select(i => (TreeNode)i).Concat(Items.Select(c => (TreeNode)c)).ToList();

        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(_name)));
            }
        }

        public CoordinateSystemCategoryItem()
        {
            Items = new ObservableCollection<CoordinateSystemItem>();
            Categories = new ObservableCollection<CoordinateSystemCategoryItem>();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public async static Task<List<TreeNode>> CreateTreeView(CoordinateSystemFilter filter)
        {
            var root = new CoordinateSystemCategoryItem()
            {
                Name = "root"
            };
            var coordinateSystems = await QueuedTask.Run(() =>
            {
                return GeometryEngine.Instance.GetPredefinedCoordinateSystemList(filter);
            });
            foreach (var coordinateSystem in coordinateSystems)
            {
                root.Add(coordinateSystem.Category.Split('/'), coordinateSystem);
            }
            return root.Children;
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
            if (Items.Any(i => (string)i.CoordinateSystem.Name == coordinateSystem.Name))
            {
                return; // already added
            }
            Items.Add(new CoordinateSystemItem()
            { 
                CoordinateSystem = coordinateSystem
            });
        }

        private void AddCategory(string[] categories, CoordinateSystemListEntry coordinateSystem)
        {
            var category = Categories.FirstOrDefault(i => (string)i.Name == categories[0]);
            if (category == null)
            {
                category = new CoordinateSystemCategoryItem()
                {
                    Name = categories[0]
                };
                Categories.Add(category);
            }
            category.Add(categories.Skip(1).ToArray(), coordinateSystem);
        }

    }
}

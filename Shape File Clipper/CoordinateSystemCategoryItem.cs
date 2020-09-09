using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Geomo.ShapeFileClipper
{
    public class CoordinateSystemCategoryItem : TreeNode
    {

        public IEnumerable<CoordinateSystemItem> Items
        {
            get
            {
                return Children.SourceCollection
                    .OfType<TreeNode>()
                    .Where(o => o.GetType() == typeof(CoordinateSystemItem))
                    .OfType<CoordinateSystemItem>();
            }
        }

        public IEnumerable<CoordinateSystemCategoryItem> Categories
        {
            get
            {
                return Children.SourceCollection
                    .OfType<TreeNode>()
                    .Where(o => o.GetType() == typeof(CoordinateSystemCategoryItem))
                    .OfType<CoordinateSystemCategoryItem>();
            }
        }
        private ObservableCollection<TreeNode> _children;
        public ICollectionView Children { get; private set; }

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

        private IComparer<TreeNode> _treeNodeComparer = new CoordinateSystemSorter();
        public IComparer<TreeNode> TreeNodeComparer
        {
            get
            {
                return _treeNodeComparer;
            }
            set
            {
                if (value == null)
                {
                    throw new NullReferenceException($"Property {nameof(TreeNodeComparer)} cannot be null.");
                }
                _treeNodeComparer = value;
            }
        }

        public CoordinateSystemCategoryItem()
        {
            _children = new ObservableCollection<TreeNode>();
            Children = CollectionViewSource.GetDefaultView(_children);
            // Children.SortDescriptions.Add(new SortDescription(nameof(Name), ListSortDirection.Ascending));
            ((ListCollectionView)Children).CustomSort = new ForwardToDelegateComparer()
            {
                Delegate = (x, y) => TreeNodeComparer.Compare((TreeNode)x, (TreeNode)y)
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public async static Task<ICollectionView> CreateCoordinateSystemTreeView(CoordinateSystemFilter filter)
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
            return root.Children; // ignoring root node
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
            _children.Add(new CoordinateSystemItem()
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
                _children.Add(category);
            }
            category.Add(categories.Skip(1).ToArray(), coordinateSystem);
        }

    }
}

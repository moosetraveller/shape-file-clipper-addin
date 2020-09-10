using ArcGIS.Core.Geometry;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Data;

namespace Geomo.ArcGisExtension 
{
    public class CoordinateSystemCategoryItem : CoordinateSystemTreeNode
    {

        public IEnumerable<CoordinateSystemItem> Items
        {
            get
            {
                return Children.SourceCollection
                    .OfType<CoordinateSystemTreeNode>()
                    .Where(o => o.GetType() == typeof(CoordinateSystemItem))
                    .OfType<CoordinateSystemItem>();
            }
        }

        public IEnumerable<CoordinateSystemCategoryItem> Categories
        {
            get
            {
                return Children.SourceCollection
                    .OfType<CoordinateSystemTreeNode>()
                    .Where(o => o.GetType() == typeof(CoordinateSystemCategoryItem))
                    .OfType<CoordinateSystemCategoryItem>();
            }
        }

        private ObservableCollection<CoordinateSystemTreeNode> _children;

        public CoordinateSystemCategoryItem()
        {
            _children = new ObservableCollection<CoordinateSystemTreeNode>();
            Children = CollectionViewSource.GetDefaultView(_children);
            ((ListCollectionView)Children).CustomSort = CoordinateSystemSorter.NonGenericComparer;
        }

        public void Add(string[] categories, CoordinateSystemListEntry coordinateSystem)
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
            if (Items.Any(i => {
                if (i.TryGetNodeObject(out CoordinateSystemListEntry entry)) {
                    return entry.Name == coordinateSystem.Name;
                }
                return false;
            }))
            {
                return; // already added
            }
            _children.Add(new CoordinateSystemItem()
            {
                NodeObject = coordinateSystem
            });
        }

        private void AddCategory(string[] categories, CoordinateSystemListEntry coordinateSystem)
        {
            var category = Categories.FirstOrDefault(i => (string)i.Name == categories[0]);
            if (category == null)
            {
                category = new CoordinateSystemCategoryItem()
                {
                    NodeObject = categories[0]
                };
                _children.Add(category);
            }
            category.Add(categories.Skip(1).ToArray(), coordinateSystem);
        }

    }
}

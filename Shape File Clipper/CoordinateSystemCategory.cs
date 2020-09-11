using ArcGIS.Core.Geometry;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Windows.Data;

namespace Geomo.ArcGisExtension
{
    public class CoordinateSystemCategory : CoordinateSystemTreeNode
    {

        public IEnumerable<CoordinateSystemItem> Items
        {
            get
            {
                return Children.SourceCollection
                    .OfType<object>()
                    .Where(o => o.GetType() == typeof(CoordinateSystemItem))
                    .OfType<CoordinateSystemItem>();
            }
        }

        public IEnumerable<CoordinateSystemCategory> Categories
        {
            get
            {
                return Children.SourceCollection
                    .OfType<object>()
                    .Where(o => o.GetType() == typeof(CoordinateSystemCategory))
                    .OfType<CoordinateSystemCategory>();
            }
        }

        private string _filter;
        public string Filter
        {
            get { return _filter; }
            set
            {
                _filter = value;
                foreach (CoordinateSystemCategory category in Categories)
                {
                    category.Filter = Filter;
                }
                ((ListCollectionView)Children).Refresh();
            }
        }

        private ObservableCollection<CoordinateSystemTreeNode> _children;

        public CoordinateSystemCategory(CoordinateSystemCategory parent) : base(parent)
        {
            _children = new ObservableCollection<CoordinateSystemTreeNode>();
            
            Children = CollectionViewSource.GetDefaultView(_children);

            ((ListCollectionView)Children).CustomSort = CoordinateSystemComparer.NonGeneric;
            ((ListCollectionView)Children).Filter = (o) =>
            {
                if (o is CoordinateSystemTreeNode node)
                {
                    if (node.Parent == null)
                    {
                        return true;
                    }
                    if (IsNoFilter(Filter))
                    {
                        return true;
                    }
                    return node.Matches(Filter) || node.HasChildMatching(Filter);
                }
                return false;
            };
        }

        public void ResetFilter()
        {
            Filter = null;
        }

        public void Add(string[] categories, CoordinateSystemListEntry coordinateSystem)
        {
            if (categories.Count() == 0)
            {
                AddItem(coordinateSystem);
            }
            else
            {
                AddCategory(categories, coordinateSystem);
            }
        }

        private void AddItem(CoordinateSystemListEntry coordinateSystem)
        {
            if (Items.Any(i =>
            {
                if (i.TryGetNodeObject(out CoordinateSystemListEntry entry))
                {
                    return entry.Name == coordinateSystem.Name;
                }
                return false;
            }))
            {
                return; // already added
            }
            _children.Add(new CoordinateSystemItem(this)
            {
                NodeObject = coordinateSystem
            });
        }

        private void AddCategory(string[] categories, CoordinateSystemListEntry coordinateSystem)
        {
            var category = Categories.FirstOrDefault(i => (string)i.Name == categories[0]);
            if (category == null)
            {
                category = new CoordinateSystemCategory(this)
                {
                    NodeObject = categories[0]
                };
                _children.Add(category);
            }
            category.Add(categories.Skip(1).ToArray(), coordinateSystem);
        }

    }
}

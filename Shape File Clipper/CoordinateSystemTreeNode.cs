using Geomo.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Policy;
using System.Windows.Data;

namespace Geomo.ArcGisExtension
{
    /// <summary>
    /// Represents either a category or a coordinate system. CoordinateSystemTreeNode is purposely 
    /// non-generic to allow different types of node objects.
    /// </summary>
    public abstract class CoordinateSystemTreeNode : TreeNode
    {
        private object _nodeObject;
        public object NodeObject
        {
            // use TryGetNodeObject<T> instead
            private get { return _nodeObject; }
            set
            {
                _nodeObject = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NodeObject)));
            }
        }

        public bool TryGetNodeObject<T>(out T nodeObject)
        {
            if (NodeObject.GetType() == typeof(T))
            {
                nodeObject = (T)NodeObject;
                return true;
            }
            nodeObject = default;
            return false;
        }

        public virtual string Name
        {
            get
            {
                return _nodeObject?.ToString();
            }
        }

        public ICollectionView Children { get; protected set; } = CollectionViewSource.GetDefaultView(new List<CoordinateSystemTreeNode>());

        private CoordinateSystemTreeNode _parent;
        public CoordinateSystemTreeNode Parent
        {
            get
            {
                return _parent;
            }
            set
            {
                var children = _parent?.Children as ListCollectionView;
                children?.Remove(_parent);
                _parent = value;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public CoordinateSystemTreeNode(CoordinateSystemTreeNode parent)
        {
            Parent = parent;
        }

        public virtual bool IsNoFilter(string filter)
        {
            return filter == null || filter.Trim().Length == 0;
        }

        public virtual bool Matches(string filter)
        {
            return GetType() == typeof(CoordinateSystemItem) && Name.ToLower().Contains(filter.ToLower());
        }

        public virtual bool HasChildMatching(string filter)
        {
            if (IsNoFilter(filter))
            {
                return true;
            }
            foreach (CoordinateSystemTreeNode node in Children)
            {
                if (node.Matches(filter) || node.HasChildMatching(filter))
                {
                    return true;
                }
            }
            return Matches(filter);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
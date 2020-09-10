using Geomo.Util;
using System.Collections.Generic;
using System.ComponentModel;
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

        private string _filter;
        public string Filter
        {
            get { return _filter; }
            set
            {
                _filter = value;
                foreach (CoordinateSystemTreeNode childNode in Children)
                {
                    childNode.Filter = Filter;
                }
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Filter)));
            }
        }

        public void ResetFilter()
        {
            Filter = null;
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
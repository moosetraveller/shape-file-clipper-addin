using Geomo.ShapeFileClipper.Utils;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Data;

namespace Geomo.ShapeFileClipper.CustomCoordinateSystemTree
{
    /// <summary>
    /// Represents either a category or a coordinate system. CoordinateSystemTreeNode is purposely 
    /// non-generic to allow different types of node objects.
    /// </summary>
    public abstract class CoordinateSystemTreeNode : ITreeNode
    {
        private object _nodeObject;
        public object NodeObject
        {
            get { return _nodeObject; }
            set
            {
                _nodeObject = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NodeObject)));
            }
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
using System.Collections;
using System.Collections.Generic;
using Geomo.ShapeFileClipper.Utils;

namespace Geomo.ShapeFileClipper.CustomCoordinateSystemTree
{
    class CoordinateSystemComparer : IComparer<ITreeNode>
    {
        public static IComparer<ITreeNode> Generic { get; } = new CoordinateSystemComparer();
        public static IComparer NonGeneric { get; } = new ForwardToDelegateComparer((x, y) => Generic.Compare((ITreeNode)x, (ITreeNode)y));

        private CoordinateSystemComparer() { }

        public int Compare(ITreeNode t1, ITreeNode t2)
        {
            if (t1.GetType() == t2.GetType())
            {
                return t1.Name.CompareTo(t2.Name);
            }
            return t1.GetType() == typeof(CoordinateSystemCategory) ? -1 : 1;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using Geomo.Util;

namespace Geomo.ArcGisExtension
{
    class CoordinateSystemComparer : IComparer<TreeNode>
    {
        public static IComparer<TreeNode> Generic { get; } = new CoordinateSystemComparer();
        public static IComparer NonGeneric { get; } = new ForwardToDelegateComparer((x, y) => Generic.Compare((TreeNode)x, (TreeNode)y));

        private CoordinateSystemComparer() { }

        public int Compare(TreeNode t1, TreeNode t2)
        {
            if (t1.GetType() == t2.GetType())
            {
                return t1.Name.CompareTo(t2.Name);
            }
            return t1.GetType() == typeof(CoordinateSystemCategory) ? -1 : 1;
        }
    }
}

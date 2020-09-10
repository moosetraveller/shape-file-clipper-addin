using System.Collections;
using System.Collections.Generic;
using Geomo.Util;

namespace Geomo.ArcGisExtension
{
    class CoordinateSystemSorter : IComparer<TreeNode>
    {
        public static IComparer<TreeNode> Comparer { get; } = new CoordinateSystemSorter();
        public static IComparer NonGenericComparer { get; } = new ForwardToDelegateComparer((x, y) => Comparer.Compare((TreeNode)x, (TreeNode)y));

        private CoordinateSystemSorter() { }

        public int Compare(TreeNode t1, TreeNode t2)
        {
            if (t1.GetType() == t2.GetType())
            {
                return t1.Name.CompareTo(t2.Name);
            }
            return t1.GetType() == typeof(CoordinateSystemCategoryItem) ? -1 : 1;
        }
    }
}

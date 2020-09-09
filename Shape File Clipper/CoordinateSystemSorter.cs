
using System;
using System.Collections;
using System.Collections.Generic;

namespace Geomo.ShapeFileClipper
{
    class CoordinateSystemSorter : IComparer<TreeNode>
    {
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

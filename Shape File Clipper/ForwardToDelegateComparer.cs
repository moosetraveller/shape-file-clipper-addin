using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geomo.ShapeFileClipper
{

    delegate int CompareDelegate(object x, object y);

    class ForwardToDelegateComparer : IComparer
    {
        public CompareDelegate Delegate { get; set; }

        public int Compare(object x, object y)
        {
            if (Delegate == null)
            {
                throw new NullReferenceException($"Property {nameof(Delegate)} must not be null.");
            }
            return Delegate(x, y);
        }
    }
}

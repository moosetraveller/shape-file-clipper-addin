using System;
using System.Collections;

namespace Geomo.Util
{

    delegate int CompareDelegate(object x, object y);

    class ForwardToDelegateComparer : IComparer
    {
        public CompareDelegate Delegate { get; private set; }

        public ForwardToDelegateComparer(CompareDelegate compareDelegate) {
            Delegate = compareDelegate;
        }

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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geomo.ShapeFileClipper
{
    public interface TreeNode : INotifyPropertyChanged
    {
        String Name { get; }

        ICollectionView Children { get; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geomo.ShapeFileClipper
{
    public interface TreeNode : INotifyPropertyChanged
    {
        String Name { get; }

        List<TreeNode> Children { get; }
    }
}

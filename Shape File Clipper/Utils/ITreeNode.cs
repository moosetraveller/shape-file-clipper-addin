using System.ComponentModel;

namespace Geomo.ShapeFileClipper.Utils
{
    public interface ITreeNode : INotifyPropertyChanged
    {
        string Name { get; }

        ICollectionView Children { get; }
    }
}

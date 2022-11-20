using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WpfToolkit.Controls
{
    internal interface IItemContainerManger
    {
        bool IsRecycling { get; set; }
        
        IReadOnlyList<IArrangeable> RealizedContainers { get; }

        int ItemIndexForChildIndex(int childIndex);

        Size RealizeItem(int itemIndex, int childIndex, Size availableSize);

        void VirtualizeItem(int childIndex);

    }
}

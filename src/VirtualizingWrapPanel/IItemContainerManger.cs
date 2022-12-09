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
        
        IReadOnlyList<IItemContainerInfo> RealizedContainers { get; }

#if NET6_0_OR_GREATER
        IReadOnlySet<IItemContainerInfo> CachedContainers { get; }
#else
        IEnumerable<IItemContainerInfo> CachedContainers { get; }
#endif

        int ItemIndexForChildIndex(int childIndex);

        Size RealizeItem(int itemIndex, int childIndex, Size availableSize);

        void VirtualizeItem(int childIndex);

    }
}

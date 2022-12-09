using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace WpfToolkit.Controls;

internal class ItemContainerManager : IItemContainerManger
{
    public bool IsRecycling { get; set; }

    private readonly IVirtualizingPanelWrapper virtualizingPanel;

    public IReadOnlyList<IItemContainerInfo> RealizedContainers => realizedContainers;

#if NET6_0_OR_GREATER
    public IReadOnlySet<IItemContainerInfo> CachedContainers => cachedContainers;
#else
    public IEnumerable<IItemContainerInfo> CachedContainers => cachedContainers;
#endif

    private readonly List<IItemContainerInfo> realizedContainers = new List<IItemContainerInfo>();

    private readonly HashSet<IItemContainerInfo> cachedContainers = new HashSet<IItemContainerInfo>();

    public ItemContainerManager(IVirtualizingPanelWrapper virtualizingPanel)
    {
        this.virtualizingPanel = virtualizingPanel;
    }

    public Size RealizeItem(int itemIndex, int childIndex, Size availableSize)
    {
        var itemContainerGenerator = virtualizingPanel.ItemContainerGenerator;
        var startPosition = itemContainerGenerator.GeneratorPositionFromIndex(itemIndex);
        using (itemContainerGenerator.StartAt(startPosition, GeneratorDirection.Forward, true))
        {
            var container = (UIElement)itemContainerGenerator.GenerateNext(out bool isNewlyRealized);

            if (!virtualizingPanel.ContainsInternalChild(container))
            {
                if (childIndex >= virtualizingPanel.InternalChildrenCount)
                {
                    virtualizingPanel.AddInternalChild(container);
                }
                else
                {
                    virtualizingPanel.InsertInternalChild(childIndex, container);
                }
            }

            var containerInfo = ItemContainerInfo.For(container);
            if (!realizedContainers.Contains(containerInfo))
            {
                cachedContainers.Remove(containerInfo);

                if (childIndex >= realizedContainers.Count)
                {
                    realizedContainers.Add(containerInfo);
                }
                else
                {
                    realizedContainers.Insert(childIndex, containerInfo);
                }
            }

            itemContainerGenerator.PrepareItemContainer(container);
            container.Measure(availableSize);
            return container.DesiredSize;
        }
    }

    public void VirtualizeItem(int childIndex)
    {
        var itemContainerGenerator = virtualizingPanel.ItemContainerGenerator;
        var generatorPosition = new GeneratorPosition(childIndex, 0);

        if (IsRecycling)
        {
            itemContainerGenerator.Recycle(generatorPosition, 1);
            cachedContainers.Add(realizedContainers[childIndex]);
        }
        else
        {
            itemContainerGenerator.Remove(generatorPosition, 1);
            virtualizingPanel.RemoveInternalChildAt(childIndex);
        }

        realizedContainers.RemoveAt(childIndex);
    }

    public int ItemIndexForChildIndex(int childIndex)
    {
        var itemContainerGenerator = virtualizingPanel.ItemContainerGenerator;
        return itemContainerGenerator.IndexFromGeneratorPosition(new GeneratorPosition(childIndex, 0));
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Moq;
using WpfToolkit.Controls;

namespace VirtualizingWrapPanelTest;

internal class ItemContainerMangerMock : IItemContainerManger
{
    public bool IsRecycling { get; set; }

    IReadOnlyList<IArrangeable> IItemContainerManger.RealizedContainers => realizedContainers.Cast<IArrangeable>().ToList();

    public IReadOnlyList<ArrangableMock> RealizedContainers => realizedContainers.ToList();

    private readonly List<TestItem> items;

    private readonly IList<ArrangableMock> realizedContainers = new List<ArrangableMock>();

    public ItemContainerMangerMock(List<TestItem> items)
    {
        this.items = items;
    }

    public int ItemIndexForChildIndex(int childIndex)
    {
        return items.IndexOf(realizedContainers[childIndex].Item);
    }

    public Size RealizeItem(int itemIndex, int childIndex, Size availableSize)
    {
        var item = items[itemIndex];

        var container = realizedContainers.FirstOrDefault(c => c.Item == item);

        if (container == null)
        {
            container = new ArrangableMock(item);

            if (childIndex >= realizedContainers.Count)
            {
                realizedContainers.Add(container);
            }
            else
            {
                realizedContainers.Insert(childIndex, container);
            }
        }

        return container.DesiredSize;
    }

    public void VirtualizeItem(int childIndex)
    {
        realizedContainers.RemoveAt(childIndex);
    }

    public bool IsItemRealized(TestItem item) 
    {
        return realizedContainers.Any(c => c.Item == item);
    }
}

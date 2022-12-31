using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using Moq;
using WpfToolkit.Controls;

namespace VirtualizingWrapPanelTest.Mocks;

internal class ItemContainerMangerMock : IItemContainerManager
{
    public event EventHandler<ItemContainerManagerItemsChangedEventArgs>? ItemsChanged;

    public bool IsRecycling { get; set; } = false;

    IReadOnlySet<IItemContainerInfo> IItemContainerManager.RealizedContainers => realizedContainers.Cast<IItemContainerInfo>().ToHashSet();

    public IReadOnlySet<ItemContainerInfoMock> RealizedContainers => realizedContainers;

    IReadOnlySet<IItemContainerInfo> IItemContainerManager.CachedContainers => cachedContainers.Cast<IItemContainerInfo>().ToHashSet();

    public IReadOnlySet<ItemContainerInfoMock> CachedContainers => cachedContainers;

    public ReadOnlyCollection<object> Items => new ReadOnlyCollection<object>(items);

    private readonly List<object> items;

    private readonly HashSet<ItemContainerInfoMock> realizedContainers = new HashSet<ItemContainerInfoMock>();

    private readonly HashSet<ItemContainerInfoMock> cachedContainers = new HashSet<ItemContainerInfoMock>();

    public ItemContainerMangerMock(List<object> items)
    {
        this.items = items;
    }

    public void InvokeItemsChangedEvent(ItemContainerManagerItemsChangedEventArgs eventArgs)
    {
        ItemsChanged?.Invoke(this, eventArgs);
    }

    public IItemContainerInfo Realize(int itemIndex, out bool itemAlreadyRealized, out bool newContainerGenerated)
    {
        var item = items[itemIndex];

        var container = realizedContainers.FirstOrDefault(container => Equals(container.Item, item));

        itemAlreadyRealized = container != null;

        if (container == null)
        {
            container = new ItemContainerInfoMock((TestItem)item);
            newContainerGenerated = !IsRecycling || !cachedContainers.Any();
            if (IsRecycling && cachedContainers.Any())
            {
                cachedContainers.Remove(cachedContainers.First());
            }
            realizedContainers.Add(container);
        }
        else 
        {
            newContainerGenerated = false;
        }

        return container;
    }

    public bool Virtualize(IItemContainerInfo containerInfo)
    {
        realizedContainers.Remove((ItemContainerInfoMock)containerInfo);
        if (IsRecycling)
        {
            cachedContainers.Add((ItemContainerInfoMock)containerInfo);
        }
        return !IsRecycling;
    }

    public bool IsItemRealized(object item)
    {
        return realizedContainers.Any(container => Equals(container.Item, item));
    }

    public int FindItemIndexOfContainer(IItemContainerInfo containerInfo)
    {
        return items.IndexOf((TestItem)containerInfo.Item);
    }
}

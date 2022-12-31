using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace WpfToolkit.Controls;

internal class ItemContainerManagerItemsChangedEventArgs
{
    public NotifyCollectionChangedAction Action { get; }
    public IReadOnlyCollection<IItemContainerInfo> RemovedContainers { get; }

    public ItemContainerManagerItemsChangedEventArgs(
        NotifyCollectionChangedAction action,
        IReadOnlyCollection<IItemContainerInfo> removedContainers)
    {
        Action = action;
        RemovedContainers = removedContainers;
    }
}

internal interface IItemContainerManager
{
    public event EventHandler<ItemContainerManagerItemsChangedEventArgs>? ItemsChanged;

    bool IsRecycling { get; set; }

    ReadOnlyCollection<object> Items { get; }

#if NET6_0_OR_GREATER
    IReadOnlySet<IItemContainerInfo> RealizedContainers { get; }
#else
    IEnumerable<IItemContainerInfo> RealizedContainers { get; }
#endif

#if NET6_0_OR_GREATER
    IReadOnlySet<IItemContainerInfo> CachedContainers { get; }
#else
    IEnumerable<IItemContainerInfo> CachedContainers { get; }
#endif

    IItemContainerInfo Realize(int itemIndex, out bool wasAlreadyRealized, out bool newContainerGenerated);

    /// <returns>true if the container should be removed, otherwise false (container is recylced)</returns>
    bool Virtualize(IItemContainerInfo containerInfo);

    int FindItemIndexOfContainer(IItemContainerInfo containerInfo);
}

internal class ItemContainerManager : IItemContainerManager
{

    public event EventHandler<ItemContainerManagerItemsChangedEventArgs>? ItemsChanged;

    public bool IsRecycling { get; set; }

    public ReadOnlyCollection<object> Items => itemContainerGenerator.Items;

#if NET6_0_OR_GREATER
    public IReadOnlySet<IItemContainerInfo> RealizedContainers => realizedContainers;
#else
    public IEnumerable<IItemContainerInfo> RealizedContainers => realizedContainers;
#endif

#if NET6_0_OR_GREATER
    public IReadOnlySet<IItemContainerInfo> CachedContainers => cachedContainers;
#else
    public IEnumerable<IItemContainerInfo> CachedContainers => cachedContainers;
#endif

    private readonly HashSet<IItemContainerInfo> realizedContainers = new HashSet<IItemContainerInfo>();

    private readonly HashSet<IItemContainerInfo> cachedContainers = new HashSet<IItemContainerInfo>();

    private readonly ItemContainerGenerator itemContainerGenerator;

    private readonly IRecyclingItemContainerGenerator recyclingItemContainerGenerator;

    public ItemContainerManager(ItemContainerGenerator itemContainerGenerator)
    {
        this.itemContainerGenerator = itemContainerGenerator;
        this.recyclingItemContainerGenerator = itemContainerGenerator;
        itemContainerGenerator.ItemsChanged += ItemContainerGenerator_ItemsChanged;
    }

    private void ItemContainerGenerator_ItemsChanged(object sender, ItemsChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Reset)
        {
            realizedContainers.Clear();
            cachedContainers.Clear();
        }

        if (e.Action == NotifyCollectionChangedAction.Remove
            || e.Action == NotifyCollectionChangedAction.Replace)
        {
            var removedCotainers = realizedContainers.Where(container => !Items.Contains(container.Item)).ToList();
            removedCotainers.ForEach(container => realizedContainers.Remove(container));
            if (IsRecycling)
            {
                removedCotainers.ForEach(container => cachedContainers.Add(container));
            }
            ItemsChanged?.Invoke(this, new ItemContainerManagerItemsChangedEventArgs(e.Action, removedCotainers));
        }
        else
        {
            ItemsChanged?.Invoke(this, new ItemContainerManagerItemsChangedEventArgs(e.Action, new IItemContainerInfo[0]));
        }
    }

    public IItemContainerInfo Realize(int itemIndex, out bool wasAlreadyRealized, out bool newContainerGenerated)
    {
        var item = Items[itemIndex];

        if (RealizedContainers.FirstOrDefault(container => container.Item == item) is { } containerInfo)
        {
            wasAlreadyRealized = true;
            newContainerGenerated = false;
            return containerInfo;
        }

        wasAlreadyRealized = false;
        var generatorPosition = recyclingItemContainerGenerator.GeneratorPositionFromIndex(itemIndex);
        using (recyclingItemContainerGenerator.StartAt(generatorPosition, GeneratorDirection.Forward))
        {
            var container = recyclingItemContainerGenerator.GenerateNext(out newContainerGenerated);
            recyclingItemContainerGenerator.PrepareItemContainer(container);
            containerInfo = ItemContainerInfo.For((UIElement)container, item);
            cachedContainers.Remove(containerInfo);
            realizedContainers.Add(containerInfo);
            return containerInfo;
        }
    }

    public bool Virtualize(IItemContainerInfo containerInfo)
    {
        int itemIndex = FindItemIndexOfContainer(containerInfo);

        if (itemIndex == -1)
        {
            Debug.WriteLine("Virtualize no more existing item");
            realizedContainers.Remove(containerInfo);
            return true;
        }

        var generatorPosition = recyclingItemContainerGenerator.GeneratorPositionFromIndex(itemIndex);

        if (IsRecycling)
        {
            recyclingItemContainerGenerator.Recycle(generatorPosition, 1);
            realizedContainers.Remove(containerInfo);
            cachedContainers.Add(containerInfo);
            return false;
        }
        else
        {
            recyclingItemContainerGenerator.Remove(generatorPosition, 1);
            realizedContainers.Remove(containerInfo);
            return true;
        }
    }

    public int FindItemIndexOfContainer(IItemContainerInfo containerInfo)
    {
        return itemContainerGenerator.IndexFromContainer(containerInfo.UIElement);
    }

}

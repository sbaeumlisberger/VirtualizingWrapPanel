using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Animation;

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

    IReadOnlyCollection<IItemContainerInfo> RealizedContainers { get; }

    IReadOnlyCollection<IItemContainerInfo> CachedContainers { get; }

    /// <summary>
    /// Realizes the specified item. If the item is already realized, nothing happens.
    /// </summary>
    /// <param name="itemIndex">Index of the item to relaize</param>
    /// <param name="isNewlyRealized">Indicates whether the specified item is newly realized</param>
    /// <param name="isNewContainer">Indicates whether a new container was generated</param>
    /// <returns>A object with information about the container of the specified item</returns>
    IItemContainerInfo Realize(int itemIndex, out bool isNewlyRealized, out bool isNewContainer);

    void Virtualize(IItemContainerInfo containerInfo);

    int FindItemIndexOfContainer(IItemContainerInfo containerInfo);
}

internal class ItemContainerManager : IItemContainerManager
{

    public event EventHandler<ItemContainerManagerItemsChangedEventArgs>? ItemsChanged;

    public bool IsRecycling { get; set; }

    public ReadOnlyCollection<object> Items => itemContainerGenerator.Items;

    public IReadOnlyCollection<IItemContainerInfo> RealizedContainers => realizedContainers;

    public IReadOnlyCollection<IItemContainerInfo> CachedContainers => cachedContainers;

    private readonly HashSet<IItemContainerInfo> realizedContainers = new HashSet<IItemContainerInfo>();

    private readonly HashSet<IItemContainerInfo> cachedContainers = new HashSet<IItemContainerInfo>();

    private readonly ItemContainerGenerator itemContainerGenerator;

    private readonly IRecyclingItemContainerGenerator recyclingItemContainerGenerator;

    private readonly Action<UIElement> addInternalChild;
    private readonly Action<UIElement> removeInternalChild;

    public ItemContainerManager(ItemContainerGenerator itemContainerGenerator, Action<UIElement> addInternalChild, Action<UIElement> removeInternalChild)
    {
        this.itemContainerGenerator = itemContainerGenerator;
        this.recyclingItemContainerGenerator = itemContainerGenerator;
        this.addInternalChild = addInternalChild;
        this.removeInternalChild = removeInternalChild;
        itemContainerGenerator.ItemsChanged += ItemContainerGenerator_ItemsChanged;
    }

    private void ItemContainerGenerator_ItemsChanged(object sender, ItemsChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Reset)
        {
            realizedContainers.Clear();
            cachedContainers.Clear();
            // childrenCollection is cleared automatically

            ItemsChanged?.Invoke(this, new ItemContainerManagerItemsChangedEventArgs(e.Action, realizedContainers));
        }
        else if (e.Action == NotifyCollectionChangedAction.Remove
            || e.Action == NotifyCollectionChangedAction.Replace)
        {
            var removedCotainers = realizedContainers.Where(container => !Items.Contains(container.Item)).ToList();
           
            removedCotainers.ForEach(container => realizedContainers.Remove(container));
           
            if (IsRecycling)
            {
                removedCotainers.ForEach(container => cachedContainers.Add(container));
            }
            else 
            {
                removedCotainers.ForEach(container => removeInternalChild(container.UIElement));
            }

            ItemsChanged?.Invoke(this, new ItemContainerManagerItemsChangedEventArgs(e.Action, removedCotainers));
        }
        else
        {          
            ItemsChanged?.Invoke(this, new ItemContainerManagerItemsChangedEventArgs(e.Action, Array.Empty<IItemContainerInfo>()));
        }
    }

    public IItemContainerInfo Realize(int itemIndex, out bool isNewlyRealized, out bool isNewContainer)
    {
        var item = Items[itemIndex];

        if (realizedContainers.FirstOrDefault(container => container.Item == item) is { } containerInfo)
        {
            isNewlyRealized = false;
            isNewContainer = false;
            return containerInfo;
        }

        isNewlyRealized = true;
        var generatorPosition = recyclingItemContainerGenerator.GeneratorPositionFromIndex(itemIndex);
        using (recyclingItemContainerGenerator.StartAt(generatorPosition, GeneratorDirection.Forward))
        {
            var container = (UIElement)recyclingItemContainerGenerator.GenerateNext(out isNewContainer);
            recyclingItemContainerGenerator.PrepareItemContainer(container);
            containerInfo = ItemContainerInfo.For(container, item);
            cachedContainers.Remove(containerInfo);
            realizedContainers.Add(containerInfo);

            if (isNewContainer)
            {
                addInternalChild(container);
            }

            return containerInfo;
        }
    }

    public void Virtualize(IItemContainerInfo containerInfo)
    {
        int itemIndex = FindItemIndexOfContainer(containerInfo);

        if (itemIndex == -1)
        {
            Debug.WriteLine("Virtualize no more existing item");
            realizedContainers.Remove(containerInfo);
            removeInternalChild(containerInfo.UIElement);
        }

        var generatorPosition = recyclingItemContainerGenerator.GeneratorPositionFromIndex(itemIndex);

        if (IsRecycling)
        {
            recyclingItemContainerGenerator.Recycle(generatorPosition, 1);
            realizedContainers.Remove(containerInfo);
            cachedContainers.Add(containerInfo);
        }
        else
        {
            recyclingItemContainerGenerator.Remove(generatorPosition, 1);
            realizedContainers.Remove(containerInfo);
            removeInternalChild(containerInfo.UIElement);
        }
    }

    public int FindItemIndexOfContainer(IItemContainerInfo containerInfo)
    {
        return itemContainerGenerator.IndexFromContainer(containerInfo.UIElement);
    }

}

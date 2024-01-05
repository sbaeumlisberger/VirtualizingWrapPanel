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

    public ItemContainerManagerItemsChangedEventArgs(NotifyCollectionChangedAction action)
    {
        Action = action;
    }
}

internal class ItemContainerManager
{
    /// <summary>
    /// Occurs when the <see cref="Items"/> collection changes.
    /// </summary>
    public event EventHandler<ItemContainerManagerItemsChangedEventArgs>? ItemsChanged;

    /// <summary>
    /// Indicates whether containers are recycled or not.
    /// </summary>
    public bool IsRecycling { get; set; }

    /// <summary>
    /// Collection that contains the items for which containers are generated.
    /// </summary>
    public IReadOnlyList<object> Items => itemContainerGenerator.Items;

    /// <summary>
    /// Dictionary that contains the realised containers. The keys are the items, the values are the containers.
    /// </summary>
    public IReadOnlyDictionary<object, UIElement> RealizedContainers => realizedContainers;

    /// <summary>
    /// Collection that contains the cached containers. Always emtpy if <see cref="IsRecycling"/> is false.
    /// </summary>
    public IReadOnlyCollection<UIElement> CachedContainers => cachedContainers;

    private readonly Dictionary<object, UIElement> realizedContainers = new Dictionary<object, UIElement>();

    private readonly HashSet<UIElement> cachedContainers = new HashSet<UIElement>();

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
            // children collection is cleared automatically

            ItemsChanged?.Invoke(this, new ItemContainerManagerItemsChangedEventArgs(e.Action));
        }
        else if (e.Action == NotifyCollectionChangedAction.Remove
            || e.Action == NotifyCollectionChangedAction.Replace)
        {
            var (item, container) = realizedContainers.Where(entry => !Items.Contains(entry.Key)).Single();

            realizedContainers.Remove(item);

            if (IsRecycling)
            {
                cachedContainers.Add(container);
            }
            else
            {
                removeInternalChild(container);
            }

            ItemsChanged?.Invoke(this, new ItemContainerManagerItemsChangedEventArgs(e.Action));
        }
        else
        {
            ItemsChanged?.Invoke(this, new ItemContainerManagerItemsChangedEventArgs(e.Action));
        }
    }

    public UIElement Realize(int itemIndex)
    {
        var item = Items[itemIndex];

        if (realizedContainers.TryGetValue(item, out var existingContainer))
        {
            return existingContainer;
        }

        var generatorPosition = recyclingItemContainerGenerator.GeneratorPositionFromIndex(itemIndex);
        using (recyclingItemContainerGenerator.StartAt(generatorPosition, GeneratorDirection.Forward))
        {
            var container = (UIElement)recyclingItemContainerGenerator.GenerateNext(out bool isNewContainer);
            recyclingItemContainerGenerator.PrepareItemContainer(container);

            cachedContainers.Remove(container);
            realizedContainers.Add(item, container);

            if (isNewContainer)
            {
                addInternalChild(container);
            }

            return container;
        }
    }

    public void Virtualize(UIElement container)
    {
        int itemIndex = FindItemIndexOfContainer(container);

        if (itemIndex == -1) // the item is already virtualized (can happen when grouping)
        {
            realizedContainers.Remove(realizedContainers.Where(entry => entry.Value == container).Single().Key);

            if (IsRecycling)
            {
                cachedContainers.Add(container);
            }
            else
            {
                removeInternalChild(container);
            }

            return;
        }

        var item = Items[itemIndex];

        var generatorPosition = recyclingItemContainerGenerator.GeneratorPositionFromIndex(itemIndex);

        if (IsRecycling)
        {
            recyclingItemContainerGenerator.Recycle(generatorPosition, 1);
            realizedContainers.Remove(item);
            cachedContainers.Add(container);
        }
        else
        {
            recyclingItemContainerGenerator.Remove(generatorPosition, 1);
            realizedContainers.Remove(item);
            removeInternalChild(container);
        }
    }

    public int FindItemIndexOfContainer(UIElement container)
    {
        return itemContainerGenerator.IndexFromContainer(container);
    }

}

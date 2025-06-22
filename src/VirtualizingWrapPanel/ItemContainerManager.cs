using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

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
    public ReadOnlyCollection<object> Items => itemContainerGenerator.Items;

    /// <summary>
    /// Dictionary that contains the realised containers. The keys are the items, the values are the containers.
    /// </summary>
    public IReadOnlyDictionary<object, UIElement> RealizedContainers => realizedContainers;

    /// <summary>
    /// Collection that contains the cached containers. Always empty if <see cref="IsRecycling"/> is false.
    /// </summary>
    public IReadOnlyCollection<UIElement> CachedContainers => cachedContainers;

    private readonly Dictionary<object, UIElement> realizedContainers = new Dictionary<object, UIElement>(ReferenceEqualityComparer.Instance);

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

    public void OnClearChildren()
    {
        realizedContainers.Clear();
        cachedContainers.Clear();
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

            cachedContainers.Remove(container);
            realizedContainers.Add(item, container);

            if (isNewContainer)
            {
                addInternalChild(container);
            }
            else
            {
                InvalidateMeasureRecursively(container);
            }

            recyclingItemContainerGenerator.PrepareItemContainer(container);

            return container;
        }
    }

    public void Virtualize(object item)
    {
        var container = realizedContainers[item];

        var generatorPosition = GeneratorPositionFromContainer(container);

        // Index is - 1 when the item is already virtualized (can happen when grouping)
        if (generatorPosition.Index != -1)
        {
            if (IsRecycling)
            {
                recyclingItemContainerGenerator.Recycle(generatorPosition, 1);
            }
            else
            {
                recyclingItemContainerGenerator.Remove(generatorPosition, 1);
            }
        }

        realizedContainers.Remove(item);

        if (IsRecycling)
        {
            cachedContainers.Add(container);
        }
        else
        {
            removeInternalChild(container);
        }
    }

    private GeneratorPosition GeneratorPositionFromContainer(UIElement container)
    {
        int itemIndex = itemContainerGenerator.IndexFromContainer(container);
        return recyclingItemContainerGenerator.GeneratorPositionFromIndex(itemIndex);
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
            foreach (var (item, container) in realizedContainers.Where(entry => !Items.Contains(entry.Key)).ToList())
            {
                realizedContainers.Remove(item);

                if (IsRecycling)
                {
                    cachedContainers.Add(container);
                }
                else
                {
                    removeInternalChild(container);
                }
            }

            ItemsChanged?.Invoke(this, new ItemContainerManagerItemsChangedEventArgs(e.Action));
        }
        else
        {
            ItemsChanged?.Invoke(this, new ItemContainerManagerItemsChangedEventArgs(e.Action));
        }
    }

    private static void InvalidateMeasureRecursively(UIElement element)
    {
        element.InvalidateMeasure();

        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
        {
            if (VisualTreeHelper.GetChild(element, i) is UIElement child)
            {
                InvalidateMeasureRecursively(child);
            }
        }
    }
}

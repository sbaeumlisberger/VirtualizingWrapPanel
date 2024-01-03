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

    public event EventHandler<ItemContainerManagerItemsChangedEventArgs>? ItemsChanged;

    public bool IsRecycling { get; set; }

    public ReadOnlyCollection<object> Items => itemContainerGenerator.Items;

    public IReadOnlyCollection<UIElement> RealizedContainers => realizedContainers.Values;

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
            var entry = realizedContainers.Where(entry => !Items.Contains(entry.Key)).Single();
            var item = entry.Key;
            var container = entry.Value;

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

        Debug.Assert(itemIndex != -1);

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

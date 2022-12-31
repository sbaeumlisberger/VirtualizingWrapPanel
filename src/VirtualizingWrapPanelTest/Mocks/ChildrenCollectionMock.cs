using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WpfToolkit.Controls;

namespace VirtualizingWrapPanelTest.Mocks;

internal class ChildrenCollectionMock : IChildrenCollection
{
    public IReadOnlyCollection<IItemContainerInfo> Collection => collection;

    private readonly List<IItemContainerInfo> collection = new List<IItemContainerInfo>();

    public void AddChild(IItemContainerInfo child)
    {
        collection.Add(child);
    }

    public void RemoveChild(IItemContainerInfo child)
    {
        collection.Remove(child);
    }

    public IItemContainerInfo ContainerForItem(object item)
    {
        return collection.First(container => Equals(container.Item, item));
    }
}

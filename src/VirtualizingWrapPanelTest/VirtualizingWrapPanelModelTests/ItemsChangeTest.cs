using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using VirtualizingWrapPanelTest.Mocks;
using WpfToolkit.Controls;

namespace VirtualizingWrapPanelTest.VirtualizingWrapPanelModelTests;

[TestClass]
public class ItemsChangeTest
{
    private ChildrenCollectionMock childrenCollectionMock = new ChildrenCollectionMock();

    [TestMethod]
    public void RemoveRealizedItem()
    {
        var items = Enumerable.Repeat(0, 101).Select(x => new TestItem(100, 100)).Cast<object>().ToList();
        var itemContainerManager = new ItemContainerMangerMock(items);
        var sut = new VirtualizingWrapPanelModel(itemContainerManager, childrenCollectionMock);
        sut.CacheLength = new VirtualizationCacheLength(0);

        sut.OnMeasure(new Size(500, 400));

        Assert.AreEqual(20, childrenCollectionMock.Collection.Count);
        Assert.AreEqual(2100, sut.Extent.Height);

        var item = (TestItem)items[17];
        items.Remove(item);
        var action = NotifyCollectionChangedAction.Remove;
        var eventArgs = new ItemsChangedEventArgs2(action, new[] { new ItemContainerInfoMock(item) });
        itemContainerManager.InvokeItemsChangedEvent(eventArgs);

        Assert.AreEqual(19, childrenCollectionMock.Collection.Count);

        sut.OnMeasure(new Size(500, 400));

        Assert.AreEqual(20, childrenCollectionMock.Collection.Count);
        Assert.AreEqual(2000, sut.Extent.Height);

        sut.OnArrange(new Size(500, 400), false);

        var container = childrenCollectionMock.ContainerForItem(items[17]);
        Assert.AreEqual(new Rect(200, 300, 100, 100), ((ItemContainerInfoMock)container).ArrangeRect);
    }

    [TestMethod]
    public void RemoveVirtualizedItem()
    {
        var items = Enumerable.Repeat(0, 101).Select(x => new TestItem(100, 100)).Cast<object>().ToList();
        var itemContainerManager = new ItemContainerMangerMock(items);
        var sut = new VirtualizingWrapPanelModel(itemContainerManager, childrenCollectionMock);
        sut.CacheLength = new VirtualizationCacheLength(0);

        sut.OnMeasure(new Size(500, 400));

        Assert.AreEqual(20, childrenCollectionMock.Collection.Count);
        Assert.AreEqual(2100, sut.Extent.Height);

        var item = (TestItem)items[58];
        items.Remove(item);
        var action = NotifyCollectionChangedAction.Remove;
        var eventArgs = new ItemsChangedEventArgs2(action, new[] { new ItemContainerInfoMock(item) });
        itemContainerManager.InvokeItemsChangedEvent(eventArgs);

        Assert.AreEqual(20, childrenCollectionMock.Collection.Count);

        sut.OnMeasure(new Size(500, 400));

        Assert.AreEqual(20, childrenCollectionMock.Collection.Count);
        Assert.AreEqual(2000, sut.Extent.Height);
    }

    // TODO Reset, Move, Add, Replace
}

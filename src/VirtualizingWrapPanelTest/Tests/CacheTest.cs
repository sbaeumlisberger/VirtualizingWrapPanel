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
public class CacheTest
{
    private ChildrenCollectionMock childrenCollectionMock = new ChildrenCollectionMock();

    [TestMethod]
    public void NoCache()
    {
        var items = Enumerable.Repeat(0, 100).Select(x => new TestItem(100, 100)).Cast<object>().ToList();
        var itemContainerManager = new ItemContainerMangerMock(items);
        var sut = new VirtualizingWrapPanelModel(itemContainerManager, childrenCollectionMock);
        sut.CacheLength = new VirtualizationCacheLength(0);

        sut.OnMeasure(new Size(500, 398));
        Assert.AreEqual(20, childrenCollectionMock.Collection.Count);

        sut.SetVerticalOffset(501);
        sut.OnMeasure(new Size(500, 398));
        Assert.AreEqual(20, childrenCollectionMock.Collection.Count);
    }

    [TestMethod]
    public void CachePage()
    {
        var items = Enumerable.Repeat(0, 100).Select(x => new TestItem(100, 100)).Cast<object>().ToList();
        var itemContainerManager = new ItemContainerMangerMock(items);
        var sut = new VirtualizingWrapPanelModel(itemContainerManager, childrenCollectionMock);
        sut.CacheLengthUnit = VirtualizationCacheLengthUnit.Page;
        sut.CacheLength = new VirtualizationCacheLength(1, 1);

        sut.OnMeasure(new Size(500, 398));
        Assert.AreEqual(40, childrenCollectionMock.Collection.Count);

        sut.SetVerticalOffset(101);
        sut.OnMeasure(new Size(500, 398));
        Assert.AreEqual(45, childrenCollectionMock.Collection.Count);

        sut.SetVerticalOffset(501);
        sut.OnMeasure(new Size(500, 398));
        Assert.AreEqual(60, childrenCollectionMock.Collection.Count);

        sut.SetVerticalOffset(650);
        sut.OnMeasure(new Size(500, 398));
        Assert.AreEqual(65, childrenCollectionMock.Collection.Count);

        sut.SetVerticalOffset(1601);
        sut.OnMeasure(new Size(500, 398));
        Assert.AreEqual(40, childrenCollectionMock.Collection.Count);
    }

    [TestMethod]
    public void CacheItem()
    {
        var items = Enumerable.Repeat(0, 100).Select(x => new TestItem(100, 100)).Cast<object>().ToList();
        var itemContainerManager = new ItemContainerMangerMock(items);
        var sut = new VirtualizingWrapPanelModel(itemContainerManager, childrenCollectionMock);
        sut.CacheLengthUnit = VirtualizationCacheLengthUnit.Item;
        sut.CacheLength = new VirtualizationCacheLength(10, 10);

        sut.OnMeasure(new Size(500, 398));
        Assert.AreEqual(30, childrenCollectionMock.Collection.Count);

        sut.SetVerticalOffset(101);
        sut.OnMeasure(new Size(500, 398));
        Assert.AreEqual(35, childrenCollectionMock.Collection.Count);

        sut.SetVerticalOffset(501);
        sut.OnMeasure(new Size(500, 398));
        Assert.AreEqual(40, childrenCollectionMock.Collection.Count);

        sut.SetVerticalOffset(650);
        sut.OnMeasure(new Size(500, 398));
        Assert.AreEqual(45, childrenCollectionMock.Collection.Count);

        sut.SetVerticalOffset(1601);
        sut.OnMeasure(new Size(500, 398));
        Assert.AreEqual(30, childrenCollectionMock.Collection.Count);
    }

    [TestMethod]
    public void CacheItem_StartInMidOfRow()
    {
        var items = Enumerable.Repeat(0, 100).Select(x => new TestItem(100, 100)).Cast<object>().ToList();
        var itemContainerManager = new ItemContainerMangerMock(items);
        var sut = new VirtualizingWrapPanelModel(itemContainerManager, childrenCollectionMock);
        sut.CacheLengthUnit = VirtualizationCacheLengthUnit.Item;
        sut.CacheLength = new VirtualizationCacheLength(3, 3);

        sut.OnMeasure(new Size(500, 398));
        sut.SetVerticalOffset(101);
        sut.OnMeasure(new Size(500, 398));
        Assert.AreEqual(26, childrenCollectionMock.Collection.Count);

        sut.OnArrange(new Size(500, 398), false);
        var firstContainerInViewport = childrenCollectionMock.ContainerForItem(items[5]);
        Assert.AreEqual(new Rect(0, -1, 100, 100), ((ItemContainerInfoMock)firstContainerInViewport).ArrangeRect);
    }

    [TestMethod]
    public void CachePixel()
    {
        var items = Enumerable.Repeat(0, 100).Select(x => new TestItem(100, 100)).Cast<object>().ToList();
        var itemContainerManager = new ItemContainerMangerMock(items);
        var sut = new VirtualizingWrapPanelModel(itemContainerManager, childrenCollectionMock);
        sut.CacheLengthUnit = VirtualizationCacheLengthUnit.Pixel;
        sut.CacheLength = new VirtualizationCacheLength(100, 100);

        sut.OnMeasure(new Size(500, 398));
        Assert.AreEqual(25, childrenCollectionMock.Collection.Count);

        sut.SetVerticalOffset(101);
        sut.OnMeasure(new Size(500, 398));
        Assert.AreEqual(30, childrenCollectionMock.Collection.Count);

        sut.SetVerticalOffset(501);
        sut.OnMeasure(new Size(500, 398));
        Assert.AreEqual(30, childrenCollectionMock.Collection.Count);

        sut.SetVerticalOffset(650);
        sut.OnMeasure(new Size(500, 398));
        Assert.AreEqual(35, childrenCollectionMock.Collection.Count);

        sut.SetVerticalOffset(1601);
        sut.OnMeasure(new Size(500, 398));
        Assert.AreEqual(25, childrenCollectionMock.Collection.Count);
    }
}

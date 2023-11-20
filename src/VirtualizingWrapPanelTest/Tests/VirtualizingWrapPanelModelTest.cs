using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Media3D;
using Moq;
using VirtualizingWrapPanelTest.Mocks;
using WpfToolkit.Controls;

namespace VirtualizingWrapPanelTest.VirtualizingWrapPanelModelTests;

[TestClass]
public class VirtualizingWrapPanelModelTest
{
    [TestMethod]
    public void Offset0x0_NoCache()
    {
        var items = new List<TestItem>()
        {
            new (100, 100), new (200, 70), new (50, 100), new(70, 80), new(120, 90), new(50, 40), // height 100
            new (80, 110), new (100, 150), new (160, 170), new (200, 180), new (40, 100), // height 270
            new (100, 100), new (180, 70), new (50, 140), new(70, 80), new(130, 90), new(50, 40), // height 410
            new (160, 160), new (200, 180), new (40, 100), new (100, 100), new (100, 150), // height 570
            new (100, 70), new (190, 70), new (50, 100), new(70, 80), new(130, 90), new(50, 40), // height 670
            new (70, 80), new(120, 90), new(50, 40), new (100, 80), new (200, 70), new (50, 60), // height 750
            new (170, 200), new (100, 180), new (140, 100), new (80, 120), new (100, 150), // height 950
            new (120, 110), new (170, 70), new (50, 100), new(40, 80), new(130, 90), new(50, 40), // height 1060
            new (40, 100), new (160, 140), new (200, 180), new (100, 100), new (100, 150), // height 1240
            new (100, 70), new (190, 70), new (50, 100), new(70, 80), new(130, 90), new(50, 40), // height 1340
        }.Cast<object>().ToList();
        var itemContainerManger = new ItemContainerMangerMock(items);
        var childrenCollectionMock = new ChildrenCollectionMock();
        var sut = new VirtualizingWrapPanelModel(itemContainerManger, childrenCollectionMock);
        sut.CacheLength = new VirtualizationCacheLength(0, 0);
        sut.AllowDifferentSizedItems= true;

        sut.OnMeasure(new Size(600, 400));

        //Assert.AreEqual(17, itemContainerManger.RealizedContainers.Count);
        for (var i = 0; i < 17; i++)
       {
            Assert.IsTrue(itemContainerManger.IsItemRealized(items[i]), $"Item {i} not realized");
        }


        sut.OnArrange(new Size(600, 400), false);

        var containers = itemContainerManger.RealizedContainers.Cast<ItemContainerInfoMock>().ToList();
        Assert.AreEqual(new Rect(0, 0, 100, 100), containers[0].ArrangeRect);
        Assert.AreEqual(new Rect(100, 0, 200, 70), containers[1].ArrangeRect);
        Assert.AreEqual(new Rect(300, 0, 50, 100), containers[2].ArrangeRect);
        Assert.AreEqual(new Rect(350, 0, 70, 80), containers[3].ArrangeRect);
        Assert.AreEqual(new Rect(420, 0, 120, 90), containers[4].ArrangeRect);
        Assert.AreEqual(new Rect(540, 0, 50, 40), containers[5].ArrangeRect);
        Assert.AreEqual(new Rect(0, 100, 80, 110), containers[6].ArrangeRect);

        //Assert.Equals(new Rect(), containers[7].ArrangeRect);
        //Assert.Equals(new Rect(), containers[8].ArrangeRect);
        //Assert.Equals(new Rect(), containers[9].ArrangeRect);
        //Assert.Equals(new Rect(), containers[10].ArrangeRect);
        //Assert.Equals(new Rect(), containers[11].ArrangeRect);
        //Assert.Equals(new Rect(), containers[12].ArrangeRect);
        //Assert.Equals(new Rect(), containers[13].ArrangeRect);
        //Assert.Equals(new Rect(), containers[14].ArrangeRect);
        //Assert.Equals(new Rect(), containers[15].ArrangeRect);
        //Assert.Equals(new Rect(), containers[16].ArrangeRect);
    }

    [TestMethod]
    public void Measure_Offset0x0_NoCache_FixedSize()
    {
        var items = Enumerable.Repeat(0, 100).Select(x => new TestItem(100, 100)).Cast<object>().ToList();
        var itemContainerManger = new ItemContainerMangerMock(items);
        var childrenCollectionMock = new ChildrenCollectionMock();
        var sut = new VirtualizingWrapPanelModel(itemContainerManger, childrenCollectionMock);
        sut.CacheLength = new VirtualizationCacheLength(0, 0);
        sut.FixedItemSize = new Size(100, 100);

        sut.OnMeasure(new Size(600, 400));

        Assert.AreEqual(24, itemContainerManger.RealizedContainers.Count);
        for (var i = 0; i < 24; i++)
        {
            Assert.IsTrue(itemContainerManger.IsItemRealized(items[i]));
        }
    }

    //[TestMethod]
    //public void NegativeOffset()
    //{
    //    var items = Enumerable.Repeat(0, 100).Select(x => new TestItem(100, 100)).Cast<object>().ToList();
    //    var itemContainerManger = new ItemContainerMangerMock(items);
    //    var childrenCollectionMock = new ChildrenCollectionMock();
    //    var sut = new VirtualizingWrapPanelModel(itemContainerManger, childrenCollectionMock);

    //    Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
    //    {
    //        sut.OnMeasure(new Size(600, 400), new Point(0, -800), out _);
    //    });
    //}

    // TODO
    // cache
    // resize
    // insert/remove items
    // arrange modes
    // vertical orientation
    // BringIndexIntoView
    // fixed/provided/calculated item size
    // grouping: negative offset
    // ...

}

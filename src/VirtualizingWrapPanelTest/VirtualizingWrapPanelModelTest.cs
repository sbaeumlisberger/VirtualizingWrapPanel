using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Media3D;
using Moq;
using WpfToolkit.Controls;

namespace VirtualizingWrapPanelTest;

[TestClass]
public class VirtualizingWrapPanelModelTest
{
    private VirtualizingWrapPanelModel sut;

    private ItemContainerMangerMock itemContainerManger;

    private List<TestItem> items = new List<TestItem>();

    [TestInitialize]
    public void Initialize()
    {
        items = new List<TestItem>()
        {
            new (100, 100), new (200, 70), new (50, 100), new(70, 80), new(130, 90), new(50, 40), // hieght 100
            new (80, 110), new (100, 150), new (160, 170), new (200, 180), new (40, 100), // height 270
            new (100, 100), new (200, 70), new (50, 140), new(70, 80), new(130, 90), new(50, 40), // height 410
            new (160, 160), new (200, 180), new (40, 100), new (100, 100), new (100, 150), // height 570
            new (100, 70), new (190, 70), new (50, 100), new(70, 80), new(130, 90), new(50, 40), // height 670
            new (70, 80), new(120, 90), new(50, 40), new (100, 80), new (200, 70), new (50, 60), // height 750
            new (170, 200), new (100, 180), new (140, 100), new (80, 120), new (100, 150), // height 950
            new (120, 110), new (170, 70), new (50, 100), new(40, 80), new(130, 90), new(50, 40), // height 1060
            new (40, 100), new (160, 140), new (200, 180), new (100, 100), new (100, 150), // height 1240
            new (100, 70), new (190, 70), new (50, 100), new(70, 80), new(130, 90), new(50, 40), // height 1340
        };

        long id = 0;
        items.ForEach(item => item.ID = id++);

        itemContainerManger = new ItemContainerMangerMock(items);

        sut = new VirtualizingWrapPanelModel(itemContainerManger);
    }

    [TestMethod]
    public void Offset0x0_NoCache()
    {
        sut.Items = items;
        sut.CacheLength = new VirtualizationCacheLength(0, 0);


        sut.OnMeasure(new Size(600, 400), new Point(0, 0));

        Assert.AreEqual(17, itemContainerManger.RealizedContainers.Count);
        for (int i = 0; i < 17; i++)
        {
            Assert.IsTrue(itemContainerManger.IsItemRealized(items[i]));
        }


        sut.OnArrange(new Size(600, 400));

        var containers = itemContainerManger.RealizedContainers;
        Assert.AreEqual(new Rect(0, 0, 100, 100), containers[0].ArrangeRect);
        Assert.AreEqual(new Rect(100, 0, 200, 70), containers[1].ArrangeRect);
        Assert.AreEqual(new Rect(300, 0, 50, 100), containers[2].ArrangeRect);
        Assert.AreEqual(new Rect(350, 0, 70, 80), containers[3].ArrangeRect);
        Assert.AreEqual(new Rect(420, 0, 130, 90), containers[4].ArrangeRect);
        Assert.AreEqual(new Rect(550, 0, 50, 40), containers[5].ArrangeRect);
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
    public void Offset0x400_NoCache()
    {
        sut.Items = items;
        sut.CacheLength = new VirtualizationCacheLength(0, 0);


        sut.OnMeasure(new Size(600, 400), new Point(0, 400));

        Assert.AreEqual(28, itemContainerManger.RealizedContainers.Count);
        for (int i = 11; i < 39; i++)
        {
            Assert.IsTrue(itemContainerManger.IsItemRealized(items[i]));
        }

        sut.OnArrange(new Size(600, 400));
    }

    [TestMethod]
    public void NegativeOffset()
    {
        sut.Items = items;
        sut.CacheLength = new VirtualizationCacheLength(0, 0);

        Size desiredSize = sut.OnMeasure(new Size(600, 400), new Point(0, -800));

        Assert.AreEqual(0, itemContainerManger.RealizedContainers.Count);
        Assert.AreEqual(0, desiredSize.Width);
        Assert.AreEqual(0, desiredSize.Height);
    }

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

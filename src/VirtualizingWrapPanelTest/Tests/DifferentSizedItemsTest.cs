using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using VirtualizingWrapPanelTest.Mocks;
using WpfToolkit.Controls;

namespace VirtualizingWrapPanelTest.VirtualizingWrapPanelModelTests;

[TestClass]
public class DifferentSizedItemsTest
{

    [TestMethod]
    public void ItemSizesAreCached()
    {
        var childrenCollectionMock = new ChildrenCollectionMock();
        var items = Enumerable.Repeat(0, 100).Select(x => new LazyTestItem()).Cast<object>().ToList();
        var itemContainerManager = new ItemContainerMangerMock(items);
        var sut = new VirtualizingWrapPanelModel(itemContainerManager, childrenCollectionMock);
        sut.AllowDifferentSizedItems = true;

        // measure twice to cache final size of item
        sut.OnMeasure(new Size(500, 400));
        sut.OnMeasure(new Size(500, 400));

        // scroll away to virtualize item
        sut.SetVerticalOffset(1000);
        sut.OnMeasure(new Size(500, 400));

        // scroll back and arrange
        sut.SetVerticalOffset(0);
        sut.OnMeasure(new Size(500, 400));
        sut.OnArrange(new Size(500, 400), false);

        var firstItem = (LazyTestItem)items[0];
        var containerOfFirstItem = (ItemContainerInfoMock)childrenCollectionMock.ContainerForItem(firstItem);
        Assert.AreEqual(firstItem.Width, containerOfFirstItem.ArrangeRect.Width);
        Assert.AreEqual(firstItem.Height, containerOfFirstItem.ArrangeRect.Height);

    }

    // TODO test extent gets to big
}

using System.Collections.ObjectModel;
using System.Windows.Controls;
using WpfToolkit.Controls;
using Xunit;
using Assert = Xunit.Assert;

namespace VirtualizingWrapPanelTest.Tests;

public class BasisTest
{
    private VirtualizingWrapPanel vwp = TestUtil.CreateVirtualizingWrapPanel(500, 400);

    [UIFact]
    public void BringIndexIntoView_AfterViewport_VerticalOrientation()
    {
        vwp.Orientation = Orientation.Vertical;
        vwp.UpdateLayout();

        vwp.BringIndexIntoViewPublic(500);
        vwp.UpdateLayout();

        Assert.Equal(12_100, vwp.HorizontalOffset);
        Assert.Equal(25_000, vwp.ExtentWidth);
        Assert.Equal(400, vwp.ExtentHeight);
        Assert.Equal(60, vwp.Children.Count);
        TestUtil.AssertItem(vwp, "Item 501", 400, 0);
    }

    [UIFact]
    public void BringIndexIntoView_BeforeViewport_VerticalOrientation()
    {
        vwp.Orientation = Orientation.Vertical;
        vwp.UpdateLayout();

        vwp.SetHorizontalOffset(20_000);
        vwp.UpdateLayout();

        vwp.BringIndexIntoViewPublic(500);
        vwp.UpdateLayout();

        Assert.Equal(12_500, vwp.HorizontalOffset);
        Assert.Equal(25_000, vwp.ExtentWidth);
        Assert.Equal(400, vwp.ExtentHeight);
        Assert.Equal(60, vwp.Children.Count);
        TestUtil.AssertItem(vwp, "Item 501", 0, 0);
    }

    [UIFact]
    public void ItemRemoved()
    {
        var items = (ObservableCollection<TestItem>)vwp.ItemsControl.ItemsSource;

        items.RemoveAt(7); // Remove Item 8
        vwp.UpdateLayout();

        TestUtil.AssertItemRangeRealized(vwp, 1, 7);
        TestUtil.AssertItemNotRealized(vwp, "Item 8");
        TestUtil.AssertItemRangeRealized(vwp, 9, 41);
        TestUtil.AssertItem(vwp, "Item 9", 200, 100);
        Assert.Equal(40, vwp.Children.Count);
    }

    [UIFact]
    public void ItemMoved_InViewport()
    {
        var items = (ObservableCollection<TestItem>)vwp.ItemsControl.ItemsSource;

        items.Move(7, 8); // Move Item 8 after Item 9
        vwp.UpdateLayout();

        TestUtil.AssertItemRangeRealized(vwp, 1, 40);
        TestUtil.AssertItem(vwp, "Item 8", 300, 100);
        TestUtil.AssertItem(vwp, "Item 9", 200, 100);
        Assert.Equal(40, vwp.Children.Count);
    }

    [UIFact]
    public void ItemMoved_OutOfViewportAndCache()
    {
        var items = (ObservableCollection<TestItem>)vwp.ItemsControl.ItemsSource;

        items.Move(7, 99); // Move Item 8 after Item 100
        vwp.UpdateLayout();

        TestUtil.AssertItemRangeRealized(vwp, 1, 7);
        TestUtil.AssertItemNotRealized(vwp, "Item 8");
        TestUtil.AssertItemRangeRealized(vwp, 9, 41);
        TestUtil.AssertItem(vwp, "Item 9", 200, 100);
        Assert.Equal(40, vwp.Children.Count);
    }

    [UIFact]
    public void ItemsCleared()
    {
        var items = (ObservableCollection<TestItem>)vwp.ItemsControl.ItemsSource;

        items.Clear();
        vwp.UpdateLayout();

        Assert.Empty(vwp.Children);
        Assert.Equal(0, vwp.ExtentHeight);
    }

    // TODO test items changes: add, replace, remove mutiple items

}

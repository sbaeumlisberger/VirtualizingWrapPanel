using System.Drawing;
using System.Windows.Controls;
using WpfToolkit.Controls;
using Xunit;
using Assert = Xunit.Assert;

namespace VirtualizingWrapPanelTest.Tests;

public class AllowDifferentSizedItemsTest
{
    private Size panelSize = new Size(500, 400);

    private List<TestItem> items = GenerateItems(1000);

    private VirtualizingWrapPanel vwp;

    public AllowDifferentSizedItemsTest()
    {
        vwp = TestUtil.CreateVirtualizingWrapPanel(panelSize.Width, panelSize.Height, items);
        VirtualizingPanel.SetCacheLength(vwp.ItemsControl, new VirtualizationCacheLength(0));
        vwp.AllowDifferentSizedItems = true;
        vwp.UpdateLayout();
        Assert_ItemsRealized(vwp);
    }

    [UIFact]
    public void IncreaseWidth_() // Issue #48
    {
        var items = new[]
{
            new TestItem("Item 1", 150, 80),
            new TestItem("Item 2", 60, 80),
            new TestItem("Item 3", 80, 110),
            new TestItem("Item 4", 130, 70),
            new TestItem("Item 5", 110, 90),
            new TestItem("Item 6", 200, 90),
            new TestItem("Item 7", 100, 60),
            new TestItem("Item 8", 90, 80),
            new TestItem("Item 9", 60, 80),
            new TestItem("Item 10", 70, 90),
            new TestItem("Item 11", 130, 80),
            new TestItem("Item 12", 120, 70),
            new TestItem("Item 13", 110, 100),
            new TestItem("Item 14", 110, 60),
            new TestItem("Item 15", 90, 120),
            new TestItem("Item 16", 170, 80),
            new TestItem("Item 17", 130, 50),
            new TestItem("Item 18", 90, 80),
            new TestItem("Item 19", 120, 70),
            new TestItem("Item 20", 80, 100),
        };

        var vwp = TestUtil.CreateVirtualizingWrapPanel(500, 400, items);

        // Width  150 + 60 + 80 + 130 = 420        | Item 1-4    | Height 110  | Total: Width=420, Height=110
        // Width  110 + 200 + 100 + 90 = 500       | Item 5-8    | Height  90  | Total: Width=500, Height=200
        // Width  60 + 70 + 130 + 120 + 110 = 490  | Item 9-13   | Height 100  | Total: Width=500, Height=300
        // Width  110 + 90 + 170 + 130 = 500       | Item 14-17  | Height 120  | Total: Width=500, Height=420
        // Width  90 + 120 + 80 = 290              | Item 18-20  | Height 100  | Total: Width=500, Height=520

        vwp.AllowDifferentSizedItems = true;
        vwp.UpdateLayout();

        Assert.Equal(500, vwp.DesiredSize.Width);
        Assert.Equal(400, vwp.DesiredSize.Height);
        //Assert.Equal(520, vwp.ExtentHeight);

        vwp.ItemsControl.Width = 700;
        vwp.UpdateLayout();

        // Width  150 + 60 + 80 + 130 + 110 = 530       | Item 1-5    | Height 110  | Total: Width=530, Height=110
        // Width  200 + 100 + 90 + 60 + 70 + 130 = 650  | Item 6-11   | Height  90  | Total: Width=650, Height=200
        // Width  120 + 110 + 110 + 90 + 170 = 600      | Item 12-16  | Height 120  | Total: Width=650, Height=320      
        // Width  130 + 90 + 120 + 80 = 420             | Item 17-20  | Height 100  | Total: Width=650, Height=420

        Assert.Equal(650, vwp.DesiredSize.Width);
        Assert.Equal(400, vwp.DesiredSize.Height);
        Assert.Equal(420, vwp.ExtentHeight);
    }

    [UIFact]
    public void ScrollToEndWithSmallSteps()
    {
        double extend = CalculateExtend(items, panelSize.Width);
        double maxOffset = extend - panelSize.Height;

        while (vwp.VerticalOffset < maxOffset)
        {
            vwp.SetVerticalOffset(Math.Min(vwp.VerticalOffset + vwp.MouseWheelDelta, maxOffset));
            vwp.UpdateLayout();

            Assert_ItemsRealized(vwp);
        }
        Assert.Equal(extend, vwp.ExtentHeight);
    }

    [UIFact]
    public void ScrollHugeStep_AfterScrolledToEnd()
    {
        double extend = CalculateExtend(items, panelSize.Width);
        double maxOffset = extend - panelSize.Height;
        while (vwp.VerticalOffset < maxOffset)
        {
            vwp.SetVerticalOffset(Math.Min(vwp.VerticalOffset + vwp.MouseWheelDelta, maxOffset));
            vwp.UpdateLayout();
        }

        vwp.SetVerticalOffset(maxOffset / 2);
        vwp.UpdateLayout();

        Assert_ItemsRealized(vwp);
        Assert.Equal(extend, vwp.ExtentHeight);
    }

    [UIFact] // TODO: fails sometimes
    public void ScrollHugeStep()
    {
        vwp.SetVerticalOffset((vwp.ExtentHeight - panelSize.Height) / 2);
        vwp.UpdateLayout();

        TestUtil.AssertItemNotRealized(vwp, items.First().Name);
        TestUtil.AssertItemNotRealized(vwp, items.Last().Name);
        Assert.True(vwp.ExtentHeight >= vwp.VerticalOffset + panelSize.Height);
    }

    [UIFact] // TODO: fails sometimes
    public void ScrollHugeStep_Recycling()
    {
        VirtualizingPanel.SetVirtualizationMode(vwp.ItemsControl, VirtualizationMode.Recycling);
        vwp.UpdateLayout();

        vwp.SetVerticalOffset((vwp.ExtentHeight - panelSize.Height) / 2);
        vwp.UpdateLayout();

        TestUtil.AssertItemNotRealized(vwp, items.First().Name);
        TestUtil.AssertItemNotRealized(vwp, items.Last().Name);
        Assert.True(vwp.ExtentHeight >= vwp.VerticalOffset + panelSize.Height);
    }

    [UIFact]
    public void Resize()
    {
        double oldExtend = vwp.ExtentHeight;
        double newWidth = 600;

        vwp.ItemsControl.Width = newWidth;
        vwp.UpdateLayout();

        Assert.Equal(vwp.ViewportWidth, newWidth);
        Assert_ItemsRealized(vwp);
        Assert.True(vwp.ExtentHeight < oldExtend);
    }

    [UIFact] // TODO: fails sometimes
    public void ScrollHugeStep_UsingItemSizeProvider()
    {
        vwp.ItemSizeProvider = new TestItemSizeProvider();
        vwp.UpdateLayout();
        double extend = CalculateExtend(items, panelSize.Width);
        double maxOffset = extend - panelSize.Height;

        vwp.SetVerticalOffset(maxOffset / 2);
        vwp.UpdateLayout();

        Assert_ItemsRealized(vwp);
        Assert.True(vwp.ExtentHeight >= maxOffset / 2 + panelSize.Height);
    }

    [UIFact]
    public void ExtendShouldAdjustWhenCollectionChanged()
    {
        double extend = CalculateExtend(items, panelSize.Width);
        double maxOffset = extend - panelSize.Height;
        while (vwp.VerticalOffset < maxOffset)
        {
            vwp.SetVerticalOffset(Math.Min(vwp.VerticalOffset + vwp.MouseWheelDelta, maxOffset));
            vwp.UpdateLayout();
        }
        double extendBefore = vwp.ExtentHeight;

        var newItems = GenerateItems(100);
        vwp.ItemsControl.ItemsSource = newItems;
        vwp.ItemsControl.UpdateLayout();

        Assert.True(vwp.ExtentHeight < extendBefore / 8);
        TestUtil.AssertItemRealized(vwp, "Item 1");
        TestUtil.AssertItemRealized(vwp, "Item 2");
    }

    [UIFact]
    public void ExtendShouldAdjustWhenCollectionChangedWithItemSizeProvider()
    {
        vwp.ItemSizeProvider = new TestItemSizeProvider();
        double extend = CalculateExtend(items, panelSize.Width);
        double maxOffset = extend - panelSize.Height;
        vwp.SetVerticalOffset(maxOffset);
        vwp.UpdateLayout();

        var newItems = GenerateItems(100);
        vwp.ItemsControl.ItemsSource = newItems;
        vwp.ItemsControl.UpdateLayout();

        double exptectedExtend = CalculateExtend(newItems, panelSize.Width);
        Assert.Equal(exptectedExtend, vwp.ExtentHeight);
        TestUtil.AssertItemRealized(vwp, "Item 99");
        TestUtil.AssertItemRealized(vwp, "Item 100");
    }

    private static double CalculateExtend(List<TestItem> items, double viewportWidth)
    {
        double rowWidth = 0;
        double rowHeight = 0;
        double extend = 0;
        foreach (TestItem item in items)
        {
            if (rowWidth + item.Width > viewportWidth)
            {
                rowWidth = 0;
                extend += rowHeight;
                rowHeight = 0;
            }
            rowWidth += item.Width;
            rowHeight = Math.Max(rowHeight, item.Height);
        }
        extend += rowHeight;
        return extend;
    }

    private static void Assert_ItemsRealized(VirtualizingWrapPanel vwp)
    {
        double rowWidth = 0;
        double rowHeight = 0;
        double extend = 0;
        var rowItems = new List<TestItem>();
        bool previousRowRealized = false;
        foreach (TestItem item in vwp.ItemsControl.Items)
        {
            if (rowWidth + item.Width > vwp.ViewportWidth)
            {
                AssertRow(vwp, rowItems, extend, rowHeight, ref previousRowRealized);
                rowWidth = 0;
                extend += rowHeight;
                rowHeight = 0;
                rowItems.Clear();
            }
            rowWidth += item.Width;
            rowHeight = Math.Max(rowHeight, item.Height);
            rowItems.Add(item);
        }
        AssertRow(vwp, rowItems, extend, rowHeight, ref previousRowRealized);
    }

    private static void AssertRow(VirtualizingWrapPanel vwp, List<TestItem> rowItems, double previousExtend, double rowHeight, ref bool previousRowRealized)
    {
        if (previousExtend + rowHeight > vwp.VerticalOffset && previousExtend < vwp.VerticalOffset + vwp.ViewportHeight)
        {
            rowItems.ForEach(item => TestUtil.AssertItemRealized(vwp, item.Name));
            previousRowRealized = true;
        }
        else
        {
            if (previousRowRealized)
            {
                TestUtil.AssertItemRealized(vwp, rowItems.First().Name);
                rowItems.Skip(1).ToList().ForEach(item => TestUtil.AssertItemNotRealized(vwp, item.Name));
            }
            else
            {
                rowItems.ForEach(item => TestUtil.AssertItemNotRealized(vwp, item.Name));
            }
            previousRowRealized = false;
        }
    }

    private static List<TestItem> GenerateItems(int itemCount)
    {
        return Enumerable.Range(1, itemCount).Select(i => new TestItem("Item " + i, Random.Shared.Next(60, 120), Random.Shared.Next(60, 120))).ToList();
    }

}

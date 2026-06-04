using System.Windows;
using System.Windows.Controls;
using WpfToolkit.Controls;
using Xunit;

namespace VirtualizingWrapPanelTest.TestsV2;

public class ScrollingTest
{
    private readonly TestController testController = TestController.CreateListBoxWithVirtualizingWrapPanel();

    [WpfTheory]
    [InlineData(VirtualizationMode.Standard)]
    [InlineData(VirtualizationMode.Recycling)]
    public async Task SetVerticalOffset(VirtualizationMode virtualizationMode)
    {
        await testController.SetVirtualizationModeAsync(virtualizationMode);

        await testController.SetVerticalOffsetAsync(330);

        Assert.Equal(330, testController.VerticalOffset);
        TestUtil.AssertContainerBounds(testController, testController.GetItemsInViewport());
    }

    [WpfTheory]
    [InlineData(VirtualizationMode.Standard)]
    [InlineData(VirtualizationMode.Recycling)]
    public async Task LineDown_ScrollUnitPixel(VirtualizationMode virtualizationMode)
    {
        await testController.SetVirtualizationModeAsync(virtualizationMode);
        await testController.SetScrollUnitAsync(ScrollUnit.Pixel);
        double verticalScrollOffsetBefore = testController.ViewportHeight;
        await testController.SetVerticalOffsetAsync(verticalScrollOffsetBefore);

        await testController.ScrollLineDownAsync();

        Assert.Equal(verticalScrollOffsetBefore + VirtualizingPanelBase.DefaultScrollLineDeltaPixel, testController.VerticalOffset);
        TestUtil.AssertContainerBounds(testController, testController.GetItemsInViewport());
    }

    [WpfTheory]
    [InlineData(VirtualizationMode.Standard)]
    [InlineData(VirtualizationMode.Recycling)]
    public async Task LineDown_ScrollUnitItem(VirtualizationMode virtualizationMode)
    {
        await testController.SetVirtualizationModeAsync(virtualizationMode);
        await testController.SetScrollUnitAsync(ScrollUnit.Item);
        double verticalScrollOffsetBefore = testController.ViewportHeight;
        await testController.SetVerticalOffsetAsync(verticalScrollOffsetBefore);

        await testController.ScrollLineDownAsync();

        Assert.Equal(verticalScrollOffsetBefore + VirtualizingPanelBase.DefaultScrollLineDeltaItem * testController.FirstChildHeight, testController.VerticalOffset);
        TestUtil.AssertContainerBounds(testController, testController.GetItemsInViewport());
    }

    [WpfTheory]
    [InlineData(VirtualizationMode.Standard)]
    [InlineData(VirtualizationMode.Recycling)]
    public async Task LineUp_ScrollUnitPixel(VirtualizationMode virtualizationMode)
    {
        await testController.SetVirtualizationModeAsync(virtualizationMode);
        await testController.SetScrollUnitAsync(ScrollUnit.Pixel);
        double verticalScrollOffsetBefore = testController.ViewportHeight;
        await testController.SetVerticalOffsetAsync(verticalScrollOffsetBefore);

        await testController.ScrollLineUpAsync();

        Assert.Equal(verticalScrollOffsetBefore - VirtualizingPanelBase.DefaultScrollLineDeltaPixel, testController.VerticalOffset);
        TestUtil.AssertContainerBounds(testController, testController.GetItemsInViewport());
    }

    [WpfTheory]
    [InlineData(VirtualizationMode.Standard)]
    [InlineData(VirtualizationMode.Recycling)]
    public async Task LineUp_ScrollUnitItem(VirtualizationMode virtualizationMode)
    {
        await testController.SetVirtualizationModeAsync(virtualizationMode);
        await testController.SetScrollUnitAsync(ScrollUnit.Item);
        double verticalScrollOffsetBefore = testController.ViewportHeight;
        await testController.SetVerticalOffsetAsync(verticalScrollOffsetBefore);

        await testController.ScrollLineUpAsync();

        Assert.Equal(verticalScrollOffsetBefore - VirtualizingPanelBase.DefaultScrollLineDeltaItem * testController.FirstChildHeight, testController.VerticalOffset);
        TestUtil.AssertContainerBounds(testController, testController.GetItemsInViewport());
    }

    [WpfTheory]
    [InlineData(VirtualizationMode.Standard)]
    [InlineData(VirtualizationMode.Recycling)]
    public async Task PageDown(VirtualizationMode virtualizationMode)
    {
        await testController.SetVirtualizationModeAsync(virtualizationMode);
        double verticalScrollOffsetBefore = testController.ViewportHeight;
        await testController.SetVerticalOffsetAsync(verticalScrollOffsetBefore);

        await testController.ScrollPageDownAsync();

        Assert.Equal(verticalScrollOffsetBefore + testController.ViewportHeight, testController.VerticalOffset);
        TestUtil.AssertContainerBounds(testController, testController.GetItemsInViewport());
    }

    [WpfTheory]
    [InlineData(VirtualizationMode.Standard)]
    [InlineData(VirtualizationMode.Recycling)]
    public async Task PageUp(VirtualizationMode virtualizationMode)
    {
        await testController.SetVirtualizationModeAsync(virtualizationMode);
        double verticalScrollOffsetBefore = testController.ViewportHeight;
        await testController.SetVerticalOffsetAsync(verticalScrollOffsetBefore);

        await testController.ScrollPageUpAsync();

        Assert.Equal(verticalScrollOffsetBefore - testController.ViewportHeight, testController.VerticalOffset);
        TestUtil.AssertContainerBounds(testController, testController.GetItemsInViewport());
    }

    [WpfTheory]
    [InlineData(VirtualizationMode.Standard)]
    [InlineData(VirtualizationMode.Recycling)]
    public async Task MouseWheelDown_ScrollUnitPixel(VirtualizationMode virtualizationMode)
    {
        await testController.SetVirtualizationModeAsync(virtualizationMode);
        await testController.SetScrollUnitAsync(ScrollUnit.Pixel);
        double verticalScrollOffsetBefore = testController.ViewportHeight;
        await testController.SetVerticalOffsetAsync(verticalScrollOffsetBefore);

        await testController.ScrollMouseWheelDownAsync();

        Assert.Equal(verticalScrollOffsetBefore + VirtualizingPanelBase.DefaultMouseWheelDeltaPixel, testController.VerticalOffset);
        TestUtil.AssertContainerBounds(testController, testController.GetItemsInViewport());
    }

    [WpfTheory]
    [InlineData(VirtualizationMode.Standard)]
    [InlineData(VirtualizationMode.Recycling)]
    public async Task MouseWheelDown_ScrollUnitItem(VirtualizationMode virtualizationMode)
    {
        await testController.SetVirtualizationModeAsync(virtualizationMode);
        await testController.SetScrollUnitAsync(ScrollUnit.Item);
        double verticalScrollOffsetBefore = testController.ViewportHeight;
        await testController.SetVerticalOffsetAsync(verticalScrollOffsetBefore);

        await testController.ScrollMouseWheelDownAsync();

        Assert.Equal(verticalScrollOffsetBefore + VirtualizingPanelBase.DefaultMouseWheelDeltaItem * testController.FirstChildHeight, testController.VerticalOffset);
        TestUtil.AssertContainerBounds(testController, testController.GetItemsInViewport());
    }

    [WpfTheory]
    [InlineData(VirtualizationMode.Standard)]
    [InlineData(VirtualizationMode.Recycling)]
    public async Task MouseWheelUp_ScrollUnitPixel(VirtualizationMode virtualizationMode)
    {
        await testController.SetVirtualizationModeAsync(virtualizationMode);
        await testController.SetScrollUnitAsync(ScrollUnit.Pixel);
        double verticalScrollOffsetBefore = testController.ViewportHeight;
        await testController.SetVerticalOffsetAsync(verticalScrollOffsetBefore);

        await testController.ScrollMouseWheelUpAsync();

        Assert.Equal(verticalScrollOffsetBefore - VirtualizingPanelBase.DefaultMouseWheelDeltaPixel, testController.VerticalOffset);
        TestUtil.AssertContainerBounds(testController, testController.GetItemsInViewport());
    }

    [WpfTheory]
    [InlineData(VirtualizationMode.Standard)]
    [InlineData(VirtualizationMode.Recycling)]
    public async Task MouseWheelUp_ScrollUnitItem(VirtualizationMode virtualizationMode)
    {
        await testController.SetVirtualizationModeAsync(virtualizationMode);
        await testController.SetScrollUnitAsync(ScrollUnit.Item);
        double verticalScrollOffsetBefore = testController.ViewportHeight;
        await testController.SetVerticalOffsetAsync(verticalScrollOffsetBefore);

        await testController.ScrollMouseWheelUpAsync();

        Assert.Equal(verticalScrollOffsetBefore - VirtualizingPanelBase.DefaultMouseWheelDeltaItem * testController.FirstChildHeight, testController.VerticalOffset);
        TestUtil.AssertContainerBounds(testController, testController.GetItemsInViewport());
    }

    [WpfTheory]
    [InlineData(VirtualizationMode.Standard)]
    [InlineData(VirtualizationMode.Recycling)]
    public async Task ScrollIntoView_ItemAfterViewport(VirtualizationMode virtualizationMode)
    {
        await testController.SetVirtualizationModeAsync(virtualizationMode);

        var targetItem = testController.Items[42];
        await testController.ScrollIntoViewAsync(targetItem);

        Assert.Equal(500, testController.VerticalOffset);
        var itemsInViewport = testController.GetItemsInViewport();
        Assert.Contains(targetItem, itemsInViewport);
        TestUtil.AssertContainerBounds(testController, itemsInViewport);
    }

    [WpfTheory]
    [InlineData(VirtualizationMode.Standard)]
    [InlineData(VirtualizationMode.Recycling)]
    public async Task ScrollIntoView_ItemBeforeViewport(VirtualizationMode virtualizationMode)
    {
        await testController.SetVirtualizationModeAsync(virtualizationMode);
        await testController.SetVerticalOffsetAsync(2000);

        var targetItem = testController.Items[42];
        await testController.ScrollIntoViewAsync(targetItem);

        Assert.Equal(800, testController.VerticalOffset);
        var itemsInViewport = testController.GetItemsInViewport();
        Assert.Contains(targetItem, itemsInViewport);
        TestUtil.AssertContainerBounds(testController, itemsInViewport);
    }

    [WpfTheory]
    [InlineData(VirtualizationMode.Standard)]
    [InlineData(VirtualizationMode.Recycling)]
    public async Task BringIndexIntoView_ItemAfterViewport(VirtualizationMode virtualizationMode)
    {
        await testController.SetVirtualizationModeAsync(virtualizationMode);

        int targetItemIndex = 42;
        await testController.BringIndexIntoViewAsync(targetItemIndex);

        Assert.Equal(500, testController.VerticalOffset);
        var itemsInViewport = testController.GetItemsInViewport();
        Assert.Contains(testController.Items[targetItemIndex], itemsInViewport);
        TestUtil.AssertContainerBounds(testController, itemsInViewport);
    }

    [WpfTheory]
    [InlineData(VirtualizationMode.Standard)]
    [InlineData(VirtualizationMode.Recycling)]
    public async Task BringIndexIntoView_ItemBeforeViewport(VirtualizationMode virtualizationMode)
    {
        await testController.SetVirtualizationModeAsync(virtualizationMode);
        await testController.SetVerticalOffsetAsync(2000);

        int targetItemIndex = 42;
        await testController.BringIndexIntoViewAsync(42);

        Assert.Equal(800, testController.VerticalOffset);
        var itemsInViewport = testController.GetItemsInViewport();
        Assert.Contains(testController.Items[targetItemIndex], itemsInViewport);
        TestUtil.AssertContainerBounds(testController, itemsInViewport);
    }

    [WpfTheory]
    [InlineData(VirtualizationMode.Standard)]
    [InlineData(VirtualizationMode.Recycling)]
    public async Task MakeVisible_AlreadyFullyVisible(VirtualizationMode virtualizationMode)
    {
        await testController.SetVirtualizationModeAsync(virtualizationMode);

        var targetContainer = testController.Children[5];
        var visibleRect = await testController.MakeVisibleAsync(targetContainer, new Rect(0, 0, testController.FirstChildWidth, testController.FirstChildHeight));

        Assert.Equal(new Rect(0, 0, 100, 100), visibleRect);
        Assert.Equal(0, testController.VerticalOffset);
    }

    [WpfTheory]
    [InlineData(VirtualizationMode.Standard)]
    [InlineData(VirtualizationMode.Recycling)]
    public async Task MakeVisible_PartiallyVisibleAtStartOfViewport(VirtualizationMode virtualizationMode)
    {
        await testController.SetVirtualizationModeAsync(virtualizationMode);
        await testController.SetVerticalOffsetAsync(50);

        var targetContainer = testController.Children[0];
        var visibleRect = await testController.MakeVisibleAsync(targetContainer, new Rect(0, 0, testController.FirstChildWidth, testController.FirstChildHeight));

        Assert.Equal(new Rect(0, 0, 100, 100), visibleRect);
        Assert.Equal(0, testController.VerticalOffset);
    }

    [WpfTheory]
    [InlineData(VirtualizationMode.Standard)]
    [InlineData(VirtualizationMode.Recycling)]
    public async Task MakeVisible_PartiallyVisibleAtEndOfViewport(VirtualizationMode virtualizationMode)
    {
        await testController.SetVirtualizationModeAsync(virtualizationMode);
        await testController.SetVerticalOffsetAsync(50);

        var targetContainer = testController.Children[21];
        var visibleRect = await testController.MakeVisibleAsync(targetContainer, new Rect(0, 0, testController.FirstChildWidth, testController.FirstChildHeight));

        Assert.Equal(new Rect(0, 0, 100, 100), visibleRect);
        Assert.Equal(100, testController.VerticalOffset);
    }

    [WpfTheory]
    [InlineData(VirtualizationMode.Standard)]
    [InlineData(VirtualizationMode.Recycling)]
    public async Task MakeVisible_PartiallyVisibleAtStartOfViewportAndGreaterThanViewport(VirtualizationMode virtualizationMode)
    {
        await testController.SetVirtualizationModeAsync(virtualizationMode);
        var itemSize = new Size(100, testController.ViewportHeight + 100 /*500*/);
        await testController.SetItemSizeAsync(itemSize);
        await testController.SetVerticalOffsetAsync(250);

        var targetContainer = testController.Children[0];
        var visibleRect = await testController.MakeVisibleAsync(targetContainer, new Rect(0, 0, itemSize.Width, itemSize.Height));

        Assert.Equal(new Rect(0, 0, itemSize.Width, testController.ViewportHeight), visibleRect);
        Assert.Equal(0, testController.VerticalOffset);
    }

    [WpfTheory]
    [InlineData(VirtualizationMode.Standard)]
    [InlineData(VirtualizationMode.Recycling)]
    public async Task MakeVisible_PartiallyVisibleAtEndOfViewportAndGreaterThanViewport(VirtualizationMode virtualizationMode)
    {
        await testController.SetVirtualizationModeAsync(virtualizationMode);
        var itemSize = new Size(100, testController.ViewportHeight + 100 /*500*/);
        await testController.SetItemSizeAsync(itemSize);
        await testController.SetVerticalOffsetAsync(250);

        var targetContainer = testController.Children[5];
        var visibleRect = await testController.MakeVisibleAsync(targetContainer, new Rect(0, 0, itemSize.Width, itemSize.Height));

        Assert.Equal(new Rect(0, 0, itemSize.Width, testController.ViewportHeight), visibleRect);
        Assert.Equal(500, testController.VerticalOffset);
    }

    //[WpfTheory]
    //[InlineData(VirtualizationMode.Standard)]
    //[InlineData(VirtualizationMode.Recycling)]
    //public async Task ScrollBarVisibility_Auto(VirtualizationMode virtualizationMode)
    //{
    //    var testController = TestController.CreateListBoxWithVirtualizingWrapPanel();
    //    await testController.ResizePanelAsync(510, 400);
    //    await testController.SetVirtualizationMode(virtualizationMode);
    //    await testController.SetScrollUnit(ScrollUnit.Pixel);   
    //    await testController.SetVerticalScrollBarVisibility(ScrollBarVisibility.Auto);

    //    //for (int i = 0; i < 40; i++)
    //    //{
    //    //    await testController.ScrollMouseWheelDownAsync();
    //    //}

    //    //await testController.SetVerticalOffsetAsync(testController.ExtentHeight - testController.ViewportHeight);

    //    Assert.Equal(4, testController.ItemsPerRow);
    //    Assert.Equal(testController.ExtentHeight - testController.ViewportHeight, testController.VerticalOffset);
    //}
}

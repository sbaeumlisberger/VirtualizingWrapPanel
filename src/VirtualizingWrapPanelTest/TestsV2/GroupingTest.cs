using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using Xunit;
using Xunit.Sdk;

namespace VirtualizingWrapPanelTest.TestsV2;

public class GroupingTest
{
    [WpfTheory]
    [InlineData(VirtualizationMode.Standard)]
    [InlineData(VirtualizationMode.Recycling)]
    public async Task ScrollIntoView_ItemAfterViewportAndCache(VirtualizationMode virtualizationMode)
    {
        var testController = TestController.CreateListBoxWithVirtualizingWrapPanel();
        await testController.SetVirtualizationModeAsync(virtualizationMode);
        await testController.SetGroupingAsync(true);

        var item = testController.Items[549];// Item 550
        await testController.ScrollIntoViewAsync(item);

        Assert.Contains(item, testController.GetItemsInViewport());
        Assert.Equal(TestController.DefaultItemsControlHeight, testController.GetContainerBounds(item).Bottom, 0.01);
    }

    [WpfTheory]
    [InlineData(VirtualizationMode.Standard)]
    [InlineData(VirtualizationMode.Recycling)]
    public async Task ScrollIntoView_ItemBeforeViewportAndCache(VirtualizationMode virtualizationMode)
    {
        var testController = TestController.CreateListBoxWithVirtualizingWrapPanel();
        await testController.SetVirtualizationModeAsync(virtualizationMode);
        await testController.SetGroupingAsync(true);
        await testController.ScrollToEndAsync();
        Assert.Contains(testController.Items[^1], testController.GetItemsInViewport());

        var item = testController.Items[549];// Item 550
        await testController.ScrollIntoViewAsync(item);

        Assert.Contains(item, testController.GetItemsInViewport());
        Assert.Equal(0, testController.GetContainerBounds(item).Top, 0.01);
    }

    [WpfTheory]
    [InlineData(VirtualizationMode.Standard)]
    [InlineData(VirtualizationMode.Recycling)]
    public async Task ScrollIntoView_ItemAfterViewportAndCache_DifferentGroupSizes(VirtualizationMode virtualizationMode)
    {
        var testController = TestController.CreateListBoxWithVirtualizingWrapPanel(TestController.GenerateItemsWithRandomGroupSizes());
        await testController.SetVirtualizationModeAsync(virtualizationMode);
        await testController.SetGroupingAsync(true);

        var item = testController.Items[549];// Item 550
        await testController.ScrollIntoViewAsync(item);

        Assert.Contains(item, testController.GetItemsInViewport());
        Assert.Equal(TestController.DefaultItemsControlHeight, testController.GetContainerBounds(item).Bottom, 0.01);
    }


    [WpfTheory]
    [InlineData(VirtualizationMode.Standard)]
    [InlineData(VirtualizationMode.Recycling)]
    public async Task ScrollIntoView_ItemBeforeViewportAndCache_DifferentGroupSizes(VirtualizationMode virtualizationMode)
    {
        var testController = TestController.CreateListBoxWithVirtualizingWrapPanel(TestController.GenerateItemsWithRandomGroupSizes());
        await testController.SetVirtualizationModeAsync(virtualizationMode);
        await testController.SetGroupingAsync(true);
        await testController.ScrollToEndAsync();
        Assert.Contains(testController.Items[^1], testController.GetItemsInViewport());

        var item = testController.Items[549];// Item 550
        await testController.ScrollIntoViewAsync(item);

        Assert.Contains(item, testController.GetItemsInViewport());
        Assert.Equal(0, testController.GetContainerBounds(item).Top, 0.01);
    }

    [WpfTheory]
    [InlineData(VirtualizationMode.Standard)]
    [InlineData(VirtualizationMode.Recycling)]
    public async Task VerticalOrientation_ScrollPageRight(VirtualizationMode virtualizationMode)
    {
        var testController = TestController.CreateListBoxWithVirtualizingWrapPanel(TestController.GenerateItemsWithRandomGroupSizes());
        await testController.SetVirtualizationModeAsync(virtualizationMode);
        await testController.SetGroupingAsync(true);
        await testController.SetOrientationAsync(Orientation.Vertical);
        await testController.SetScrollUnitAsync(ScrollUnit.Pixel);

        // TODO: why?
        await testController.SetHorizontalScrollBarVisibility(ScrollBarVisibility.Auto);

        await testController.ScrollPageRightAsync();

        Assert.Equal(TestController.DefaultItemsControlWidth, testController.HorizontalOffset);
    }
}

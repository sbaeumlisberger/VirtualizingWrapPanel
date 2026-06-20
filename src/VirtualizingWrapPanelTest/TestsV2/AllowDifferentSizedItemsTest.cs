using System.Windows.Controls;
using Xunit;

namespace VirtualizingWrapPanelTest.TestsV2;

public class AllowDifferentSizedItemsTest
{
    [WpfTheory]
    [InlineData(VirtualizationMode.Standard)]
    [InlineData(VirtualizationMode.Recycling)]
    public async Task ScrollIntoView_AfterViewportAndCache(VirtualizationMode virtualizationMode)
    {
        var testController = TestController.CreateListBoxWithVirtualizingWrapPanel(TestController.GenerateDifferentSizedItems());
        await testController.SetVirtualizationModeAsync(virtualizationMode);
        await testController.SetAllowDifferentSizedItemsAsync(true);

        var item = testController.Items[549]; // Item 550
        await testController.ScrollIntoViewAsync(item);

        // an exact position in the viewport is not guaranteed
        Assert.Contains(item, testController.GetItemsInViewport());
    }

    [WpfTheory]
    [InlineData(VirtualizationMode.Standard)]
    [InlineData(VirtualizationMode.Recycling)]
    public async Task ScrollIntoView_BeforeViewportAndCache(VirtualizationMode virtualizationMode)
    {
        var testController = TestController.CreateListBoxWithVirtualizingWrapPanel(TestController.GenerateDifferentSizedItems());
        await testController.SetVirtualizationModeAsync(virtualizationMode);
        await testController.SetAllowDifferentSizedItemsAsync(true);
        await testController.ScrollToEndAsync();
        Assert.Contains(testController.Items[^1], testController.GetItemsInViewport());

        var item = testController.Items[549]; // Item 550
        await testController.ScrollIntoViewAsync(item);

        // an exact position in the viewport is not guaranteed
        Assert.Contains(item, testController.GetItemsInViewport());
    }

    [WpfTheory]
    [InlineData(VirtualizationMode.Standard)]
    [InlineData(VirtualizationMode.Recycling)]
    public async Task ScrollIntoView_AfterViewportAndCache_Grouping(VirtualizationMode virtualizationMode)
    {
        var testController = TestController.CreateListBoxWithVirtualizingWrapPanel(TestController.GenerateDifferentSizedItems());
        await testController.SetVirtualizationModeAsync(virtualizationMode);
        await testController.SetAllowDifferentSizedItemsAsync(true);
        await testController.SetGroupingAsync(true);

        var item = testController.Items[549]; // Item 550
        await testController.ScrollIntoViewAsync(item);

        // an exact position in the viewport is not guaranteed
        Assert.Contains(item, testController.GetItemsInViewport());
    }


    [WpfTheory]
    [InlineData(VirtualizationMode.Standard)]
    [InlineData(VirtualizationMode.Recycling)]
    public async Task ScrollIntoView_BeforeViewportAndCache_Grouping(VirtualizationMode virtualizationMode)
    {
        var testController = TestController.CreateListBoxWithVirtualizingWrapPanel(TestController.GenerateDifferentSizedItems());
        await testController.SetVirtualizationModeAsync(virtualizationMode);
        await testController.SetAllowDifferentSizedItemsAsync(true);
        await testController.SetGroupingAsync(true);
        await testController.ScrollToEndAsync();
        Assert.Contains(testController.Items[^1], testController.GetItemsInViewport());

        var item = testController.Items[549]; // Item 550
        await testController.ScrollIntoViewAsync(item);

        // an exact position in the viewport is not guaranteed
        Assert.Contains(item, testController.GetItemsInViewport());
    }

    // TODO different large groups


}

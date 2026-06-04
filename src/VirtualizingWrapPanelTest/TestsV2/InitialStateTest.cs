using System.Collections.ObjectModel;
using System.Windows.Controls;
using Xunit;

namespace VirtualizingWrapPanelTest.TestsV2;

public class InitialStateTest
{
    [WpfTheory]
    [InlineData(VirtualizationMode.Standard)]
    [InlineData(VirtualizationMode.Recycling)]
    public async Task MoreItemsThanFittingInViewport(VirtualizationMode virtualizationMode)
    {
        var testController = TestController.CreateListBoxWithVirtualizingWrapPanel();
        await testController.SetVirtualizationModeAsync(virtualizationMode);

        // do nothing, just verify initial state

        Assert.Equal(TestController.DefaultItemsControlWidth, testController.DesiredSize.Width);
        Assert.Equal(TestController.DefaultItemsControlHeight, testController.DesiredSize.Height);
        Assert.Equal(TestController.DefaultItemsControlWidth, testController.ViewportWidth);
        Assert.Equal(TestController.DefaultItemsControlHeight, testController.ViewportHeight);
        var expectedExtent = TestUtil.GetExpectedExtent(testController);
        Assert.Equal(expectedExtent.Width, testController.ExtentWidth);
        Assert.Equal(expectedExtent.Height, testController.ExtentHeight);
        var itemsInViewport = testController.GetItemsInViewport();
        Assert.Equal(testController.ItemsPerPage, itemsInViewport.Count);
        TestUtil.AssertContainerBounds(testController, itemsInViewport);
    }

    [WpfTheory]
    [InlineData(VirtualizationMode.Standard)]
    [InlineData(VirtualizationMode.Recycling)]
    public async Task LessItemsThanFittingInViewport(VirtualizationMode virtualizationMode)
    {
        var items = TestController.GenerateItems(8);
        var testController = TestController.CreateListBoxWithVirtualizingWrapPanel(items);
        await testController.SetVirtualizationModeAsync(virtualizationMode);

        // do nothing, just verify initial state

        Assert.Equal(TestController.DefaultItemsControlWidth, testController.DesiredSize.Width);
        Assert.Equal(2 * testController.FirstChildHeight, testController.DesiredSize.Height);
        Assert.Equal(TestController.DefaultItemsControlWidth, testController.ViewportWidth);
        Assert.Equal(TestController.DefaultItemsControlHeight, testController.ViewportHeight);
        Assert.Equal(TestController.DefaultItemsControlWidth, testController.ExtentWidth);
        Assert.Equal(2 * testController.FirstChildHeight, testController.ExtentHeight);
        var itemsInViewport = testController.GetItemsInViewport();
        Assert.Equal(items.Count, itemsInViewport.Count);
        TestUtil.AssertContainerBounds(testController, itemsInViewport);
    }

    [WpfTheory]
    [InlineData(VirtualizationMode.Standard)]
    [InlineData(VirtualizationMode.Recycling)]
    public async Task OneItem(VirtualizationMode virtualizationMode)
    {
        var items = TestController.GenerateItems(1);
        var testController = TestController.CreateListBoxWithVirtualizingWrapPanel(items);
        await testController.SetVirtualizationModeAsync(virtualizationMode);

        // do nothing, just verify initial state

        Assert.Equal(testController.FirstChildWidth, testController.DesiredSize.Width);
        Assert.Equal(testController.FirstChildHeight, testController.DesiredSize.Height);
        Assert.Equal(TestController.DefaultItemsControlWidth, testController.ViewportWidth);
        Assert.Equal(TestController.DefaultItemsControlHeight, testController.ViewportHeight);
        Assert.Equal(testController.FirstChildWidth, testController.ExtentWidth);
        Assert.Equal(testController.FirstChildHeight, testController.ExtentHeight);
        var itemsInViewport = testController.GetItemsInViewport();
        Assert.Single(itemsInViewport);
        TestUtil.AssertContainerBounds(testController, itemsInViewport);
    }

    [WpfTheory]
    [InlineData(VirtualizationMode.Standard)]
    [InlineData(VirtualizationMode.Recycling)]
    public async Task ZeroItems(VirtualizationMode virtualizationMode)
    {
        var items = new ObservableCollection<TestItem>();
        var testController = TestController.CreateListBoxWithVirtualizingWrapPanel(items);
        await testController.SetVirtualizationModeAsync(virtualizationMode);

        // do nothing, just verify initial state

        Assert.Equal(0, testController.DesiredSize.Width);
        Assert.Equal(0, testController.DesiredSize.Height);
        Assert.Equal(TestController.DefaultItemsControlWidth, testController.ViewportWidth);
        Assert.Equal(TestController.DefaultItemsControlHeight, testController.ViewportHeight);
        Assert.Equal(0, testController.ExtentWidth);
        Assert.Equal(0, testController.ExtentHeight);
        Assert.Empty(testController.GetItemsInViewport());
    }

    [WpfFact]
    public async Task ItemsNotDistinct()
    {
        var items = Enumerable.Repeat(new TestItem("TestItem", 100, 100), 100).ToList();

        var tc = TestController.CreateListBoxWithVirtualizingWrapPanel(items);
        await tc.UpdateLayoutAsync();

        await Task.Delay(1000);
        Assert.Equal(20 + 1, tc.Children.Count);
    }
}

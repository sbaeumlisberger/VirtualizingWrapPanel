using System.Windows.Controls;
using Xunit;

namespace VirtualizingWrapPanelTest.TestsV2;

public class ResizeTest
{
    private readonly TestController testController = TestController.CreateListBoxWithVirtualizingWrapPanel();

    [WpfTheory]
    [InlineData(VirtualizationMode.Standard)]
    [InlineData(VirtualizationMode.Recycling)]
    public async Task IncreaseWidth_NoScrollOffset(VirtualizationMode virtualizationMode)
    {
        await testController.SetVirtualizationModeAsync(virtualizationMode);

        double newWidth = 6 * testController.FirstChildWidth;
        await testController.ResizePanelAsync(newWidth, testController.ViewportHeight);

        var expectedExtent = TestUtil.GetExpectedExtent(testController);
        Assert.Equal(expectedExtent.Height, testController.ExtentHeight);
        Assert.Equal(expectedExtent.Width, testController.ExtentWidth);
        Assert.Equal(newWidth, testController.ViewportWidth);
        Assert.Equal(0, testController.VerticalOffset);
        Assert.Equal(6, testController.ItemsPerRow);
        Assert.Equal(4 * 6, testController.ItemsPerPage);
        var itemsInViewport = testController.GetItemsInViewport();
        Assert.Equal(4 * 6, itemsInViewport.Count);
        Assert.Equal(testController.Items[0], itemsInViewport.First());
        TestUtil.AssertContainerBounds(testController, itemsInViewport);
    }

    [WpfTheory]
    [InlineData(VirtualizationMode.Standard)]
    [InlineData(VirtualizationMode.Recycling)]
    public async Task DecreaseWidth_NoScrollOffset(VirtualizationMode virtualizationMode)
    {
        await testController.SetVirtualizationModeAsync(virtualizationMode);

        double newWidth = 4 * testController.FirstChildWidth;
        await testController.ResizePanelAsync(newWidth, testController.ViewportHeight);

        var expectedExtent = TestUtil.GetExpectedExtent(testController);
        Assert.Equal(expectedExtent.Height, testController.ExtentHeight);
        Assert.Equal(expectedExtent.Width, testController.ExtentWidth);
        Assert.Equal(newWidth, testController.ViewportWidth);
        Assert.Equal(0, testController.VerticalOffset);
        Assert.Equal(4, testController.ItemsPerRow);
        Assert.Equal(4 * 4, testController.ItemsPerPage);
        var itemsInViewport = testController.GetItemsInViewport();
        Assert.Equal(4 * 4, itemsInViewport.Count);
        Assert.Equal(testController.Items[0], itemsInViewport.First());
        TestUtil.AssertContainerBounds(testController, itemsInViewport);
    }

    [WpfTheory]
    [InlineData(VirtualizationMode.Standard)]
    [InlineData(VirtualizationMode.Recycling)]
    public async Task IncreaseWidth_WithScrollOffset(VirtualizationMode virtualizationMode)
    {
        await testController.SetVirtualizationModeAsync(virtualizationMode);
        await testController.SetVerticalOffsetAsync(1000);
        double verticalScrollOffsetBeforeResize = testController.VerticalOffset;

        double newWidth = 6 * testController.FirstChildWidth;
        await testController.ResizePanelAsync(newWidth, testController.ViewportHeight);

        var expectedExtent = TestUtil.GetExpectedExtent(testController);
        Assert.Equal(expectedExtent.Height, testController.ExtentHeight);
        Assert.Equal(expectedExtent.Width, testController.ExtentWidth);
        Assert.Equal(newWidth, testController.ViewportWidth);
        Assert.Equal(verticalScrollOffsetBeforeResize, testController.VerticalOffset);
        Assert.Equal(6, testController.ItemsPerRow);
        Assert.Equal(4 * 6, testController.ItemsPerPage);
        var itemsInViewport = testController.GetItemsInViewport();
        Assert.Equal(4 * 6, itemsInViewport.Count);
        Assert.Equal(testController.Items[60], itemsInViewport.First());
        TestUtil.AssertContainerBounds(testController, itemsInViewport);
    }

    [WpfTheory]
    [InlineData(VirtualizationMode.Standard)]
    [InlineData(VirtualizationMode.Recycling)]
    public async Task DecreaseWidth_WithScrollOffset(VirtualizationMode virtualizationMode)
    {
        await testController.SetVirtualizationModeAsync(virtualizationMode);
        await testController.SetVerticalOffsetAsync(1000);
        double verticalScrollOffsetBeforeResize = testController.VerticalOffset;

        double newWidth = 4 * testController.FirstChildWidth;
        await testController.ResizePanelAsync(newWidth, testController.ViewportHeight);

        var expectedExtent = TestUtil.GetExpectedExtent(testController);
        Assert.Equal(expectedExtent.Height, testController.ExtentHeight);
        Assert.Equal(expectedExtent.Width, testController.ExtentWidth);
        Assert.Equal(newWidth, testController.ViewportWidth);
        Assert.Equal(verticalScrollOffsetBeforeResize, testController.VerticalOffset);
        Assert.Equal(4, testController.ItemsPerRow);
        Assert.Equal(4 * 4, testController.ItemsPerPage);
        var itemsInViewport = testController.GetItemsInViewport();
        Assert.Equal(4 * 4, itemsInViewport.Count);
        Assert.Equal(testController.Items[40], itemsInViewport.First());
        TestUtil.AssertContainerBounds(testController, itemsInViewport);
    }

    [WpfTheory]
    [InlineData(VirtualizationMode.Standard)]
    [InlineData(VirtualizationMode.Recycling)]
    public async Task IncreaseWidth_ScrolledToEnd(VirtualizationMode virtualizationMode)
    {
        await testController.SetVirtualizationModeAsync(virtualizationMode);
        await testController.ScrollToEndAsync();
        double verticalScrollOffsetBeforeResize = testController.VerticalOffset;

        double newWidth = 6 * testController.FirstChildWidth;
        await testController.ResizePanelAsync(newWidth, testController.ViewportHeight);
        
        var expectedExtent = TestUtil.GetExpectedExtent(testController);
        Assert.Equal(expectedExtent.Height, testController.ExtentHeight);
        Assert.Equal(expectedExtent.Width, testController.ExtentWidth);
        Assert.Equal(newWidth, testController.ViewportWidth);
        Assert.Equal(testController.ExtentHeight - testController.ViewportHeight, testController.VerticalOffset);
        Assert.Equal(6, testController.ItemsPerRow);
        Assert.Equal(4 * 6, testController.ItemsPerPage);
        var itemsInViewport = testController.GetItemsInViewport();
        Assert.Equal(3 * 6 + 4, itemsInViewport.Count); // three full lines and one partial line
        Assert.Equal(testController.Items[testController.Items.Count - itemsInViewport.Count], itemsInViewport.First());
        TestUtil.AssertContainerBounds(testController, itemsInViewport);
    }

    [WpfTheory]
    [InlineData(VirtualizationMode.Standard)]
    [InlineData(VirtualizationMode.Recycling)]
    public async Task DecreaseWidth_ScrolledToEnd(VirtualizationMode virtualizationMode)
    {
        await testController.SetVirtualizationModeAsync(virtualizationMode);
        await testController.ScrollToEndAsync();
        double verticalScrollOffsetBeforeResize = testController.VerticalOffset;

        double newWidth = 4 * testController.FirstChildWidth;
        await testController.ResizePanelAsync(newWidth, testController.ViewportHeight);

        var expectedExtent = TestUtil.GetExpectedExtent(testController);
        Assert.Equal(expectedExtent.Height, testController.ExtentHeight);
        Assert.Equal(expectedExtent.Width, testController.ExtentWidth);
        Assert.Equal(newWidth, testController.ViewportWidth);
        Assert.Equal(verticalScrollOffsetBeforeResize, testController.VerticalOffset);
        Assert.Equal(4, testController.ItemsPerRow);
        Assert.Equal(4 * 4, testController.ItemsPerPage);
        var itemsInViewport = testController.GetItemsInViewport();
        Assert.Equal(4 * 4, itemsInViewport.Count);
        Assert.Equal(testController.Items[7984], itemsInViewport.First());
        TestUtil.AssertContainerBounds(testController, itemsInViewport);
    }

    [WpfTheory]
    [InlineData(VirtualizationMode.Standard)]
    [InlineData(VirtualizationMode.Recycling)]
    public async Task IncreaseHeight_NoScrollOffset(VirtualizationMode virtualizationMode)
    {
        await testController.SetVirtualizationModeAsync(virtualizationMode);

        double newHeight = 5 * testController.FirstChildHeight;
        await testController.ResizePanelAsync(testController.ViewportWidth, newHeight);

        var expectedExtent = TestUtil.GetExpectedExtent(testController);
        Assert.Equal(expectedExtent.Height, testController.ExtentHeight);
        Assert.Equal(expectedExtent.Width, testController.ExtentWidth);
        Assert.Equal(newHeight, testController.ViewportHeight);
        Assert.Equal(0, testController.VerticalOffset);
        Assert.Equal(5, testController.ItemsPerRow);
        Assert.Equal(5 * 5, testController.ItemsPerPage);
        var itemsInViewport = testController.GetItemsInViewport();
        Assert.Equal(5 * 5, itemsInViewport.Count);
        Assert.Equal(testController.Items[0], itemsInViewport.First());
        TestUtil.AssertContainerBounds(testController, itemsInViewport);
    }

    [WpfTheory]
    [InlineData(VirtualizationMode.Standard)]
    [InlineData(VirtualizationMode.Recycling)]
    public async Task DecreaseHeight_NoScrollOffset(VirtualizationMode virtualizationMode)
    {
        await testController.SetVirtualizationModeAsync(virtualizationMode);

        double newHeight = 3 * testController.FirstChildHeight;
        await testController.ResizePanelAsync(testController.ViewportWidth, newHeight);

        var expectedExtent = TestUtil.GetExpectedExtent(testController);
        Assert.Equal(expectedExtent.Height, testController.ExtentHeight);
        Assert.Equal(expectedExtent.Width, testController.ExtentWidth);
        Assert.Equal(newHeight, testController.ViewportHeight);
        Assert.Equal(0, testController.VerticalOffset);
        Assert.Equal(0, testController.VerticalOffset);
        Assert.Equal(5, testController.ItemsPerRow);
        Assert.Equal(3 * 5, testController.ItemsPerPage);
        var itemsInViewport = testController.GetItemsInViewport();
        Assert.Equal(3 * 5, itemsInViewport.Count);
        Assert.Equal(testController.Items[0], itemsInViewport.First());
        TestUtil.AssertContainerBounds(testController, itemsInViewport);
    }

    [WpfTheory]
    [InlineData(VirtualizationMode.Standard)]
    [InlineData(VirtualizationMode.Recycling)]
    public async Task IncreaseHeight_WithScrollOffset(VirtualizationMode virtualizationMode)
    {
        await testController.SetVirtualizationModeAsync(virtualizationMode);
        await testController.SetVerticalOffsetAsync(1000);
        double verticalScrollOffsetBeforeResize = testController.VerticalOffset;

        double newHeight = 5 * testController.FirstChildHeight;
        await testController.ResizePanelAsync(testController.ViewportWidth, newHeight);

        var expectedExtent = TestUtil.GetExpectedExtent(testController);
        Assert.Equal(expectedExtent.Height, testController.ExtentHeight);
        Assert.Equal(expectedExtent.Width, testController.ExtentWidth);
        Assert.Equal(newHeight, testController.ViewportHeight);
        Assert.Equal(verticalScrollOffsetBeforeResize, testController.VerticalOffset);
        Assert.Equal(5, testController.ItemsPerRow);
        Assert.Equal(5 * 5, testController.ItemsPerPage);
        var itemsInViewport = testController.GetItemsInViewport();
        Assert.Equal(5 * 5, itemsInViewport.Count);
        Assert.Equal(testController.Items[50], itemsInViewport.First());
        TestUtil.AssertContainerBounds(testController, itemsInViewport);
    }

    [WpfTheory]
    [InlineData(VirtualizationMode.Standard)]
    [InlineData(VirtualizationMode.Recycling)]
    public async Task DecreaseHeight_WithScrollOffset(VirtualizationMode virtualizationMode)
    {
        await testController.SetVirtualizationModeAsync(virtualizationMode);
        await testController.SetVerticalOffsetAsync(1000);
        double verticalScrollOffsetBeforeResize = testController.VerticalOffset;

        double newHeight = 3 * testController.FirstChildHeight;
        await testController.ResizePanelAsync(testController.ViewportWidth, newHeight);

        var expectedExtent = TestUtil.GetExpectedExtent(testController);
        Assert.Equal(expectedExtent.Height, testController.ExtentHeight);
        Assert.Equal(expectedExtent.Width, testController.ExtentWidth);
        Assert.Equal(newHeight, testController.ViewportHeight);
        Assert.Equal(verticalScrollOffsetBeforeResize, testController.VerticalOffset);
        Assert.Equal(5, testController.ItemsPerRow);
        Assert.Equal(3 * 5, testController.ItemsPerPage);
        var itemsInViewport = testController.GetItemsInViewport();
        Assert.Equal(3 * 5, itemsInViewport.Count);
        Assert.Equal(testController.Items[50], itemsInViewport.First());
        TestUtil.AssertContainerBounds(testController, itemsInViewport);
    }

    [WpfTheory]
    [InlineData(VirtualizationMode.Standard)]
    [InlineData(VirtualizationMode.Recycling)]
    public async Task IncreaseHeight_ScrolledToEnd(VirtualizationMode virtualizationMode)
    {
        await testController.SetVirtualizationModeAsync(virtualizationMode);
        await testController.ScrollToEndAsync();
        double verticalScrollOffsetBeforeResize = testController.VerticalOffset;

        double newHeight = 5 * testController.FirstChildHeight;
        await testController.ResizePanelAsync(testController.ViewportWidth, newHeight);

        var expectedExtent = TestUtil.GetExpectedExtent(testController);
        Assert.Equal(expectedExtent.Height, testController.ExtentHeight);
        Assert.Equal(expectedExtent.Width, testController.ExtentWidth);
        Assert.Equal(newHeight, testController.ViewportHeight);
        Assert.Equal(verticalScrollOffsetBeforeResize - testController.FirstChildHeight, testController.VerticalOffset);
        Assert.Equal(5, testController.ItemsPerRow);
        Assert.Equal(5 * 5, testController.ItemsPerPage);
        var itemsInViewport = testController.GetItemsInViewport();
        Assert.Equal(5 * 5, itemsInViewport.Count);
        Assert.Equal(testController.Items[testController.Items.Count - itemsInViewport.Count], itemsInViewport.First());
        TestUtil.AssertContainerBounds(testController, itemsInViewport);
    }

    [WpfTheory]
    [InlineData(VirtualizationMode.Standard)]
    [InlineData(VirtualizationMode.Recycling)]
    public async Task DecreaseHeight_ScrolledToEnd(VirtualizationMode virtualizationMode)
    {
        await testController.SetVirtualizationModeAsync(virtualizationMode);
        await testController.ScrollToEndAsync();
        double verticalScrollOffsetBeforeResize = testController.VerticalOffset;

        double newHeight = 3 * testController.FirstChildHeight;
        await testController.ResizePanelAsync(testController.ViewportWidth, newHeight);

        var expectedExtent = TestUtil.GetExpectedExtent(testController);
        Assert.Equal(expectedExtent.Height, testController.ExtentHeight);
        Assert.Equal(expectedExtent.Width, testController.ExtentWidth);
        Assert.Equal(newHeight, testController.ViewportHeight);
        Assert.Equal(verticalScrollOffsetBeforeResize, testController.VerticalOffset);
        Assert.Equal(5, testController.ItemsPerRow);
        Assert.Equal(3 * 5, testController.ItemsPerPage);
        var itemsInViewport = testController.GetItemsInViewport();
        Assert.Equal(3 * 5, itemsInViewport.Count);
        Assert.Equal(testController.Items[testController.Items.Count - 4 * testController.ItemsPerRow], itemsInViewport.First());
        TestUtil.AssertContainerBounds(testController, itemsInViewport);
    }

    // TODO test auto hide scroll bar
}

using System.Collections.ObjectModel;
using System.Windows;
using WpfToolkit.Controls;
using Xunit;
using Assert = Xunit.Assert;

namespace VirtualizingWrapPanelTest.TestsV2;

public class SpacingModeTest
{
    private readonly TestController tc = TestController.CreateListBoxWithVirtualizingWrapPanel(
        items: TestController.GenerateItems(itemCount: 6),
        width: 560,
        height: 400);

    [WpfFact]
    public void Uniform()
    {
        // Uniform is the default value

        Assert.Equal(500, tc.DesiredSize.Width);
        Assert.Equal(new Point(10, 0), tc.GetContainerPosition("Item 1"));
        Assert.Equal(new Point(120, 0), tc.GetContainerPosition("Item 2"));
        Assert.Equal(new Point(230, 0), tc.GetContainerPosition("Item 3"));
        Assert.Equal(new Point(340, 0), tc.GetContainerPosition("Item 4"));
        Assert.Equal(new Point(450, 0), tc.GetContainerPosition("Item 5"));
        Assert.Equal(new Point(10, 100), tc.GetContainerPosition("Item 6"));
    }

    [WpfFact]
    public async Task Uniform_HorizontalAlignmentCenter()
    {
        await tc.SetHorizontalAlignmentAsync(HorizontalAlignment.Center);

        Assert.Equal(500, tc.DesiredSize.Width);
        Assert.Equal(new Point(30, 0), tc.GetVirtualizingWrapPanelPosition());
        Assert.Equal(new Point(0, 0), tc.GetContainerPosition("Item 1"));
        Assert.Equal(new Point(100, 0), tc.GetContainerPosition("Item 2"));
        Assert.Equal(new Point(200, 0), tc.GetContainerPosition("Item 3"));
        Assert.Equal(new Point(300, 0), tc.GetContainerPosition("Item 4"));
        Assert.Equal(new Point(400, 0), tc.GetContainerPosition("Item 5"));
        Assert.Equal(new Point(0, 100), tc.GetContainerPosition("Item 6"));
    }

    [WpfFact]
    public async Task Uniform_HorizontalAlignmentRight()
    {
        await tc.SetHorizontalAlignmentAsync(HorizontalAlignment.Right);

        Assert.Equal(500, tc.DesiredSize.Width);
        Assert.Equal(new Point(60, 0), tc.GetVirtualizingWrapPanelPosition());
        Assert.Equal(new Point(0, 0), tc.GetContainerPosition("Item 1"));
        Assert.Equal(new Point(100, 0), tc.GetContainerPosition("Item 2"));
        Assert.Equal(new Point(200, 0), tc.GetContainerPosition("Item 3"));
        Assert.Equal(new Point(300, 0), tc.GetContainerPosition("Item 4"));
        Assert.Equal(new Point(400, 0), tc.GetContainerPosition("Item 5"));
        Assert.Equal(new Point(0, 100), tc.GetContainerPosition("Item 6"));
    }

    [WpfFact]
    public async Task Uniform_HorizontalAlignmentLeft()
    {
        await tc.SetHorizontalAlignmentAsync(HorizontalAlignment.Left);

        Assert.Equal(500, tc.DesiredSize.Width);
        Assert.Equal(new Point(0, 0), tc.GetVirtualizingWrapPanelPosition());
        Assert.Equal(new Point(0, 0), tc.GetContainerPosition("Item 1"));
        Assert.Equal(new Point(100, 0), tc.GetContainerPosition("Item 2"));
        Assert.Equal(new Point(200, 0), tc.GetContainerPosition("Item 3"));
        Assert.Equal(new Point(300, 0), tc.GetContainerPosition("Item 4"));
        Assert.Equal(new Point(400, 0), tc.GetContainerPosition("Item 5"));
        Assert.Equal(new Point(0, 100), tc.GetContainerPosition("Item 6"));
    }

    [WpfFact]
    public async Task Uniform_DifferentSizedItems()
    {
        var items = new ObservableCollection<TestItem>([
            new TestItem("Item 1", 85, 70),
            new TestItem("Item 2", 90, 95),
            new TestItem("Item 3", 100, 75),
            new TestItem("Item 4", 75, 60),
            new TestItem("Item 5", 95, 90),
            new TestItem("Item 6", 80, 40),
            new TestItem("Item 7", 70, 80),
        ]);
        var tc = TestController.CreateListBoxWithVirtualizingWrapPanel(items: items, width: 560, height: 400);

        await tc.SetAllowDifferentSizedItemsAsync(true);

        //Assert.Equal(525, vwp.DesiredSize.Width);
        Assert.Equal(new Rect(5, 0, 85, 70), tc.GetContainerBounds("Item 1"));
        Assert.Equal(new Rect(95, 0, 90, 95), tc.GetContainerBounds("Item 2"));
        Assert.Equal(new Rect(190, 0, 100, 75), tc.GetContainerBounds("Item 3"));
        Assert.Equal(new Rect(295, 0, 75, 60), tc.GetContainerBounds("Item 4"));
        Assert.Equal(new Rect(375, 0, 95, 90), tc.GetContainerBounds("Item 5"));
        Assert.Equal(new Rect(475, 0, 80, 40), tc.GetContainerBounds("Item 6"));
        Assert.Equal(new Rect(245, 95, 70, 80), tc.GetContainerBounds("Item 7"));
    }

    [WpfFact]
    public async Task None()
    {
        await tc.SetSpacingModeAsync(SpacingMode.None);

        Assert.Equal(500, tc.DesiredSize.Width);
        Assert.Equal(new Point(0, 0), tc.GetContainerPosition("Item 1"));
        Assert.Equal(new Point(100, 0), tc.GetContainerPosition("Item 2"));
        Assert.Equal(new Point(200, 0), tc.GetContainerPosition("Item 3"));
        Assert.Equal(new Point(300, 0), tc.GetContainerPosition("Item 4"));
        Assert.Equal(new Point(400, 0), tc.GetContainerPosition("Item 5"));
        Assert.Equal(new Point(0, 100), tc.GetContainerPosition("Item 6"));
    }


    [WpfFact]
    public async Task BetweenItemsOnly()
    {
        await tc.SetSpacingModeAsync(SpacingMode.BetweenItemsOnly);

        Assert.Equal(500, tc.DesiredSize.Width);
        Assert.Equal(new Point(0, 0), tc.GetContainerPosition("Item 1"));
        Assert.Equal(new Point(115, 0), tc.GetContainerPosition("Item 2"));
        Assert.Equal(new Point(230, 0), tc.GetContainerPosition("Item 3"));
        Assert.Equal(new Point(345, 0), tc.GetContainerPosition("Item 4"));
        Assert.Equal(new Point(460, 0), tc.GetContainerPosition("Item 5"));
        Assert.Equal(new Point(0, 100), tc.GetContainerPosition("Item 6"));
    }

    [WpfFact(Skip = "Since 2.5+ the full viewport width is used when AllowDifferentSizedItems is true to make the extent calculation more stable")]
    public async Task BetweenItemsOnly_DifferentSizedItems_HorizontalAlignmentCenter()
    {
        var items = new ObservableCollection<TestItem>([
            new TestItem("Item 1", 85, 70),
            new TestItem("Item 2", 90, 95),
            new TestItem("Item 3", 100, 75),
            new TestItem("Item 4", 75, 60),
            new TestItem("Item 5", 95, 90),
            new TestItem("Item 6", 80, 40),
            new TestItem("Item 7", 70, 80),
            new TestItem("Item 8", 90, 65),
        ]);
        var tc = TestController.CreateListBoxWithVirtualizingWrapPanel(items: items, width: 560, height: 400);

        await tc.SetHorizontalAlignmentAsync(HorizontalAlignment.Center);
        await tc.SetAllowDifferentSizedItemsAsync(true);
        await tc.SetSpacingModeAsync(SpacingMode.BetweenItemsOnly);

        //Assert.Equal(525, vwp.DesiredSize.Width);
        Assert.Equal(new Rect(0, 0, 85, 70), tc.GetContainerBounds("Item 1"));
        Assert.Equal(new Rect(85, 0, 90, 95), tc.GetContainerBounds("Item 2"));
        Assert.Equal(new Rect(175, 0, 100, 75), tc.GetContainerBounds("Item 3"));
        Assert.Equal(new Rect(275, 0, 75, 60), tc.GetContainerBounds("Item 4"));
        Assert.Equal(new Rect(350, 0, 95, 90), tc.GetContainerBounds("Item 5"));
        Assert.Equal(new Rect(445, 0, 80, 40), tc.GetContainerBounds("Item 6"));
        Assert.Equal(new Rect(0, 95, 70, 80), tc.GetContainerBounds("Item 7"));
        Assert.Equal(new Rect(435, 95, 90, 65), tc.GetContainerBounds("Item 8"));
    }

    [WpfFact]
    public async Task StartAndEndOnly()
    {
        await tc.SetSpacingModeAsync(SpacingMode.StartAndEndOnly);

        Assert.Equal(500, tc.DesiredSize.Width);
        Assert.Equal(new Point(30, 0), tc.GetContainerPosition("Item 1"));
        Assert.Equal(new Point(130, 0), tc.GetContainerPosition("Item 2"));
        Assert.Equal(new Point(230, 0), tc.GetContainerPosition("Item 3"));
        Assert.Equal(new Point(330, 0), tc.GetContainerPosition("Item 4"));
        Assert.Equal(new Point(430, 0), tc.GetContainerPosition("Item 5"));
        Assert.Equal(new Point(30, 100), tc.GetContainerPosition("Item 6"));
    }

    [WpfFact]
    public async Task StretchItemsTrue()
    {
        await tc.SetStretchItemsAsync(true);

        Assert.Equal(500, tc.DesiredSize.Width);
        Assert.Equal(new Rect(0, 0, 112, 100), tc.GetContainerBounds("Item 1"));
        Assert.Equal(new Rect(112, 0, 112, 100), tc.GetContainerBounds("Item 2"));
        Assert.Equal(new Rect(224, 0, 112, 100), tc.GetContainerBounds("Item 3"));
        Assert.Equal(new Rect(336, 0, 112, 100), tc.GetContainerBounds("Item 4"));
        Assert.Equal(new Rect(448, 0, 112, 100), tc.GetContainerBounds("Item 5"));
        Assert.Equal(new Rect(0, 100, 112, 100), tc.GetContainerBounds("Item 6"));
    }

    [WpfFact]
    public async Task StretchItemsTrue_MaxWidth()
    {
        await tc.SetStretchItemsAsync(true);
        await tc.SetItemContainerStyleAsync(new Style()
        {
            BasedOn = tc.DefaultItemContainerStyle,
            Setters = {
                new Setter(FrameworkElement.MaxWidthProperty, 106d)
            }
        });

        Assert.Equal(500, tc.DesiredSize.Width);
        Assert.Equal(new Rect(5, 0, 106, 100), tc.GetContainerBounds("Item 1"));
        Assert.Equal(new Rect(116, 0, 106, 100), tc.GetContainerBounds("Item 2"));
        Assert.Equal(new Rect(227, 0, 106, 100), tc.GetContainerBounds("Item 3"));
        Assert.Equal(new Rect(338, 0, 106, 100), tc.GetContainerBounds("Item 4"));
        Assert.Equal(new Rect(449, 0, 106, 100), tc.GetContainerBounds("Item 5"));
        Assert.Equal(new Rect(5, 100, 106, 100), tc.GetContainerBounds("Item 6"));
    }
}

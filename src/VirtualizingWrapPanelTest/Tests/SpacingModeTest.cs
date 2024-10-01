using System.Windows;
using WpfToolkit.Controls;
using Xunit;
using Assert = Xunit.Assert;

namespace VirtualizingWrapPanelTest.Tests;

public class SpacingModeTest
{
    private readonly VirtualizingWrapPanel vwp = TestUtil.CreateVirtualizingWrapPanel(560, 400, itemCount: 6);

    [UIFact]
    public void Uniform()
    {
        // Uniform is the default value

        Assert.Equal(500, vwp.DesiredSize.Width);
        TestUtil.AssertItem(vwp, "Item 1", 10, 0);
        TestUtil.AssertItem(vwp, "Item 2", 120, 0);
        TestUtil.AssertItem(vwp, "Item 3", 230, 0);
        TestUtil.AssertItem(vwp, "Item 4", 340, 0);
        TestUtil.AssertItem(vwp, "Item 5", 450, 0);
        TestUtil.AssertItem(vwp, "Item 6", 10, 100);
    }

    [UIFact]
    public void Uniform_HorizontalAlignmentCenter()
    {
        vwp.HorizontalAlignment = HorizontalAlignment.Center;
        vwp.UpdateLayout();

        Assert.Equal(500, vwp.DesiredSize.Width);
        TestUtil.AssertPanelPosition(vwp, 30, 0);
        TestUtil.AssertItem(vwp, "Item 1", 0, 0);
        TestUtil.AssertItem(vwp, "Item 2", 100, 0);
        TestUtil.AssertItem(vwp, "Item 3", 200, 0);
        TestUtil.AssertItem(vwp, "Item 4", 300, 0);
        TestUtil.AssertItem(vwp, "Item 5", 400, 0);
        TestUtil.AssertItem(vwp, "Item 6", 0, 100);
    }

    [UIFact]
    public void Uniform_HorizontalAlignmentRight()
    {
        vwp.HorizontalAlignment = HorizontalAlignment.Right;
        vwp.UpdateLayout();

        Assert.Equal(500, vwp.DesiredSize.Width);
        TestUtil.AssertPanelPosition(vwp, 60, 0);
        TestUtil.AssertItem(vwp, "Item 1", 0, 0);
        TestUtil.AssertItem(vwp, "Item 2", 100, 0);
        TestUtil.AssertItem(vwp, "Item 3", 200, 0);
        TestUtil.AssertItem(vwp, "Item 4", 300, 0);
        TestUtil.AssertItem(vwp, "Item 5", 400, 0);
        TestUtil.AssertItem(vwp, "Item 6", 0, 100);
    }

    [UIFact]
    public void Uniform_HorizontalAlignmentLeft()
    {
        vwp.HorizontalAlignment = HorizontalAlignment.Left;
        vwp.UpdateLayout();

        Assert.Equal(500, vwp.DesiredSize.Width);
        TestUtil.AssertPanelPosition(vwp, 0, 0);
        TestUtil.AssertItem(vwp, "Item 1", 0, 0);
        TestUtil.AssertItem(vwp, "Item 2", 100, 0);
        TestUtil.AssertItem(vwp, "Item 3", 200, 0);
        TestUtil.AssertItem(vwp, "Item 4", 300, 0);
        TestUtil.AssertItem(vwp, "Item 5", 400, 0);
        TestUtil.AssertItem(vwp, "Item 6", 0, 100);
    }

    [UIFact]
    public void Uniform_DifferentSizedItems()
    {
        var items = new[]
        {
            new TestItem("Item 1", 85, 70),
            new TestItem("Item 2", 90, 95),
            new TestItem("Item 3", 100, 75),
            new TestItem("Item 4", 75, 60),
            new TestItem("Item 5", 95, 90),
            new TestItem("Item 6", 80, 40),
            new TestItem("Item 7", 70, 80),
        };
        var vwp = TestUtil.CreateVirtualizingWrapPanel(560, 400, items);

        vwp.AllowDifferentSizedItems = true;
        vwp.UpdateLayout();

        Assert.Equal(525, vwp.DesiredSize.Width);
        TestUtil.AssertItem(vwp, "Item 1", 5, 0, 85, 70);
        TestUtil.AssertItem(vwp, "Item 2", 95, 0, 90, 95);
        TestUtil.AssertItem(vwp, "Item 3", 190, 0, 100, 75);
        TestUtil.AssertItem(vwp, "Item 4", 295, 0, 75, 60);
        TestUtil.AssertItem(vwp, "Item 5", 375, 0, 95, 90);
        TestUtil.AssertItem(vwp, "Item 6", 475, 0, 80, 40);
        TestUtil.AssertItem(vwp, "Item 7", 245, 95, 70, 80);
    }

    [UIFact]
    public void None()
    {
        vwp.SpacingMode = SpacingMode.None;
        vwp.UpdateLayout();

        Assert.Equal(500, vwp.DesiredSize.Width);
        TestUtil.AssertItem(vwp, "Item 1", 0, 0);
        TestUtil.AssertItem(vwp, "Item 2", 100, 0);
        TestUtil.AssertItem(vwp, "Item 3", 200, 0);
        TestUtil.AssertItem(vwp, "Item 4", 300, 0);
        TestUtil.AssertItem(vwp, "Item 5", 400, 0);
        TestUtil.AssertItem(vwp, "Item 6", 0, 100);
    }


    [UIFact]
    public void BetweenItemsOnly()
    {
        vwp.SpacingMode = SpacingMode.BetweenItemsOnly;
        vwp.UpdateLayout();

        Assert.Equal(500, vwp.DesiredSize.Width);
        TestUtil.AssertItem(vwp, "Item 1", 0, 0);
        TestUtil.AssertItem(vwp, "Item 2", 115, 0);
        TestUtil.AssertItem(vwp, "Item 3", 230, 0);
        TestUtil.AssertItem(vwp, "Item 4", 345, 0);
        TestUtil.AssertItem(vwp, "Item 5", 460, 0);
        TestUtil.AssertItem(vwp, "Item 6", 0, 100);
    }

    [UIFact]
    public void BetweenItemsOnly_DifferentSizedItems_HorizontalAlignmentCenter()
    {
        var items = new[]
        {
            new TestItem("Item 1", 85, 70),
            new TestItem("Item 2", 90, 95),
            new TestItem("Item 3", 100, 75),
            new TestItem("Item 4", 75, 60),
            new TestItem("Item 5", 95, 90),
            new TestItem("Item 6", 80, 40),
            new TestItem("Item 7", 70, 80),
            new TestItem("Item 8", 90, 65),
        };
        var vwp = TestUtil.CreateVirtualizingWrapPanel(560, 400, items);

        vwp.HorizontalAlignment = HorizontalAlignment.Center;
        vwp.AllowDifferentSizedItems = true;
        vwp.SpacingMode = SpacingMode.BetweenItemsOnly;
        vwp.UpdateLayout();

        Assert.Equal(525, vwp.DesiredSize.Width);
        TestUtil.AssertItem(vwp, "Item 1", 0, 0, 85, 70);
        TestUtil.AssertItem(vwp, "Item 2", 85, 0, 90, 95);
        TestUtil.AssertItem(vwp, "Item 3", 175, 0, 100, 75);
        TestUtil.AssertItem(vwp, "Item 4", 275, 0, 75, 60);
        TestUtil.AssertItem(vwp, "Item 5", 350, 0, 95, 90);
        TestUtil.AssertItem(vwp, "Item 6", 445, 0, 80, 40);
        TestUtil.AssertItem(vwp, "Item 7", 0, 95, 70, 80);
        TestUtil.AssertItem(vwp, "Item 8", 435, 95, 90, 65);
    }

    [UIFact]
    public void StartAndEndOnly()
    {
        vwp.SpacingMode = SpacingMode.StartAndEndOnly;
        vwp.UpdateLayout();

        Assert.Equal(500, vwp.DesiredSize.Width);
        TestUtil.AssertItem(vwp, "Item 1", 30, 0);
        TestUtil.AssertItem(vwp, "Item 2", 130, 0);
        TestUtil.AssertItem(vwp, "Item 3", 230, 0);
        TestUtil.AssertItem(vwp, "Item 4", 330, 0);
        TestUtil.AssertItem(vwp, "Item 5", 430, 0);
        TestUtil.AssertItem(vwp, "Item 6", 30, 100);
    }

    [UIFact]
    public void StretchItemsTrue()
    {
        vwp.StretchItems = true;
        vwp.UpdateLayout();

        Assert.Equal(500, vwp.DesiredSize.Width);
        TestUtil.AssertItem(vwp, "Item 1", 0, 0, 112, 100);
        TestUtil.AssertItem(vwp, "Item 2", 112, 0, 112, 100);
        TestUtil.AssertItem(vwp, "Item 3", 224, 0, 112, 100);
        TestUtil.AssertItem(vwp, "Item 4", 336, 0, 112, 100);
        TestUtil.AssertItem(vwp, "Item 5", 448, 0, 112, 100);
        TestUtil.AssertItem(vwp, "Item 6", 0, 100, 112, 100);
    }

    [UIFact]
    public void StretchItemsTrue_MaxWidth()
    {
        vwp.StretchItems = true;
        vwp.ItemsControl.ItemContainerStyle = new Style()
        {
            Setters = { new Setter() { Property = FrameworkElement.MaxWidthProperty, Value = 106d } }
        };
        vwp.UpdateLayout();

        Assert.Equal(500, vwp.DesiredSize.Width);
        TestUtil.AssertItem(vwp, "Item 1", 5, 0, 106, 100);
        TestUtil.AssertItem(vwp, "Item 2", 116, 0, 106, 100);
        TestUtil.AssertItem(vwp, "Item 3", 227, 0, 106, 100);
        TestUtil.AssertItem(vwp, "Item 4", 338, 0, 106, 100);
        TestUtil.AssertItem(vwp, "Item 5", 449, 0, 106, 100);
        TestUtil.AssertItem(vwp, "Item 6", 5, 100, 106, 100);
    }
}

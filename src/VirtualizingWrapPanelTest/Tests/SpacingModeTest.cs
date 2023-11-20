using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WpfToolkit.Controls;
using Xunit;
using Assert = Xunit.Assert;

namespace VirtualizingWrapPanelTest.Tests;

public class SpacingModeTest
{
    private readonly VirtualizingWrapPanel vwp = TestUtil.CreateVirtualizingWrapPanel(560, 400, itemCount: 6);

    [UIFact]
    public void SpacingMode_None()
    {
        vwp.SpacingMode = SpacingMode.None;
        vwp.UpdateLayout();

        Assert.Equal(500, vwp.DesiredSize.Width);
        TestUtil.AssertItemPosition(vwp, "Item 1", 0, 0);
        TestUtil.AssertItemPosition(vwp, "Item 2", 100, 0);
        TestUtil.AssertItemPosition(vwp, "Item 3", 200, 0);
        TestUtil.AssertItemPosition(vwp, "Item 4", 300, 0);
        TestUtil.AssertItemPosition(vwp, "Item 5", 400, 0);
        TestUtil.AssertItemPosition(vwp, "Item 6", 0, 100);
    }

    [UIFact]
    public void SpacingMode_Uniform()
    {
        // Uniform is the default value

        Assert.Equal(500, vwp.DesiredSize.Width);
        TestUtil.AssertItemPosition(vwp, "Item 1", 10, 0);
        TestUtil.AssertItemPosition(vwp, "Item 2", 120, 0);
        TestUtil.AssertItemPosition(vwp, "Item 3", 230, 0);
        TestUtil.AssertItemPosition(vwp, "Item 4", 340, 0);
        TestUtil.AssertItemPosition(vwp, "Item 5", 450, 0);
        TestUtil.AssertItemPosition(vwp, "Item 6", 10, 100);
    }

    [UIFact]
    public void SpacingMode_BetweenItemsOnly()
    {
        vwp.SpacingMode = SpacingMode.BetweenItemsOnly;
        vwp.UpdateLayout();

        Assert.Equal(500, vwp.DesiredSize.Width);
        TestUtil.AssertItemPosition(vwp, "Item 1", 0, 0);
        TestUtil.AssertItemPosition(vwp, "Item 2", 115, 0);
        TestUtil.AssertItemPosition(vwp, "Item 3", 230, 0);
        TestUtil.AssertItemPosition(vwp, "Item 4", 345, 0);
        TestUtil.AssertItemPosition(vwp, "Item 5", 460, 0);
        TestUtil.AssertItemPosition(vwp, "Item 6", 0, 100);
    }

    [UIFact]
    public void SpacingMode_StartAndEndOnly()
    {
        vwp.SpacingMode = SpacingMode.StartAndEndOnly;
        vwp.UpdateLayout();

        Assert.Equal(500, vwp.DesiredSize.Width);
        TestUtil.AssertItemPosition(vwp, "Item 1", 30, 0);
        TestUtil.AssertItemPosition(vwp, "Item 2", 130, 0);
        TestUtil.AssertItemPosition(vwp, "Item 3", 230, 0);
        TestUtil.AssertItemPosition(vwp, "Item 4", 330, 0);
        TestUtil.AssertItemPosition(vwp, "Item 5", 430, 0);
        TestUtil.AssertItemPosition(vwp, "Item 6", 30, 100);
    }

    [UIFact]
    public void SpacingMode_StretchItemsTrue()
    {
        vwp.StretchItems = true;
        vwp.UpdateLayout();

        Assert.Equal(500, vwp.DesiredSize.Width);
        TestUtil.AssertItemPosition(vwp, "Item 1", 0, 0);
        TestUtil.AssertItemPosition(vwp, "Item 2", 112, 0);
        TestUtil.AssertItemPosition(vwp, "Item 3", 224, 0);
        TestUtil.AssertItemPosition(vwp, "Item 4", 336, 0);
        TestUtil.AssertItemPosition(vwp, "Item 5", 448, 0);
        TestUtil.AssertItemPosition(vwp, "Item 6", 0, 100);
    }

    [UIFact]
    public void SpacingMode_Uniform_HorizontalAlignmentCenter()
    {
        vwp.HorizontalAlignment = HorizontalAlignment.Center;
        vwp.UpdateLayout();

        Assert.Equal(500, vwp.DesiredSize.Width);
        TestUtil.AssertPanelPosition(vwp, 30, 0);
        TestUtil.AssertItemPosition(vwp, "Item 1", 0, 0);
        TestUtil.AssertItemPosition(vwp, "Item 2", 100, 0);
        TestUtil.AssertItemPosition(vwp, "Item 3", 200, 0);
        TestUtil.AssertItemPosition(vwp, "Item 4", 300, 0);
        TestUtil.AssertItemPosition(vwp, "Item 5", 400, 0);
        TestUtil.AssertItemPosition(vwp, "Item 6", 0, 100);
    }

    [UIFact]
    public void SpacingMode_Uniform_HorizontalAlignmentRight()
    {
        vwp.HorizontalAlignment = HorizontalAlignment.Right;
        vwp.UpdateLayout();

        Assert.Equal(500, vwp.DesiredSize.Width);
        TestUtil.AssertPanelPosition(vwp, 60, 0);
        TestUtil.AssertItemPosition(vwp, "Item 1", 0, 0);
        TestUtil.AssertItemPosition(vwp, "Item 2", 100, 0);
        TestUtil.AssertItemPosition(vwp, "Item 3", 200, 0);
        TestUtil.AssertItemPosition(vwp, "Item 4", 300, 0);
        TestUtil.AssertItemPosition(vwp, "Item 5", 400, 0);
        TestUtil.AssertItemPosition(vwp, "Item 6", 0, 100);
    }

    [UIFact]
    public void SpacingMode_Uniform_HorizontalAlignmentLeft()
    {
        vwp.HorizontalAlignment = HorizontalAlignment.Left;
        vwp.UpdateLayout();

        Assert.Equal(500, vwp.DesiredSize.Width);
        TestUtil.AssertPanelPosition(vwp, 0, 0);
        TestUtil.AssertItemPosition(vwp, "Item 1", 0, 0);
        TestUtil.AssertItemPosition(vwp, "Item 2", 100, 0);
        TestUtil.AssertItemPosition(vwp, "Item 3", 200, 0);
        TestUtil.AssertItemPosition(vwp, "Item 4", 300, 0);
        TestUtil.AssertItemPosition(vwp, "Item 5", 400, 0);
        TestUtil.AssertItemPosition(vwp, "Item 6", 0, 100);
    }

    [UIFact]
    public void SpacingMode_Uniform_DifferentSizedItems()
    {
        var items = new[] 
        {
            new Item("Item 1", 85, 70),
            new Item("Item 2", 90, 95),
            new Item("Item 3", 100, 75),
            new Item("Item 4", 75, 60),
            new Item("Item 5", 95, 90),
            new Item("Item 6", 80, 40),
            new Item("Item 7", 70, 80),
        };
        var vwp = TestUtil.CreateVirtualizingWrapPanel(560, 400, items);

        vwp.AllowDifferentSizedItems = true;
        vwp.UpdateLayout();

        Assert.Equal(525, vwp.DesiredSize.Width);
        TestUtil.AssertItemPosition(vwp, "Item 1", 5, 0);
        TestUtil.AssertItemPosition(vwp, "Item 2", 95, 0);
        TestUtil.AssertItemPosition(vwp, "Item 3", 190, 0);
        TestUtil.AssertItemPosition(vwp, "Item 4", 295, 0);
        TestUtil.AssertItemPosition(vwp, "Item 5", 375, 0);
        TestUtil.AssertItemPosition(vwp, "Item 6", 475, 0);
        TestUtil.AssertItemPosition(vwp, "Item 7", 245, 95);
    }

    [UIFact]
    public void SpacingMode_BetweenItemsOnly_DifferentSizedItems_HorizontalAlignmentCenter()
    {
        var items = new[] 
        {
            new Item("Item 1", 85, 70),
            new Item("Item 2", 90, 95),
            new Item("Item 3", 100, 75),
            new Item("Item 4", 75, 60),
            new Item("Item 5", 95, 90),
            new Item("Item 6", 80, 40),
            new Item("Item 7", 70, 80),
            new Item("Item 8", 90, 65),
        };
        var vwp = TestUtil.CreateVirtualizingWrapPanel(560, 400, items);

        vwp.HorizontalAlignment = HorizontalAlignment.Center;
        vwp.AllowDifferentSizedItems = true;
        vwp.SpacingMode = SpacingMode.BetweenItemsOnly;
        vwp.UpdateLayout();

        Assert.Equal(525, vwp.DesiredSize.Width);
        TestUtil.AssertItemPosition(vwp, "Item 1", 0, 0);
        TestUtil.AssertItemPosition(vwp, "Item 2", 85, 0);
        TestUtil.AssertItemPosition(vwp, "Item 3", 175, 0);
        TestUtil.AssertItemPosition(vwp, "Item 4", 275, 0);
        TestUtil.AssertItemPosition(vwp, "Item 5", 350, 0);
        TestUtil.AssertItemPosition(vwp, "Item 6", 445, 0);
        TestUtil.AssertItemPosition(vwp, "Item 7", 0, 95);
        TestUtil.AssertItemPosition(vwp, "Item 8", 435, 95);
    }
}

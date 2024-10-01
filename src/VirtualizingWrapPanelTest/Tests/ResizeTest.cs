using WpfToolkit.Controls;
using Xunit;
using Assert = Xunit.Assert;

namespace VirtualizingWrapPanelTest.Tests;

public class ResizeTest
{
    private VirtualizingWrapPanel vwp = TestUtil.CreateVirtualizingWrapPanel(500, 400);

    [UIFact]
    public void IncreaseWidth()
    {
        vwp.ItemsControl.Width = 600;
        vwp.UpdateLayout();

        Assert.Equal(600, vwp.ExtentWidth);
        Assert.Equal(16_700, vwp.ExtentHeight);

        Assert.Equal(48, vwp.Children.Count);
        TestUtil.AssertItem(vwp, "Item 6", 500, 0);
    }

    [UIFact]
    public void IncreaseHeight()
    {
        vwp.ItemsControl.Height = 500;
        vwp.UpdateLayout();

        Assert.Equal(500, vwp.ExtentWidth);
        Assert.Equal(20_000, vwp.ExtentHeight);

        Assert.Equal(50, vwp.Children.Count);
        TestUtil.AssertItem(vwp, "Item 6", 0, 100);
    }

    [UIFact]
    public void IncreaseWidthAndHeight()
    {
        vwp.ItemsControl.Width = 600;
        vwp.ItemsControl.Height = 500;
        vwp.UpdateLayout();

        Assert.Equal(600, vwp.ExtentWidth);
        Assert.Equal(16_700, vwp.ExtentHeight);

        Assert.Equal(60, vwp.Children.Count);
        TestUtil.AssertItem(vwp, "Item 6", 500, 0);
    }

    [UIFact]
    public void DecreaseWidth()
    {
        vwp.ItemsControl.Width = 400;
        vwp.UpdateLayout();

        Assert.Equal(400, vwp.ExtentWidth);
        Assert.Equal(25_000, vwp.ExtentHeight);

        Assert.Equal(32, vwp.Children.Count);
        TestUtil.AssertItem(vwp, "Item 6", 100, 100);
    }

    [UIFact]
    public void DecreaseHeight()
    {
        vwp.ItemsControl.Height = 300;
        vwp.UpdateLayout();

        Assert.Equal(500, vwp.ExtentWidth);
        Assert.Equal(20_000, vwp.ExtentHeight);

        Assert.Equal(30, vwp.Children.Count);
        TestUtil.AssertItem(vwp, "Item 6", 0, 100);
    }

    [UIFact]
    public void DecreaseWidthAndHeight()
    {
        vwp.ItemsControl.Width = 400;
        vwp.ItemsControl.Height = 300;
        vwp.UpdateLayout();

        Assert.Equal(400, vwp.ExtentWidth);
        Assert.Equal(25_000, vwp.ExtentHeight);

        Assert.Equal(24, vwp.Children.Count);
        TestUtil.AssertItem(vwp, "Item 6", 100, 100);
    }

}

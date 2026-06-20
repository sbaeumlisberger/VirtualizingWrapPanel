using System.Windows.Controls;
using WpfToolkit.Controls;
using Xunit;
using Assert = Xunit.Assert;

namespace VirtualizingWrapPanelTest.Tests;

public class BasisTest
{
    private VirtualizingWrapPanel vwp = TestUtil.CreateVirtualizingWrapPanel(500, 400);

    [UIFact]
    public void BringIndexIntoView_AfterViewport_VerticalOrientation()
    {
        vwp.Orientation = Orientation.Vertical;
        vwp.UpdateLayout();

        vwp.BringIndexIntoViewPublic(500);
        vwp.UpdateLayout();

        Assert.Equal(12_100, vwp.HorizontalOffset);
        Assert.Equal(25_000, vwp.ExtentWidth);
        Assert.Equal(400, vwp.ExtentHeight);
        Assert.Equal(60, vwp.Children.Count);
        TestUtil.AssertItem(vwp, "Item 501", 400, 0);
    }

    [UIFact]
    public void BringIndexIntoView_BeforeViewport_VerticalOrientation()
    {
        vwp.Orientation = Orientation.Vertical;
        vwp.UpdateLayout();

        vwp.SetHorizontalOffset(20_000);
        vwp.UpdateLayout();

        vwp.BringIndexIntoViewPublic(500);
        vwp.UpdateLayout();

        Assert.Equal(12_500, vwp.HorizontalOffset);
        Assert.Equal(25_000, vwp.ExtentWidth);
        Assert.Equal(400, vwp.ExtentHeight);
        Assert.Equal(60, vwp.Children.Count);
        TestUtil.AssertItem(vwp, "Item 501", 0, 0);
    }
}

using System.Windows.Controls;
using WpfToolkit.Controls;
using Xunit;
using Assert = Xunit.Assert;

namespace VirtualizingWrapPanelTest.Tests;

public class CacheTest
{
    private VirtualizingWrapPanel vwp = TestUtil.CreateVirtualizingWrapPanel(500, 400);

    [UITheory]
    [InlineData(0, 20)]
    [InlineData(500, 20)]
    [InlineData(550, 25)] // row partial visible
    public void NoCache(double offset, int expectedChildCount)
    {
        VirtualizingPanel.SetCacheLength(vwp.ItemsControl, new VirtualizationCacheLength(0));
        vwp.SetVerticalOffset(offset);

        vwp.UpdateLayout();

        Assert.Equal(expectedChildCount, vwp.Children.Count);
    }

    [UITheory]
    [InlineData(0, 60)]
    [InlineData(1000, 100)]
    [InlineData(1050, 105)] // row partial visible
    public void CacheUnitPage(double offset, int expectedChildCount)
    {
        VirtualizingPanel.SetCacheLength(vwp.ItemsControl, new VirtualizationCacheLength(2));
        vwp.SetVerticalOffset(offset);

        vwp.UpdateLayout();

        Assert.Equal(expectedChildCount, vwp.Children.Count);
    }

    [UITheory]
    [InlineData(0, 22)]
    [InlineData(500, 24)]
    [InlineData(550, 29)] // row partial visible
    public void CacheUnitItem(double offset, int expectedChildCount)
    {
        VirtualizingPanel.SetCacheLength(vwp.ItemsControl, new VirtualizationCacheLength(2));
        VirtualizingPanel.SetCacheLengthUnit(vwp.ItemsControl, VirtualizationCacheLengthUnit.Item);
        vwp.SetVerticalOffset(offset);

        vwp.UpdateLayout();

        Assert.Equal(expectedChildCount, vwp.Children.Count);
    }

    [UIFact]
    public void CacheUnitItem_DifferentLengths()
    {
        VirtualizingPanel.SetCacheLength(vwp.ItemsControl, new VirtualizationCacheLength(1, 2));
        VirtualizingPanel.SetCacheLengthUnit(vwp.ItemsControl, VirtualizationCacheLengthUnit.Item);
        vwp.SetVerticalOffset(500);

        vwp.UpdateLayout();

        Assert.Equal(23, vwp.Children.Count);
    }

    [UITheory]
    [InlineData(0, 150, 30)]
    [InlineData(50, 150, 30)]
    [InlineData(51, 150, 35)]
    [InlineData(50, 200, 35)]
    [InlineData(500, 150, 40)]
    [InlineData(500, 200, 40)]
    public void CacheUnitPixel(double offset, int cacheSize, int expectedChildCount)
    {
        VirtualizingPanel.SetCacheLength(vwp.ItemsControl, new VirtualizationCacheLength(cacheSize));
        VirtualizingPanel.SetCacheLengthUnit(vwp.ItemsControl, VirtualizationCacheLengthUnit.Pixel);
        vwp.SetVerticalOffset(offset);

        vwp.UpdateLayout();

        Assert.Equal(expectedChildCount, vwp.Children.Count);
    }
}

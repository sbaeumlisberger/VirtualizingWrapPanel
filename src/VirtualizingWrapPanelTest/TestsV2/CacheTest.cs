using System.Windows.Controls;
using Xunit;
using Assert = Xunit.Assert;

namespace VirtualizingWrapPanelTest.TestsV2;

public class CacheTest
{
    private readonly TestController vwp = TestController.CreateListBoxWithVirtualizingWrapPanel();

    [WpfTheory]
    [InlineData(0, 20)]
    [InlineData(500, 20)]
    [InlineData(550, 25)] // row partial visible
    public async Task NoCache(double offset, int expectedChildCount)
    {
        await vwp.SetCacheLengthAsync(new VirtualizationCacheLength(0));
        await vwp.SetVerticalOffsetAsync(offset);

        Assert.Equal(expectedChildCount, vwp.Children.Count);
    }

    [WpfTheory]
    [InlineData(0, 60)]
    [InlineData(100, 65)]
    [InlineData(1000, 100)]
    [InlineData(1050, 105)] // row partial visible
    public async Task CacheUnitPage(double offset, int expectedChildCount)
    {
        await vwp.SetCacheLengthUnitAsync(VirtualizationCacheLengthUnit.Page);
        await vwp.SetCacheLengthAsync(new VirtualizationCacheLength(2));
        await vwp.SetVerticalOffsetAsync(offset);

        Assert.Equal(expectedChildCount, vwp.Children.Count);
    }

    [WpfTheory]
    [InlineData(0, 30)]
    [InlineData(100, 35)]
    [InlineData(500, 40)]
    [InlineData(550, 45)] // row partial visible
    public async Task CacheUnitItem(double offset, int expectedChildCount)
    {
        // VirtualizationCacheLengthUnit.Item is the default
        await vwp.SetCacheLengthAsync(new VirtualizationCacheLength(10));
        await vwp.SetVerticalOffsetAsync(offset);

        Assert.Equal(expectedChildCount, vwp.Children.Count);
    }

    [WpfFact]
    public async Task CacheUnitItem_DifferentLengths()
    {
        // VirtualizationCacheLengthUnit.Item is the default
        await vwp.SetCacheLengthAsync(new VirtualizationCacheLength(1, 2));
        await vwp.SetVerticalOffsetAsync(500);

        Assert.Equal(23, vwp.Children.Count);
    }

    [WpfTheory]
    [InlineData(0, 150, 30)]
    [InlineData(50, 150, 30)]
    [InlineData(51, 150, 35)]
    [InlineData(50, 200, 35)]
    [InlineData(500, 150, 40)]
    [InlineData(500, 200, 40)]
    public async Task CacheUnitPixel(double offset, int cacheSize, int expectedChildCount)
    {
        await vwp.SetCacheLengthUnitAsync(VirtualizationCacheLengthUnit.Pixel);
        await vwp.SetCacheLengthAsync(new VirtualizationCacheLength(cacheSize));
        await vwp.SetVerticalOffsetAsync(offset);

        Assert.Equal(expectedChildCount, vwp.Children.Count);
    }
}

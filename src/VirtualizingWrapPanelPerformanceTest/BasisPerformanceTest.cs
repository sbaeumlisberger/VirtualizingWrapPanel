using System.Diagnostics;
using System.Windows.Controls;
using WpfToolkit.Controls;
using Xunit.Abstractions;

namespace VirtualizingWrapPanelTest.PerformanceTests;

public class BasisPerformanceTest(ITestOutputHelper testOutputHelper)
{
    // TODO find a way to get more reliable results

    [WpfTheory]
    [InlineData(VirtualizationCacheLengthUnit.Item, 10, false, 15)]
    [InlineData(VirtualizationCacheLengthUnit.Item, 10, true, 100)]
    [InlineData(VirtualizationCacheLengthUnit.Page, 1, false, 15)]
    [InlineData(VirtualizationCacheLengthUnit.Page, 1, true, 90)]
    [InlineData(VirtualizationCacheLengthUnit.Pixel, 200, false, 15)]
    [InlineData(VirtualizationCacheLengthUnit.Pixel, 200, true, 70)]
    public void SetVerticalOffset(VirtualizationCacheLengthUnit cacheUnit, int cacheLength, bool allowDifferentSizedItems, int maxAllowedAvgMilliseconds)
    {
        VirtualizingWrapPanel vwp = TestUtil.CreateVirtualizingWrapPanel(500, 500, 1_000_000);
        VirtualizingPanel.SetVirtualizationMode(vwp.ItemsControl, VirtualizationMode.Recycling);
        VirtualizingPanel.SetCacheLengthUnit(vwp.ItemsControl, cacheUnit);
        VirtualizingPanel.SetCacheLength(vwp.ItemsControl, new VirtualizationCacheLength(cacheLength));
        vwp.AllowDifferentSizedItems = allowDifferentSizedItems;
        vwp.UpdateLayout();

        int maxVerticaOffset = CalculateMaxVerticalOffset(vwp);
        
        int iterations = 50;

        Stopwatch sw = Stopwatch.StartNew();  

        for (int i = 0; i < iterations; i++)
        {
            vwp.SetVerticalOffset(Random.Shared.Next(maxVerticaOffset));
            vwp.InvalidateMeasure();
            vwp.UpdateLayout();
        }

        sw.Stop();

        double avgTime = Math.Round(sw.Elapsed.TotalMilliseconds / iterations);

        testOutputHelper.WriteLine($"Average time was {avgTime}ms");

        Assert.True(avgTime <= maxAllowedAvgMilliseconds, $"Average time was {avgTime}ms, but should be less than or equal to {maxAllowedAvgMilliseconds}ms.");
    }


    [WpfTheory]
    [InlineData(false, 10)]
    [InlineData(true, 300)]
    public void RemoveItemFromItemsSource(bool allowDifferentSizedItems, int maxAllowedAvgMilliseconds)
    {
        var items = TestUtil.GenerateItems(1_000_000);
        VirtualizingWrapPanel vwp = TestUtil.CreateVirtualizingWrapPanel(500, 500, items);
        vwp.AllowDifferentSizedItems = allowDifferentSizedItems;
        vwp.UpdateLayout();

        int maxVerticaOffset = CalculateMaxVerticalOffset(vwp);
        vwp.SetVerticalOffset(maxVerticaOffset);
        vwp.UpdateLayout();

        int iterations = 10;

        Stopwatch sw = Stopwatch.StartNew();

        for (int i = 0; i < iterations; i++)
        {
            items.RemoveAt(Random.Shared.Next(items.Count));
            vwp.UpdateLayout();
        }

        sw.Stop();

        double avgTime = Math.Round(sw.Elapsed.TotalMilliseconds / iterations);

        testOutputHelper.WriteLine($"Average time was {avgTime}ms");

        Assert.True(avgTime <= maxAllowedAvgMilliseconds, $"Average time was {avgTime}ms, but should be less than or equal to {maxAllowedAvgMilliseconds}ms.");
    }

    private static int CalculateMaxVerticalOffset(VirtualizingWrapPanel vwp)
    {
        int itemsPerRow = (int)Math.Floor(vwp.ItemsControl.ActualWidth / TestUtil.DefaultItemWidth);
        int rowCount = (int)Math.Ceiling((double)vwp.ItemsControl.Items.Count / itemsPerRow);
        int maxVerticaOffset = rowCount * TestUtil.DefaultItemHeight - TestUtil.DefaultItemHeight;
        return maxVerticaOffset;
    }
}


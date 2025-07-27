using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using WpfToolkit.Controls;
using Xunit;

namespace VirtualizingWrapPanelTest.Tests;

public class RegressionTest
{

    [WpfFact]
    public void RemoveVirtualizingWrapPanelFromItemsControlAndRemoveItem()
    {
        var items = TestUtil.GenerateItems(1000);
        var collectionView = CollectionViewSource.GetDefaultView(items);
        var itemsControl = TestUtil.CreateListBox(500, 400, collectionView);
        var vwp = TestUtil.GetVisualChild<VirtualizingWrapPanel>(itemsControl)!;
        vwp.UpdateLayout();
        TestUtil.AssertItemRealized(vwp, "Item 1");

        itemsControl.ItemsPanel = null;
        items.RemoveAt(0);
        vwp.UpdateLayout();

        // If no exception is thrown, the test passes.
    }

    // #87 DivideByZeroException when item width greater than viewport width and cache unit is Item
    [WpfTheory]
    [InlineData(VirtualizationCacheLengthUnit.Item, 10)]
    [InlineData(VirtualizationCacheLengthUnit.Page, 1)]
    [InlineData(VirtualizationCacheLengthUnit.Pixel, 100)]
    public void ItemWidthGreaterThanViewportWidth(VirtualizationCacheLengthUnit cacheLengthUnit, int cacheLength)
    {
        var vwp = TestUtil.CreateVirtualizingWrapPanel(500, 400);
        VirtualizingPanel.SetCacheLengthUnit(vwp.ItemsControl, cacheLengthUnit);
        VirtualizingPanel.SetCacheLength(vwp.ItemsControl, new VirtualizationCacheLength(cacheLength));
        vwp.SetVerticalOffset(1000);
        vwp.UpdateLayout();

        vwp.ItemSize = new Size(vwp.ItemsControl.Width + 100, 100);
        vwp.UpdateLayout();
    }

}

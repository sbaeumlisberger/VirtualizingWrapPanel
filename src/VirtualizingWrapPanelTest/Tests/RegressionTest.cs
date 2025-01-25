using System.Windows.Data;
using WpfToolkit.Controls;
using Xunit;

namespace VirtualizingWrapPanelTest.Tests;

public class RegressionTest
{

    [UIFact]
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


}

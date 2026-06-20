using System.Windows.Controls;
using Xunit;

namespace VirtualizingWrapPanelTest.TestsV2;

public class CollectionChangedTest
{
    [WpfTheory]
    [InlineData(VirtualizationMode.Standard, false)]
    [InlineData(VirtualizationMode.Standard, true)]
    [InlineData(VirtualizationMode.Recycling, false)]
    [InlineData(VirtualizationMode.Recycling, true)]
    public async Task InsertItemAtStart(VirtualizationMode virtualizationMode, bool allowDifferentSizedItems)
    {
        var items = allowDifferentSizedItems ? TestController.GenerateDifferentSizedItems() : TestController.GenerateItems();
        var tc = TestController.CreateListBoxWithVirtualizingWrapPanel(items);
        await tc.SetVirtualizationModeAsync(virtualizationMode);
        await tc.SetAllowDifferentSizedItemsAsync(allowDifferentSizedItems);

        var newItem = new TestItem("NewItem", TestController.DefaulTestItemWidth, TestController.DefaulTestItemHeight);
        items.Insert(0, newItem);
        await tc.UpdateLayoutAsync();

        Assert.NotNull(tc.GetContainerForItem(newItem));

        if (allowDifferentSizedItems)
        {
            Assert.Equal(items.Count, tc.ItemSizesCache.Count);
        }
    }

    [WpfTheory]
    [InlineData(VirtualizationMode.Standard, false)]
    [InlineData(VirtualizationMode.Standard, true)]
    [InlineData(VirtualizationMode.Recycling, false)]
    [InlineData(VirtualizationMode.Recycling, true)]
    public async Task RemoveItem(VirtualizationMode virtualizationMode, bool allowDifferentSizedItems)
    {
        var items = allowDifferentSizedItems ? TestController.GenerateDifferentSizedItems() : TestController.GenerateItems();
        var tc = TestController.CreateListBoxWithVirtualizingWrapPanel(items);
        await tc.SetVirtualizationModeAsync(virtualizationMode);
        await tc.SetAllowDifferentSizedItemsAsync(allowDifferentSizedItems);

        var item = items[10];
        items.Remove(item);
        await tc.UpdateLayoutAsync();

        Assert.Null(tc.GetContainerForItem(item));

        if (allowDifferentSizedItems)
        {
            Assert.Equal(items.Count, tc.ItemSizesCache.Count);
        }
    }

    [WpfTheory]
    [InlineData(VirtualizationMode.Standard, false, false)]
    [InlineData(VirtualizationMode.Standard, false, true)]
    [InlineData(VirtualizationMode.Standard, true, false)]
    [InlineData(VirtualizationMode.Standard, true, true)]
    [InlineData(VirtualizationMode.Recycling, false, false)]
    [InlineData(VirtualizationMode.Recycling, false, true)]
    [InlineData(VirtualizationMode.Recycling, true, false)]
    [InlineData(VirtualizationMode.Recycling, true, true)]
    public async Task RemoveLastItem(VirtualizationMode virtualizationMode, bool allowDifferentSizedItems, bool scrollToEnd)
    {
        var items = allowDifferentSizedItems ? TestController.GenerateDifferentSizedItems() : TestController.GenerateItems();
        var tc = TestController.CreateListBoxWithVirtualizingWrapPanel(items);
        await tc.SetVirtualizationModeAsync(virtualizationMode);
        await tc.SetAllowDifferentSizedItemsAsync(allowDifferentSizedItems);
        if (scrollToEnd) { await tc.ScrollToEndAsync(); }

        var last = items.Last();
        items.Remove(last);
        await tc.UpdateLayoutAsync();

        Assert.Null(tc.GetContainerForItem(last));

        if (allowDifferentSizedItems)
        {
            Assert.Equal(items.Count, tc.ItemSizesCache.Count);
        }
    }

    [WpfTheory]
    [InlineData(VirtualizationMode.Standard)]
    [InlineData(VirtualizationMode.Recycling)]
    public async Task RemoveFocusedItem(VirtualizationMode virtualizationMode)
    {
        var items = TestController.GenerateItems(10);
        var tc = TestController.CreateListBoxWithVirtualizingWrapPanel(items);
        await tc.SetVirtualizationModeAsync(virtualizationMode);
        var item = items.Last();
        await tc.FocusItemAsync(item);

        items.Remove(item);
        await tc.UpdateLayoutAsync();

        Assert.Null(tc.GetContainerForItem(item));
        Assert.Equal(items.Count, tc.Children.Count);
    }

    [WpfTheory]
    [InlineData(VirtualizationMode.Standard, false)]
    [InlineData(VirtualizationMode.Standard, true)]
    [InlineData(VirtualizationMode.Recycling, false)]
    [InlineData(VirtualizationMode.Recycling, true)]
    public async Task MoveItemIntoViewport(VirtualizationMode virtualizationMode, bool allowDifferentSizedItems)
    {
        var items = allowDifferentSizedItems ? TestController.GenerateDifferentSizedItems() : TestController.GenerateItems();
        var tc = TestController.CreateListBoxWithVirtualizingWrapPanel(items);
        await tc.SetVirtualizationModeAsync(virtualizationMode);
        await tc.SetAllowDifferentSizedItemsAsync(allowDifferentSizedItems);

        var item = items[9000];
        items.Move(9000, 0);
        await tc.UpdateLayoutAsync();

        Assert.NotNull(tc.GetContainerForItem(item));
    }

    [WpfTheory]
    [InlineData(VirtualizationMode.Standard, false)]
    [InlineData(VirtualizationMode.Standard, true)]
    [InlineData(VirtualizationMode.Recycling, false)]
    [InlineData(VirtualizationMode.Recycling, true)]
    public async Task MoveItemOutOfViewport(VirtualizationMode virtualizationMode, bool allowDifferentSizedItems)
    {
        var items = allowDifferentSizedItems ? TestController.GenerateDifferentSizedItems() : TestController.GenerateItems();
        var tc = TestController.CreateListBoxWithVirtualizingWrapPanel(items);
        await tc.SetVirtualizationModeAsync(virtualizationMode);
        await tc.SetAllowDifferentSizedItemsAsync(allowDifferentSizedItems);

        var item = items[0];
        items.Move(0, 9000);
        await tc.UpdateLayoutAsync();

        Assert.Null(tc.GetContainerForItem(item));
    }

    [WpfTheory]
    [InlineData(VirtualizationMode.Standard, false, false)]
    [InlineData(VirtualizationMode.Standard, false, true)]
    [InlineData(VirtualizationMode.Standard, true, false)]
    [InlineData(VirtualizationMode.Standard, true, true)]
    [InlineData(VirtualizationMode.Recycling, false, false)]
    [InlineData(VirtualizationMode.Recycling, false, true)]
    [InlineData(VirtualizationMode.Recycling, true, false)]
    [InlineData(VirtualizationMode.Recycling, true, true)]
    public async Task MoveLastItem(VirtualizationMode virtualizationMode, bool allowDifferentSizedItems, bool scrollToEnd)
    {
        var items = allowDifferentSizedItems ? TestController.GenerateDifferentSizedItems() : TestController.GenerateItems();
        var tc = TestController.CreateListBoxWithVirtualizingWrapPanel(items);
        await tc.SetVirtualizationModeAsync(virtualizationMode);
        await tc.SetAllowDifferentSizedItemsAsync(allowDifferentSizedItems);
        if (scrollToEnd) { await tc.ScrollToEndAsync(); }

        var item = items.Last();
        items.Move(items.Count - 1, 0);
        await tc.UpdateLayoutAsync();

        if (scrollToEnd)
        {
            Assert.Null(tc.GetContainerForItem(item));
        }
        else
        {
            Assert.NotNull(tc.GetContainerForItem(item));
            Assert.Equal(0, tc.GetContainerBounds(item).Top);
        }
    }

    [WpfTheory]
    [InlineData(VirtualizationMode.Standard, false)]
    [InlineData(VirtualizationMode.Standard, true)]
    [InlineData(VirtualizationMode.Recycling, false)]
    [InlineData(VirtualizationMode.Recycling, true)]
    public async Task ClearItems(VirtualizationMode virtualizationMode, bool allowDifferentSizedItems)
    {
        var items = allowDifferentSizedItems ? TestController.GenerateDifferentSizedItems() : TestController.GenerateItems();
        var tc = TestController.CreateListBoxWithVirtualizingWrapPanel(items);
        await tc.SetVirtualizationModeAsync(virtualizationMode);
        await tc.SetAllowDifferentSizedItemsAsync(allowDifferentSizedItems);

        items.Clear();
        await tc.UpdateLayoutAsync();

        Assert.Empty(tc.Children);
        Assert.Equal(0, tc.ExtentHeight);
    }

    // TODO: test items changes: add, replace, remove mutiple items

    // TODO: scroll offset adjust when scrolled down and filter applied
}

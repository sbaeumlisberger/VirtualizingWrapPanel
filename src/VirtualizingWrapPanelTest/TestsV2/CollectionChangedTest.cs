using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using Xunit;

namespace VirtualizingWrapPanelTest.TestsV2;

public class CollectionChangedTest
{
    [WpfTheory]
    [InlineData(VirtualizationMode.Standard)]
    [InlineData(VirtualizationMode.Recycling)]
    public async Task RemoveFocusedItem(VirtualizationMode virtualizationMode)
    {
        var items = TestController.GenerateItems(10);
        var tc = TestController.CreateListBoxWithVirtualizingWrapPanel(items);
        await tc.SetVirtualizationModeAsync(virtualizationMode);
        await tc.FocusItemAsync(items.Last());

        items.Remove(items.Last());
        await tc.UpdateLayoutAsync();

        Assert.Equal(items.Count, tc.Children.Count);
    }

    // TODO: scroll offset adjust when scrolled down and filter applied
}

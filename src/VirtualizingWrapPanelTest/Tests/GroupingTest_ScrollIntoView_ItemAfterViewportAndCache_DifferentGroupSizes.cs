using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Xunit;

namespace VirtualizingWrapPanelTest.Tests;

public class GroupingTest_ScrollIntoView_ItemAfterViewportAndCache_DifferentGroupSizes
{
    private const int ViewportWidth = 500;

    private const int ViewportHeight = 400;

    private const int HeaderHeight = 20;

    private const int Margin = 5;

    private readonly ListBox itemsControl;

    private readonly VirtualizingStackPanel vsp;

    public GroupingTest_ScrollIntoView_ItemAfterViewportAndCache_DifferentGroupSizes()
    {
        itemsControl = Setup(TestUtil.GenerateItemsWithRandomGroupSizes(1000));
        vsp = TestUtil.GetVisualChild<VirtualizingStackPanel>(itemsControl)!;
    }

    private static ListBox Setup(ObservableCollection<TestItem> items)
    {
        var collectionView = CollectionViewSource.GetDefaultView(items);
        collectionView.GroupDescriptions.Add(new PropertyGroupDescription(nameof(TestItem.Group)));
        var itemsControl = TestUtil.CreateListBox(ViewportWidth + Margin + 2 /*Padding*/, ViewportHeight + 2 /*Padding*/, collectionView);
        itemsControl.GroupStyle.Add(new GroupStyle() { HeaderTemplate = TestUtil.CreateDefaultGroupHeaderTemplate(HeaderHeight) });
        VirtualizingPanel.SetIsVirtualizingWhenGrouping(itemsControl, true);
        return itemsControl;
    }

    [UITheory]
    [InlineData(VirtualizationMode.Standard)]
    [InlineData(VirtualizationMode.Recycling)]
    public void ScrollIntoView_ItemAfterViewportAndCache_DifferentGroupSizes(VirtualizationMode virtualizationMode)
    {
        VirtualizingPanel.SetVirtualizationMode(itemsControl, virtualizationMode);
        itemsControl.UpdateLayout();

        itemsControl.ScrollIntoView(itemsControl.Items[549]); // Item 550
        itemsControl.UpdateLayout();

        var itemContainer = TestUtil.AssertItemRealized(vsp, "Item 550");
        var position = itemContainer.TranslatePoint(new Point(0, 0), vsp);
        Assert.Equal(ViewportHeight - TestUtil.DefaultItemHeight, position.Y);
    }
}

using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Xunit;

namespace VirtualizingWrapPanelTest.Tests;

public class GroupingTest_ItemsDoNotOverlap
{
    private const int ViewportWidth = 500;

    private const int ViewportHeight = 500;

    private const int HeaderHeight = 20;

    private const int Margin = 5;

    private const int ItemsPerGroup = 15; // 3 rows a 5 items

    private const int GroupsCount = 10;

    private const int ItemsCount = ItemsPerGroup * GroupsCount; // 150

    private readonly ListBox itemsControl;

    private readonly VirtualizingStackPanel vsp;

    public GroupingTest_ItemsDoNotOverlap()
    {
        itemsControl = Setup(TestUtil.GenerateItems(ItemsCount, ItemsPerGroup));
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


    // #90 Items may sometimes appear overlapping 
    [WpfFact]
    public async Task ItemsDoNotOverlap()
    {
        // scroll to end
        itemsControl.ScrollIntoView(itemsControl.Items[itemsControl.Items.Count - 1]);
        itemsControl.UpdateLayout();
        await Task.Delay(1000); // TODO: find a better way to wait for layout to complete
        TestUtil.AssertItemRealized(vsp, "Item 150");

        // check for overlapping items
        var itemContainers = TestUtil.FindItemContainers(vsp);
        foreach (var itemContainer in itemContainers)
        {
            var item = (TestItem)itemContainer.DataContext;
            var bounds = GetItemContainerBounds(itemContainer);

            foreach (var otherItemContainer in itemContainers.Except([itemContainer]))
            {
                var otherItem = (TestItem)otherItemContainer.DataContext;
                var otherBounds = GetItemContainerBounds(otherItemContainer);
                var intersection = Rect.Intersect(bounds, otherBounds);
                bool overlap = intersection.Width > 0 && intersection.Height > 0;
                Assert.False(overlap, $"Items {item.Name} and {otherItem.Name} are overlapping.");
            }
        }
    }

    private Rect GetItemContainerBounds(FrameworkElement element)
    {
        var position = element.TranslatePoint(new Point(0, 0), vsp);
        return new Rect(position, new Size(element.ActualWidth, element.ActualHeight));
    }
}

using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using WpfToolkit.Controls;
using Xunit;

namespace VirtualizingWrapPanelTest.Tests;

public class GroupingTest
{
    private const int ViewportWidth = 500;

    private const int ViewportHeight = 400;

    private const int HeaderHeight = 10;

    private static readonly string HeaderTemplate = $"""
        <DataTemplate xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"> 
            <Border Height="{HeaderHeight}"/>
        </DataTemplate>
        """;

    private readonly ListBox itemsControl;

    private readonly VirtualizingStackPanel vsp;

    public GroupingTest()
    {
        itemsControl = Setup(TestUtil.GenerateItems(1000));
        vsp = TestUtil.GetVisualChild<VirtualizingStackPanel>(itemsControl)!;
    }

    private static ListBox Setup(ObservableCollection<TestItem> items)
    {
        var collectionView = CollectionViewSource.GetDefaultView(items);
        collectionView.GroupDescriptions.Add(new PropertyGroupDescription(nameof(TestItem.Group)));
        var itemsControl = TestUtil.CreateListBox(ViewportWidth + 5 /*Margin*/ + 2 /*Padding*/, ViewportHeight + 2 /*Padding*/, collectionView);
        itemsControl.GroupStyle.Add(new GroupStyle() { HeaderTemplate = TestUtil.CreateDateTemplate(HeaderTemplate) });
        VirtualizingPanel.SetIsVirtualizingWhenGrouping(itemsControl, true);
        return itemsControl;
    }

    [UITheory]
    [InlineData(VirtualizationMode.Standard)]
    [InlineData(VirtualizationMode.Recycling)]
    public void SetVerticalOffset(VirtualizationMode virtualizationMode)
    {
        VirtualizingPanel.SetVirtualizationMode(itemsControl, virtualizationMode);
        itemsControl.UpdateLayout();
        Assert.Equal(20100, vsp.ExtentHeight);

        vsp.SetVerticalOffset(1900);
        itemsControl.UpdateLayout();

        TestUtil.AssertItem(vsp, "Item 100", 400 + 5 /*Margin*/, 10);
        TestUtil.AssertItem(vsp, "Item 101", 5 /*Margin*/, 120);
        Assert.Equal(2, TestUtil.GetVisualChilds<VirtualizingWrapPanel>(itemsControl).Count);
    }

    [UITheory]
    [InlineData(VirtualizationMode.Standard)]
    [InlineData(VirtualizationMode.Recycling)]
    public void ScrollIntoView_ItemAfterViewportAndCache(VirtualizationMode virtualizationMode)
    {
        VirtualizingPanel.SetVirtualizationMode(itemsControl, virtualizationMode);
        itemsControl.UpdateLayout();

        itemsControl.ScrollIntoView(itemsControl.Items[549]); // Item 550
        itemsControl.UpdateLayout();

        TestUtil.AssertItem(vsp, "Item 550", 400 + 5 /*Margin*/, ViewportHeight - TestUtil.DefaultItemHeight);
        Assert.Equal(10660, vsp.VerticalOffset);
    }

    [UITheory]
    [InlineData(VirtualizationMode.Standard)]
    [InlineData(VirtualizationMode.Recycling)]
    public void ScrollIntoView_ItemBeforeViewportAndCache(VirtualizationMode virtualizationMode)
    {
        VirtualizingPanel.SetVirtualizationMode(itemsControl, virtualizationMode);
        itemsControl.UpdateLayout();

        vsp.SetVerticalOffset(15000);
        itemsControl.UpdateLayout();
        Assert.Equal(15000, vsp.VerticalOffset);

        itemsControl.ScrollIntoView(itemsControl.Items[549]); // Item 550
        itemsControl.UpdateLayout();

        TestUtil.AssertItem(vsp, "Item 550", 400 + 5 /*Margin*/, 0);
        Assert.Equal(10960, vsp.VerticalOffset);
    }

    [UITheory]
    [InlineData(VirtualizationMode.Standard)]
    [InlineData(VirtualizationMode.Recycling)]
    public void ScrollIntoView_ItemAfterViewportAndCache_DifferentGroupSizes(VirtualizationMode virtualizationMode)
    {
        var itemsControl = Setup(TestUtil.GenerateItemsWithRandomGroupSizes(1000));
        var vsp = TestUtil.GetVisualChild<VirtualizingStackPanel>(itemsControl)!;
        VirtualizingPanel.SetVirtualizationMode(itemsControl, virtualizationMode);
        itemsControl.UpdateLayout();

        itemsControl.ScrollIntoView(itemsControl.Items[549]); // Item 550
        itemsControl.UpdateLayout();

        var itemContainer = TestUtil.AssertItemRealized(vsp, "Item 550");
        var position = itemContainer.TranslatePoint(new Point(0, 0), vsp);
        Assert.Equal(ViewportHeight - TestUtil.DefaultItemHeight, position.Y);
    }

    // TODO: ScrollIntoView orientation
}

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

    private const int HeaderHeight = 20;

    private const int ItemCount = 1000;

    private const int ItemsPerRow = ViewportWidth / TestUtil.DefaultItemWidth;

    private static readonly int RowCount = (int)Math.Ceiling((double)ItemCount / ItemsPerRow);

    private const int GroupCount = 10;

    private const int ItemPerGroup = ItemCount / GroupCount;

    private static readonly int RowsPerGroup = (int)Math.Ceiling((double)ItemPerGroup / ItemsPerRow);

    private static readonly int GroupHeightWithHeader = HeaderHeight + RowsPerGroup * TestUtil.DefaultItemHeight;

    private const int Margin = 5;

    private static readonly string HeaderTemplate = $"""
        <DataTemplate xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"> 
            <Border Background="Green" Height="{HeaderHeight}"/>
        </DataTemplate>
        """;

    private readonly ListBox itemsControl;

    private readonly VirtualizingStackPanel vsp;

    public GroupingTest()
    {
        itemsControl = Setup(TestUtil.GenerateItems(ItemCount, ItemPerGroup));
        vsp = TestUtil.GetVisualChild<VirtualizingStackPanel>(itemsControl)!;
    }

    private static ListBox Setup(ObservableCollection<TestItem> items)
    {
        var collectionView = CollectionViewSource.GetDefaultView(items);
        collectionView.GroupDescriptions.Add(new PropertyGroupDescription(nameof(TestItem.Group)));
        var itemsControl = TestUtil.CreateListBox(ViewportWidth + Margin + 2 /*Padding*/, ViewportHeight + 2 /*Padding*/, collectionView);
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
        double expectedExtetHeight = RowCount * TestUtil.DefaultItemHeight + GroupCount * HeaderHeight;
        Assert.Equal(expectedExtetHeight, vsp.ExtentHeight);

        vsp.SetVerticalOffset(GroupHeightWithHeader - 50);
        itemsControl.UpdateLayout();

        AssertItem(vsp, "Item 100", 400, -50);
        AssertItem(vsp, "Item 101", 0, TestUtil.DefaultItemHeight + HeaderHeight - 50);
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

        AssertItem(vsp, "Item 550", 400, ViewportHeight - TestUtil.DefaultItemHeight);
        Assert.Equal(10720, vsp.VerticalOffset);
    }

    [UITheory]
    [InlineData(VirtualizationMode.Standard)]
    [InlineData(VirtualizationMode.Recycling)]
    public void ScrollIntoView_ItemBeforeViewportAndCache(VirtualizationMode virtualizationMode)
    {
        VirtualizingPanel.SetVirtualizationMode(itemsControl, virtualizationMode);
        itemsControl.UpdateLayout();
        vsp.SetVerticalOffset(7 * GroupHeightWithHeader);
        itemsControl.UpdateLayout();
        Assert.Equal(7 * GroupHeightWithHeader, vsp.VerticalOffset);
        AssertItem(vsp, "Item 701", 0, HeaderHeight);

        itemsControl.ScrollIntoView(itemsControl.Items[549]); // Item 550
        itemsControl.UpdateLayout();

        AssertItem(vsp, "Item 550", 400, 0);
        double expectedVerticalOffset = 5 * GroupHeightWithHeader + HeaderHeight + 9 * TestUtil.DefaultItemHeight;
        Assert.Equal(expectedVerticalOffset, vsp.VerticalOffset);
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

    // Bug #61: Items disappear from top row while using grouping 
    [UITheory]
    [InlineData(VirtualizationMode.Standard)]
    [InlineData(VirtualizationMode.Recycling)]
    public void HeaderSizeIsIncludedInItemRangeCalculation_Start(VirtualizationMode virtualizationMode)
    {
        VirtualizingPanel.SetVirtualizationMode(itemsControl, virtualizationMode);
        VirtualizingPanel.SetCacheLengthUnit(itemsControl, VirtualizationCacheLengthUnit.Item);
        VirtualizingPanel.SetCacheLength(itemsControl, new VirtualizationCacheLength(3));
        itemsControl.UpdateLayout();

        vsp.SetVerticalOffset(TestUtil.DefaultItemHeight + HeaderHeight / 2);
        itemsControl.UpdateLayout();

        TestUtil.AssertItemRealized(vsp, "Item 1");
    }

    // Bug #61: Items disappear from top row while using grouping 
    [UITheory]
    [InlineData(VirtualizationMode.Standard)]
    [InlineData(VirtualizationMode.Recycling)]
    public void HeaderSizeIsIncludedInItemRangeCalculation_End(VirtualizationMode virtualizationMode)
    {
        itemsControl.Height = 415;
        int cacheLength = 3;
        VirtualizingPanel.SetVirtualizationMode(itemsControl, virtualizationMode);
        VirtualizingPanel.SetCacheLengthUnit(itemsControl, VirtualizationCacheLengthUnit.Item);
        VirtualizingPanel.SetCacheLength(itemsControl, new VirtualizationCacheLength(cacheLength));
        itemsControl.UpdateLayout();

        TestUtil.AssertItemRangeRealized(vsp, 1, 20 + cacheLength);

        vsp.SetVerticalOffset(HeaderHeight / 2);
        itemsControl.UpdateLayout();

        TestUtil.AssertItemRangeRealized(vsp, 1, 25 + cacheLength);
    }

    // Bug #68: ArgumentOutOfRangeException when adding GroupStyle at runtime
    [WpfTheory]
    [InlineData(VirtualizationMode.Standard)]
    [InlineData(VirtualizationMode.Recycling)]
    public void AddGroupStyle(VirtualizationMode virtualizationMode)
    {
        VirtualizingPanel.SetVirtualizationMode(itemsControl, virtualizationMode);
        itemsControl.GroupStyle.Clear();
        itemsControl.UpdateLayout();
        var vwp = TestUtil.GetVisualChild<VirtualizingWrapPanel>(itemsControl)!;

        itemsControl.GroupStyle.Add(new GroupStyle() { HeaderTemplate = TestUtil.CreateDateTemplate(HeaderTemplate) });

        // ensure no exception is thrown when the VirtualizingWrapPanel is measured after adding the GroupStyle (#68)
        vwp.Measure(new Size(ViewportWidth, ViewportHeight));     

        itemsControl.UpdateLayout();
        var vsp = TestUtil.GetVisualChild<VirtualizingStackPanel>(itemsControl)!;
        TestUtil.AssertItemRangeRealized(vsp, 1, 1);
    }

    private static void AssertItem(VirtualizingPanel virtualizingPanel, string itemName, int x, int y, int width = TestUtil.DefaultItemWidth, int height = TestUtil.DefaultItemHeight)
    {
        TestUtil.AssertItem(virtualizingPanel, itemName, x + Margin, y, width, height);
    }
}

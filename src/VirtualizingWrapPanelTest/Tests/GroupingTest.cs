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

    private const int ItemsPerRow = ViewportWidth / TestUtil.DefaultItemWidth; // 5

    private const int RowCount = (int)((double)ItemCount / ItemsPerRow); // 200

    private const int GroupCount = 10;

    private const int ItemPerGroup = ItemCount / GroupCount; // 100

    private const int RowsPerGroup = (int)((double)ItemPerGroup / ItemsPerRow); // 20

    private const int GroupHeightWithHeader = HeaderHeight + RowsPerGroup * TestUtil.DefaultItemHeight;

    private const int Margin = 5;

    private readonly TestRunner testRunner;

    private readonly ListBox itemsControl;

    private readonly VirtualizingStackPanel vsp;

    public GroupingTest()
    {
        testRunner = new TestRunner();
        itemsControl = Setup(TestUtil.GenerateItems(ItemCount, ItemPerGroup));
        vsp = TestUtil.GetVisualChild<VirtualizingStackPanel>(itemsControl)!;
    }

    private ListBox Setup(ObservableCollection<TestItem> items)
    {
        var collectionView = CollectionViewSource.GetDefaultView(items);
        collectionView.GroupDescriptions.Add(new PropertyGroupDescription(nameof(TestItem.Group)));
        var itemsControl = TestUtil.CreateListBox(ViewportWidth + Margin + 2 /*Padding*/, ViewportHeight + 2 /*Padding*/, collectionView, testRunner.Window);
        itemsControl.GroupStyle.Add(new GroupStyle() { HeaderTemplate = TestUtil.CreateDefaultGroupHeaderTemplate(HeaderHeight) });
        VirtualizingPanel.SetIsVirtualizingWhenGrouping(itemsControl, true);
        return itemsControl;
    }

    [WpfTheory]
    [InlineData(VirtualizationMode.Standard)]
    [InlineData(VirtualizationMode.Recycling)]
    public async Task SetVerticalOffset(VirtualizationMode virtualizationMode)
    {
        VirtualizingPanel.SetVirtualizationMode(itemsControl, virtualizationMode);
        await testRunner.UpdateLayoutAsync(itemsControl);
        double expectedExtentHeight = RowCount * TestUtil.DefaultItemHeight + GroupCount * HeaderHeight;
        Assert.Equal(expectedExtentHeight, vsp.ExtentHeight);

        vsp.SetVerticalOffset(GroupHeightWithHeader - 50);
        await testRunner.UpdateLayoutAsync(itemsControl);

        AssertItem(vsp, "Item 100", 400, -50);
        AssertItem(vsp, "Item 101", 0, TestUtil.DefaultItemHeight + HeaderHeight - 50);
        Assert.Equal(2, TestUtil.GetVisualChilds<VirtualizingWrapPanel>(itemsControl).Count);
    }

    [WpfTheory]
    [InlineData(VirtualizationMode.Standard)]
    [InlineData(VirtualizationMode.Recycling)]
    public async Task ScrollIntoView_ItemAfterViewportAndCache(VirtualizationMode virtualizationMode)
    {
        VirtualizingPanel.SetVirtualizationMode(itemsControl, virtualizationMode);
        await testRunner.UpdateLayoutAsync(itemsControl);

        itemsControl.ScrollIntoView(itemsControl.Items[549]); // Item 550
        await testRunner.UpdateLayoutAsync(itemsControl);

        double expectedVerticalOffset = 5 * GroupHeightWithHeader + HeaderHeight + 6 * TestUtil.DefaultItemHeight;
        AssertItem(vsp, "Item 550", 400, ViewportHeight - TestUtil.DefaultItemHeight);
        Assert.Equal(expectedVerticalOffset, vsp.VerticalOffset);
    }

    [WpfTheory]
    [InlineData(VirtualizationMode.Standard)]
    [InlineData(VirtualizationMode.Recycling)]
    public async Task ScrollIntoView_ItemBeforeViewportAndCache(VirtualizationMode virtualizationMode)
    {
        VirtualizingPanel.SetVirtualizationMode(itemsControl, virtualizationMode);
        await testRunner.UpdateLayoutAsync(itemsControl);

        vsp.SetVerticalOffset(7 * GroupHeightWithHeader);
        await testRunner.UpdateLayoutAsync(itemsControl);
        Assert.Equal(7 * GroupHeightWithHeader, vsp.VerticalOffset);
        AssertItem(vsp, "Item 701", 0, HeaderHeight);

        itemsControl.ScrollIntoView(itemsControl.Items[549]); // Item 550
        await testRunner.UpdateLayoutAsync(itemsControl);

        AssertItem(vsp, "Item 550", 400, 0);
        double expectedVerticalOffset = 5 * GroupHeightWithHeader + HeaderHeight + 9 * TestUtil.DefaultItemHeight;
        Assert.Equal(expectedVerticalOffset, vsp.VerticalOffset);
    }

    // TODO: ScrollIntoView orientation

    // Bug #61: Items disappear from top row while using grouping 
    [WpfTheory]
    [InlineData(VirtualizationMode.Standard)]
    [InlineData(VirtualizationMode.Recycling)]
    public async Task HeaderSizeIsIncludedInItemRangeCalculation_StartAsync(VirtualizationMode virtualizationMode)
    {
        VirtualizingPanel.SetVirtualizationMode(itemsControl, virtualizationMode);
        VirtualizingPanel.SetCacheLengthUnit(itemsControl, VirtualizationCacheLengthUnit.Item);
        VirtualizingPanel.SetCacheLength(itemsControl, new VirtualizationCacheLength(3));
        await testRunner.UpdateLayoutAsync(itemsControl);

        vsp.SetVerticalOffset(TestUtil.DefaultItemHeight + HeaderHeight / 2);
        await testRunner.UpdateLayoutAsync(itemsControl);

        TestUtil.AssertItemRealized(vsp, "Item 1");
    }

    // Bug #61: Items disappear from top row while using grouping 
    [WpfTheory]
    [InlineData(VirtualizationMode.Standard)]
    [InlineData(VirtualizationMode.Recycling)]
    public async Task HeaderSizeIsIncludedInItemRangeCalculation_End(VirtualizationMode virtualizationMode)
    {
        itemsControl.Height = 415;
        int cacheLength = 3;
        VirtualizingPanel.SetVirtualizationMode(itemsControl, virtualizationMode);
        VirtualizingPanel.SetCacheLengthUnit(itemsControl, VirtualizationCacheLengthUnit.Item);
        VirtualizingPanel.SetCacheLength(itemsControl, new VirtualizationCacheLength(cacheLength));
        await testRunner.UpdateLayoutAsync(itemsControl);

        TestUtil.AssertItemRangeRealized(vsp, 1, 20 + cacheLength);

        vsp.SetVerticalOffset(HeaderHeight / 2);
        await testRunner.UpdateLayoutAsync(itemsControl);

        TestUtil.AssertItemRangeRealized(vsp, 1, 25 + cacheLength);
    }

    // Bug #68: ArgumentOutOfRangeException when adding GroupStyle at runtime
    [WpfTheory]
    [InlineData(VirtualizationMode.Standard)]
    [InlineData(VirtualizationMode.Recycling)]
    public async Task AddGroupStyle(VirtualizationMode virtualizationMode)
    {
        VirtualizingPanel.SetVirtualizationMode(itemsControl, virtualizationMode);
        itemsControl.GroupStyle.Clear();
        await testRunner.UpdateLayoutAsync(itemsControl);
        var vwp = TestUtil.GetVisualChild<VirtualizingWrapPanel>(itemsControl)!;

        itemsControl.GroupStyle.Add(new GroupStyle() { HeaderTemplate = TestUtil.CreateDefaultGroupHeaderTemplate(HeaderHeight) });

        // ensure no exception is thrown when the VirtualizingWrapPanel is measured after adding the GroupStyle (#68)
        vwp.Measure(new Size(ViewportWidth, ViewportHeight));

        await testRunner.UpdateLayoutAsync(itemsControl);
        var vsp = TestUtil.GetVisualChild<VirtualizingStackPanel>(itemsControl)!;
        TestUtil.AssertItemRangeRealized(vsp, 1, 1);
    }

    private static void AssertItem(VirtualizingPanel virtualizingPanel, string itemName, int x, int y, int width = TestUtil.DefaultItemWidth, int height = TestUtil.DefaultItemHeight)
    {
        TestUtil.AssertItem(virtualizingPanel, itemName, x + Margin, y, width, height);
    }
}

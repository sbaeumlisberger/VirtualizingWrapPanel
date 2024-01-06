using System.Windows.Controls;
using System.Windows.Data;
using WpfToolkit.Controls;
using Xunit;

namespace VirtualizingWrapPanelTest.Tests;

public class GroupingTest
{
    private ListBox itemsControl;

    private VirtualizingStackPanel vsp;

    public GroupingTest()
    {
        var headerTemplate = """
            <DataTemplate xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"> 
                <Border Height="10"/>
            </DataTemplate>
            """;
        var collectionView = CollectionViewSource.GetDefaultView(TestUtil.GenerateItems(1000));
        collectionView.GroupDescriptions.Add(new PropertyGroupDescription(nameof(TestItem.Group)));
        itemsControl = TestUtil.CreateListBox(500 + 5 /*Margin*/ + 2 /*Padding*/, 400 + 2 /*Padding*/, collectionView);
        itemsControl.GroupStyle.Add(new GroupStyle() { HeaderTemplate = TestUtil.CreateDateTemplate(headerTemplate) });
        VirtualizingPanel.SetIsVirtualizingWhenGrouping(itemsControl, true);
        vsp = TestUtil.GetVisualChild<VirtualizingStackPanel>(itemsControl)!;
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

        TestUtil.AssertItem(vsp, "Item 550", 400 + 5 /*Margin*/, 300);
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

    // TODO: ScrollIntoView orientation
}

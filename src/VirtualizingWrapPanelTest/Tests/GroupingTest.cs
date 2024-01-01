using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
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
        itemsControl = TestUtil.CreateListBox(500 + 5 + 2, 400 + 2, collectionView);
        itemsControl.GroupStyle.Add(new GroupStyle() { HeaderTemplate = TestUtil.CreateDateTemplate(headerTemplate) });
        VirtualizingPanel.SetIsVirtualizingWhenGrouping(itemsControl, true);
        itemsControl.UpdateLayout();
        vsp = TestUtil.GetVisualChild<VirtualizingStackPanel>(itemsControl)!;
    }

    [UIFact]
    public void Test()
    {
        vsp.SetVerticalOffset(1900);
        itemsControl.UpdateLayout();

        Assert.Equal(2, TestUtil.GetVisualChilds<VirtualizingWrapPanel>(itemsControl).Count);
    }

    [UIFact]
    public void ScrollIntoView_ItemAfterViewportAndCache()
    {
        itemsControl.ScrollIntoView(itemsControl.Items[549]); // Item 550
        itemsControl.UpdateLayout();
     
        Assert.NotNull(vsp);
        Assert.Equal(10660, vsp.VerticalOffset);
    }

    [UIFact]
    public void ScrollIntoView_ItemBeforeViewportAndCache()
    {
        vsp.SetVerticalOffset(15000);
        itemsControl.UpdateLayout();
        Assert.Equal(15000, vsp.VerticalOffset);

        itemsControl.ScrollIntoView(itemsControl.Items[549]); // Item 550
        itemsControl.UpdateLayout();

        Assert.Equal(10960, vsp.VerticalOffset);
    }

    // TODO: ScrollIntoView orientation
}

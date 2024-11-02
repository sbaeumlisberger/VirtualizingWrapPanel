using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using WpfToolkit.Controls;
using Xunit;
using Assert = Xunit.Assert;

namespace VirtualizingWrapPanelTest.Tests;

public class BasisTest
{
    private VirtualizingWrapPanel vwp = TestUtil.CreateVirtualizingWrapPanel(500, 400);

    [UIFact]
    public void NoItems()
    {
        vwp = TestUtil.CreateVirtualizingWrapPanel(500, 400, itemCount: 0);

        Assert.Equal(0, vwp.DesiredSize.Width);
        Assert.Equal(0, vwp.DesiredSize.Height);
    }

    [UIFact]
    public void Inital()
    {
        Assert.Equal(500, vwp.DesiredSize.Width);
        Assert.Equal(400, vwp.DesiredSize.Height);

        Assert.Equal(500, vwp.ExtentWidth);
        Assert.Equal(20_000, vwp.ExtentHeight);

        Assert.Equal(40, vwp.Children.Count);

        // in viewport
        TestUtil.AssertItem(vwp, "Item 1", 0, 0);
        TestUtil.AssertItem(vwp, "Item 2", 100, 0);
        TestUtil.AssertItem(vwp, "Item 5", 400, 0);
        TestUtil.AssertItem(vwp, "Item 6", 0, 100);
        TestUtil.AssertItem(vwp, "Item 11", 0, 200);
        TestUtil.AssertItem(vwp, "Item 16", 0, 300);

        // after viewport
        TestUtil.AssertItem(vwp, "Item 21", 0, 400);
        TestUtil.AssertItem(vwp, "Item 26", 0, 500);
    }

    [UIFact]
    public void OffsetOfOneRow()
    {
        vwp.SetVerticalOffset(100);
        vwp.UpdateLayout();

        Assert.Equal(500, vwp.DesiredSize.Width);
        Assert.Equal(400, vwp.DesiredSize.Height);

        Assert.Equal(45, vwp.Children.Count);

        // before viewport
        TestUtil.AssertItem(vwp, "Item 1", 0, -100);
        TestUtil.AssertItem(vwp, "Item 2", 100, -100);
        TestUtil.AssertItem(vwp, "Item 5", 400, -100);

        // in viewport
        TestUtil.AssertItem(vwp, "Item 6", 0, 0);
        TestUtil.AssertItem(vwp, "Item 7", 100, 0);
        TestUtil.AssertItem(vwp, "Item 10", 400, 0);
        TestUtil.AssertItem(vwp, "Item 11", 0, 100);
        TestUtil.AssertItem(vwp, "Item 16", 0, 200);
        TestUtil.AssertItem(vwp, "Item 21", 0, 300);

        // after viewport
        TestUtil.AssertItem(vwp, "Item 26", 0, 400);
        TestUtil.AssertItem(vwp, "Item 31", 0, 500);
    }

    [UIFact]
    public void OffsetOfMultiplePages()
    {
        vwp.SetVerticalOffset(800);
        vwp.UpdateLayout();

        Assert.Equal(500, vwp.DesiredSize.Width);
        Assert.Equal(400, vwp.DesiredSize.Height);

        Assert.Equal(60, vwp.Children.Count);

        // before viewport
        TestUtil.AssertItem(vwp, "Item 31", 0, -200);
        TestUtil.AssertItem(vwp, "Item 36", 0, -100);
        TestUtil.AssertItem(vwp, "Item 37", 100, -100);
        TestUtil.AssertItem(vwp, "Item 40", 400, -100);

        // in viewport
        TestUtil.AssertItem(vwp, "Item 41", 0, 0);
        TestUtil.AssertItem(vwp, "Item 42", 100, 0);
        TestUtil.AssertItem(vwp, "Item 45", 400, 0);
        TestUtil.AssertItem(vwp, "Item 46", 0, 100);
        TestUtil.AssertItem(vwp, "Item 51", 0, 200);
        TestUtil.AssertItem(vwp, "Item 56", 0, 300);

        // after viewport
        TestUtil.AssertItem(vwp, "Item 61", 0, 400);
        TestUtil.AssertItem(vwp, "Item 66", 0, 500);
    }

    [UIFact]
    public void OffsetOfMultiplePages_Recycling()
    {
        VirtualizingPanel.SetVirtualizationMode(vwp.ItemsControl, VirtualizationMode.Recycling);
        vwp.UpdateLayout();

        vwp.SetVerticalOffset(800);
        vwp.UpdateLayout();

        Assert.Equal(500, vwp.DesiredSize.Width);
        Assert.Equal(400, vwp.DesiredSize.Height);

        Assert.Equal(60, vwp.Children.Count);

        // before viewport
        TestUtil.AssertItem(vwp, "Item 31", 0, -200);
        TestUtil.AssertItem(vwp, "Item 36", 0, -100);
        TestUtil.AssertItem(vwp, "Item 37", 100, -100);
        TestUtil.AssertItem(vwp, "Item 40", 400, -100);

        // in viewport
        TestUtil.AssertItem(vwp, "Item 41", 0, 0);
        TestUtil.AssertItem(vwp, "Item 42", 100, 0);
        TestUtil.AssertItem(vwp, "Item 45", 400, 0);
        TestUtil.AssertItem(vwp, "Item 46", 0, 100);
        TestUtil.AssertItem(vwp, "Item 51", 0, 200);
        TestUtil.AssertItem(vwp, "Item 56", 0, 300);

        // after viewport
        TestUtil.AssertItem(vwp, "Item 61", 0, 400);
        TestUtil.AssertItem(vwp, "Item 66", 0, 500);
    }

    [UIFact]
    public void RowPartiallyInViewport()
    {
        vwp.SetVerticalOffset(850);
        vwp.UpdateLayout();

        Assert.Equal(500, vwp.DesiredSize.Width);
        Assert.Equal(400, vwp.DesiredSize.Height);

        Assert.Equal(65, vwp.Children.Count);

        // before viewport
        TestUtil.AssertItem(vwp, "Item 21", 0, -450);
        TestUtil.AssertItem(vwp, "Item 26", 0, -350);
        TestUtil.AssertItem(vwp, "Item 31", 0, -250);
        TestUtil.AssertItem(vwp, "Item 36", 0, -150);
        TestUtil.AssertItem(vwp, "Item 37", 100, -150);
        TestUtil.AssertItem(vwp, "Item 40", 400, -150);

        // in viewport
        TestUtil.AssertItem(vwp, "Item 41", 0, -50);
        TestUtil.AssertItem(vwp, "Item 42", 100, -50);
        TestUtil.AssertItem(vwp, "Item 45", 400, -50);
        TestUtil.AssertItem(vwp, "Item 46", 0, 50);
        TestUtil.AssertItem(vwp, "Item 51", 0, 150);
        TestUtil.AssertItem(vwp, "Item 56", 0, 250);
        TestUtil.AssertItem(vwp, "Item 61", 0, 350);

        // after viewport
        TestUtil.AssertItem(vwp, "Item 66", 0, 450);
        TestUtil.AssertItem(vwp, "Item 71", 0, 550);
        TestUtil.AssertItem(vwp, "Item 76", 0, 650);
        TestUtil.AssertItem(vwp, "Item 81", 0, 750);
    }


    [UIFact]
    public void BringIndexIntoView_AfterViewport()
    {
        vwp.BringIndexIntoViewPublic(500);
        vwp.UpdateLayout();

        Assert.Equal(9700, vwp.VerticalOffset);
        Assert.Equal(500, vwp.ExtentWidth);
        Assert.Equal(20_000, vwp.ExtentHeight);
        Assert.Equal(60, vwp.Children.Count);
        TestUtil.AssertItem(vwp, "Item 501", 0, 300);
    }

    [UIFact]
    public void BringIndexIntoView_AfterViewport_Recycling()
    {
        VirtualizingPanel.SetVirtualizationMode(vwp.ItemsControl, VirtualizationMode.Recycling);
        vwp.UpdateLayout();

        vwp.BringIndexIntoViewPublic(500);
        vwp.UpdateLayout();

        Assert.Equal(9700, vwp.VerticalOffset);
        Assert.Equal(500, vwp.ExtentWidth);
        Assert.Equal(20_000, vwp.ExtentHeight);
        Assert.Equal(60, vwp.Children.Count);
        TestUtil.AssertItem(vwp, "Item 501", 0, 300);
    }

    [UIFact]
    public void BringIndexIntoView_BeforeViewport()
    {
        vwp.SetVerticalOffset(16_000);
        vwp.UpdateLayout();

        vwp.BringIndexIntoViewPublic(500);
        vwp.UpdateLayout();

        Assert.Equal(10000, vwp.VerticalOffset);
        Assert.Equal(500, vwp.ExtentWidth);
        Assert.Equal(20_000, vwp.ExtentHeight);
        Assert.Equal(60, vwp.Children.Count);
        TestUtil.AssertItem(vwp, "Item 501", 0, 0);
    }

    [UIFact]
    public void BringIndexIntoView_AfterViewport_VerticalOrientation()
    {
        vwp.Orientation = Orientation.Vertical;
        vwp.UpdateLayout();

        vwp.BringIndexIntoViewPublic(500);
        vwp.UpdateLayout();

        Assert.Equal(12_100, vwp.HorizontalOffset);
        Assert.Equal(25_000, vwp.ExtentWidth);
        Assert.Equal(400, vwp.ExtentHeight);
        Assert.Equal(60, vwp.Children.Count);
        TestUtil.AssertItem(vwp, "Item 501", 400, 0);
    }


    [UIFact]
    public void BringIndexIntoView_BeforeViewport_VerticalOrientation()
    {
        vwp.Orientation = Orientation.Vertical;
        vwp.UpdateLayout();

        vwp.SetHorizontalOffset(20_000);
        vwp.UpdateLayout();

        vwp.BringIndexIntoViewPublic(500);
        vwp.UpdateLayout();

        Assert.Equal(12_500, vwp.HorizontalOffset);
        Assert.Equal(25_000, vwp.ExtentWidth);
        Assert.Equal(400, vwp.ExtentHeight);
        Assert.Equal(60, vwp.Children.Count);
        TestUtil.AssertItem(vwp, "Item 501", 0, 0);
    }

    [UIFact]
    public void BringIndexIntoView_BeforeViewport_Recycling()
    {
        VirtualizingPanel.SetVirtualizationMode(vwp.ItemsControl, VirtualizationMode.Recycling);
        vwp.UpdateLayout();
        vwp.SetVerticalOffset(16_000);
        vwp.UpdateLayout();

        vwp.BringIndexIntoViewPublic(500);
        vwp.UpdateLayout();

        Assert.Equal(10000, vwp.VerticalOffset);
        Assert.Equal(500, vwp.ExtentWidth);
        Assert.Equal(20_000, vwp.ExtentHeight);
        TestUtil.AssertItem(vwp, "Item 501", 0, 0);
    }

    [UIFact]
    public void MakeVisible_InViewport()
    {
        var child6 = vwp.Children[5];

        var visibleRect = vwp.MakeVisible(child6, new Rect(0, 0, 100, 100));

        Assert.Equal(new Rect(0, 0, 100, 100), visibleRect);
        Assert.Equal(0, vwp.VerticalOffset);
    }

    [UIFact]
    public void MakeVisible_AfterViewport()
    {
        var child21 = vwp.Children[20];

        var visibleRect = vwp.MakeVisible(child21, new Rect(0, 0, 100, 100));

        Assert.Equal(new Rect(0, 0, 100, 100), visibleRect);
        Assert.Equal(100, vwp.VerticalOffset);
    }

    [UIFact]
    public void MakeVisible_BeforeViewport()
    {
        vwp.SetVerticalOffset(100);
        vwp.UpdateLayout();
        var child1 = vwp.Children[0];

        var visibleRect = vwp.MakeVisible(child1, new Rect(0, 0, 100, 100));

        Assert.Equal(new Rect(0, 0, 100, 100), visibleRect);
        Assert.Equal(0, vwp.VerticalOffset);
    }

    [UIFact]
    public void MakeVisible_BeforeViewportAndGreaterThanViewport()
    {
        vwp.ItemsControl.Height = 60;
        vwp.SetVerticalOffset(100);
        vwp.UpdateLayout();
        var child1 = vwp.Children[0];

        var visibleRect = vwp.MakeVisible(child1, new Rect(0, 0, 100, 100));

        Assert.Equal(new Rect(0, 0, 100, 60), visibleRect);
        Assert.Equal(0, vwp.VerticalOffset);
    }


    [UIFact]
    public void MakeVisible_AfterViewportAndGreaterThanViewport()
    {
        vwp.ItemsControl.Height = 60;
        vwp.UpdateLayout();
        var child6 = vwp.Children[5];

        var visibleRect = vwp.MakeVisible(child6, new Rect(0, 0, 100, 100));

        Assert.Equal(new Rect(0, 40, 100, 60), visibleRect);
        Assert.Equal(100, vwp.VerticalOffset);
    }

    // TODO item based scrolling

    [UIFact]
    public void ExtentIsUpdatedWhenItemSizeChanged()
    {
        vwp.ItemSize = new Size(100, 200);
        vwp.UpdateLayout();

        Assert.Equal(40_000, vwp.ExtentHeight);

        vwp.ItemSize = new Size(200, 200);
        vwp.UpdateLayout();

        Assert.Equal(100_000, vwp.ExtentHeight);

        vwp.ItemSize = new Size(100, 100);
        vwp.UpdateLayout();

        Assert.Equal(20_000, vwp.ExtentHeight);

    }

    // TODO item size stretch content
    //vwp.ItemsControl.ItemContainerStyle = new Style()
    //{
    //    Setters = {
    //        new Setter(Control.HorizontalContentAlignmentProperty, HorizontalAlignment.Stretch),
    //            new Setter(Control.VerticalContentAlignmentProperty, VerticalAlignment.Stretch),
    //        }
    //};

    [UIFact]
    public void ChildrensAreMeasuredWhenItemSizeChanged() // Issue #50
    {
        var itemTemplate = """
            <DataTemplate xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"> 
                <Grid Width="100" Height="100">
                    <TextBlock Text="{Binding Name}"/>
                </Grid>
            </DataTemplate>
            """;
        vwp.ItemsControl.ItemTemplate = TestUtil.CreateDateTemplate(itemTemplate);
        vwp.ItemSize = new Size(100, 100);
        vwp.UpdateLayout();

        vwp.ItemSize = new Size(50, 50);

        foreach (var child in vwp.Children.Cast<UIElement>())
        {
            Assert.False(child.IsMeasureValid);
        }

        vwp.UpdateLayout();

        foreach (var child in vwp.Children.Cast<UIElement>())
        {
            Assert.True(child.IsMeasureValid);

            Assert.Equal(50, child.DesiredSize.Width);
            Assert.Equal(50, child.DesiredSize.Height);
        }
    }

    [UIFact]
    public void ItemRemoved()
    {
        var items = (ObservableCollection<TestItem>)vwp.ItemsControl.ItemsSource;

        items.RemoveAt(7); // Remove Item 8
        vwp.UpdateLayout();

        TestUtil.AssertItemRangeRealized(vwp, 1, 7);
        TestUtil.AssertItemNotRealized(vwp, "Item 8");
        TestUtil.AssertItemRangeRealized(vwp, 9, 41);
        TestUtil.AssertItem(vwp, "Item 9", 200, 100);
        Assert.Equal(40, vwp.Children.Count);
    }

    [UIFact]
    public void ItemMoved_InViewport()
    {
        var items = (ObservableCollection<TestItem>)vwp.ItemsControl.ItemsSource;

        items.Move(7, 8); // Move Item 8 after Item 9
        vwp.UpdateLayout();

        TestUtil.AssertItemRangeRealized(vwp, 1, 40);
        TestUtil.AssertItem(vwp, "Item 8", 300, 100);
        TestUtil.AssertItem(vwp, "Item 9", 200, 100);
        Assert.Equal(40, vwp.Children.Count);
    }

    [UIFact]
    public void ItemMoved_OutOfViewportAndCache()
    {
        var items = (ObservableCollection<TestItem>)vwp.ItemsControl.ItemsSource;

        items.Move(7, 99); // Move Item 8 after Item 100
        vwp.UpdateLayout();

        TestUtil.AssertItemRangeRealized(vwp, 1, 7);
        TestUtil.AssertItemNotRealized(vwp, "Item 8");
        TestUtil.AssertItemRangeRealized(vwp, 9, 41);
        TestUtil.AssertItem(vwp, "Item 9", 200, 100);
        Assert.Equal(40, vwp.Children.Count);
    }

    [UIFact]
    public void ItemsCleared()
    {
        var items = (ObservableCollection<TestItem>)vwp.ItemsControl.ItemsSource;

        items.Clear();
        vwp.UpdateLayout();

        Assert.Empty(vwp.Children);
        Assert.Equal(0, vwp.ExtentHeight);
    }

    [WpfFact]
    public void ThrowsExceptionWhenItemsAreNotDistinct()
    {
        var items = Enumerable.Repeat(new TestItem("TestItem", 100, 100), 100).ToList();

        Assert.Throws<InvalidOperationException>(() =>
        {
            vwp = TestUtil.CreateVirtualizingWrapPanel(500, 400, items);
            vwp.UpdateLayout();
        });
    }

    // TODO test items changes: add, replace, remove mutiple items

}

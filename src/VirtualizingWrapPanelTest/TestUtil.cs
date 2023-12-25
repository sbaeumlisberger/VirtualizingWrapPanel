using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using WpfToolkit.Controls;
using System.Xml;
using System.Windows.Markup;
using Assert = Xunit.Assert;

namespace VirtualizingWrapPanelTest;

public record class TestItem(string Name, int Width, int Height);

internal class TestUtil
{
    public static VirtualizingWrapPanel CreateVirtualizingWrapPanel(
        double width,
        double height,
        IList<TestItem>? items = null,
        int itemCount = 1000)
    {
        var itemsControl = new VirtualizingItemsControl();
        ScrollViewer.SetVerticalScrollBarVisibility(itemsControl, ScrollBarVisibility.Hidden);
        itemsControl.ItemsPanel = new ItemsPanelTemplate(new FrameworkElementFactory(typeof(VirtualizingWrapPanel)));
        itemsControl.ItemsSource = items ?? Enumerable.Range(1, itemCount).Select(x => new TestItem("Item " + x, 100, 100)).ToList();
        itemsControl.ItemTemplate = CreateDefaultItemTemplate();
        itemsControl.Width = width;
        itemsControl.Height = height;
        itemsControl.Background = new SolidColorBrush(Colors.Red);

        var window = new Window
        {
            Width = 0,
            Height = 0,
            WindowStyle = WindowStyle.None,
            ShowInTaskbar = false,
            ShowActivated = false,
            Content = itemsControl
        };
        window.Show();

        return GetVisualChild<VirtualizingWrapPanel>(itemsControl)!;
    }

    public static FrameworkElement AssertItemRealized(VirtualizingWrapPanel vwp, string itemName)
    {
        var itemContainer = FindItemContainer(vwp, itemName);
        Assert.True(itemContainer != null, $"{itemName} is not realized, but should be");
        return itemContainer;
    }

    public static void AssertItemNotRealized(VirtualizingWrapPanel vwp, string itemName)
    {
        var itemContainer = FindItemContainer(vwp, itemName);
        Assert.True(itemContainer == null, $"{itemName} is realized, but should not be");
    }

    public static void AssertItem(VirtualizingWrapPanel vwp, string itemName, int x, int y, int width = 100, int height = 100)
    {
        var itemContainer = AssertItemRealized(vwp, itemName);
        var position = itemContainer.TranslatePoint(new Point(0, 0), vwp);
        Assert.Equal(x, position.X);
        Assert.Equal(y, position.Y);
        Assert.Equal(width, itemContainer.ActualWidth);
        Assert.Equal(height, itemContainer.ActualHeight);
    }

    public static void AssertPanelPosition(VirtualizingWrapPanel vwp, int x, int y)
    {
        var position = vwp.TranslatePoint(new Point(0, 0), vwp.ItemsControl);
        Assert.Equal(x, position.X);
        Assert.Equal(y, position.Y);
    }

    public static DataTemplate CreateItemTemplate(string dataTemplate)
    {
        return (DataTemplate)XamlReader.Load(XmlReader.Create(new StringReader(dataTemplate)));
    }

    private static FrameworkElement? FindItemContainer(VirtualizingWrapPanel vwp, string itemName)
    {
        return vwp.Children.OfType<FrameworkElement>().Where(x => ((TestItem)x.DataContext).Name == itemName).SingleOrDefault();
    }

    private static DataTemplate CreateDefaultItemTemplate()
    {
        return CreateItemTemplate("""
            <DataTemplate xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"> 
                <Border Width="{Binding Width}" Height="{Binding Height}" Background="Blue">
                    <TextBlock Text="{Binding Name}"/>
                </Border>
            </DataTemplate>
            """);
    }

    private static T? GetVisualChild<T>(DependencyObject parent) where T : Visual
    {
        int childCount = VisualTreeHelper.GetChildrenCount(parent);

        for (int i = 0; i < childCount; i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);

            if (child is not T)
            {
                child = GetVisualChild<T>(child);
            }

            if (child is T result)
            {
                return result;
            }
        }
        return null;
    }

}


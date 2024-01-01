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
using System.Collections;

namespace VirtualizingWrapPanelTest;

public record class TestItem(string Name, int Width, int Height, string? Group = null);

internal class TestUtil
{
    public const int DefaultItemWidth = 100;
    public const int DefaultItemHeight = 100;

    public static VirtualizingWrapPanel CreateVirtualizingWrapPanel(
        double width,
        double height,
        IEnumerable? items = null,
        int itemCount = 1000)
    {
        var itemsControl = CreateVirtualizingItemsControl(width, height, items ?? GenerateItems(itemCount));
        return GetVisualChild<VirtualizingWrapPanel>(itemsControl)!;
    }

    public static ListBox CreateListBox(
        double width,
        double height,
        IEnumerable items)
    {
        var itemsControl = new ListBox();
        SetupAndShowItemsControl(itemsControl, width, height, items);
        return itemsControl;
    }

    public static VirtualizingItemsControl CreateVirtualizingItemsControl(
        double width,
        double height,
        IEnumerable items)
    {
        var itemsControl = new VirtualizingItemsControl();
        SetupAndShowItemsControl(itemsControl, width, height, items);
        return itemsControl;
    }

    private static void SetupAndShowItemsControl(
        ItemsControl itemsControl,
       double width,
       double height,
       IEnumerable items)
    {
        ScrollViewer.SetVerticalScrollBarVisibility(itemsControl, ScrollBarVisibility.Hidden);
        ScrollViewer.SetHorizontalScrollBarVisibility(itemsControl, ScrollBarVisibility.Hidden);

        itemsControl.ItemsPanel = new ItemsPanelTemplate(new FrameworkElementFactory(typeof(VirtualizingWrapPanel)));
        itemsControl.ItemsSource = items;
        itemsControl.ItemTemplate = CreateDefaultItemTemplate();
        itemsControl.Width = width;
        itemsControl.Height = height;
        itemsControl.BorderThickness = new Thickness(0);
        itemsControl.Padding = new Thickness(0);
        itemsControl.Background = new SolidColorBrush(Colors.Red);

        itemsControl.ItemContainerStyle = new Style()
        {
            Setters =
            {
                new Setter(Control.MarginProperty, new Thickness(0)),
                new Setter(Control.PaddingProperty, new Thickness(0)),
                new Setter(Control.BorderThicknessProperty, new Thickness(0))
            }
        };

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
    }

    public static List<TestItem> GenerateItems(int itemCount, int groupSize = 100)
    {
        return Enumerable.Range(1, itemCount).Select(i => new TestItem("Item " + i, DefaultItemWidth, DefaultItemHeight, "Group " + (i - 1) / groupSize)).ToList();
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

    public static void AssertItem(VirtualizingWrapPanel vwp, string itemName, int x, int y, int width = DefaultItemWidth, int height = DefaultItemHeight)
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

    public static DataTemplate CreateDateTemplate(string dataTemplate)
    {
        return (DataTemplate)XamlReader.Load(XmlReader.Create(new StringReader(dataTemplate)));
    }

    private static FrameworkElement? FindItemContainer(VirtualizingWrapPanel vwp, string itemName)
    {
        return vwp.Children.OfType<FrameworkElement>().Where(x => ((TestItem)x.DataContext).Name == itemName).SingleOrDefault();
    }

    private static DataTemplate CreateDefaultItemTemplate()
    {
        return CreateDateTemplate("""
            <DataTemplate xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"> 
                <Border Width="{Binding Width}" Height="{Binding Height}" Background="Blue">
                    <TextBlock Text="{Binding Name}"/>
                </Border>
            </DataTemplate>
            """);
    }

    public static T? GetVisualChild<T>(DependencyObject parent) where T : Visual
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

    public static List<T> GetVisualChilds<T>(DependencyObject parent) where T : Visual
    {
        var foundChilds = new List<T>();

        int childCount = VisualTreeHelper.GetChildrenCount(parent);

        for (int i = 0; i < childCount; i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);

            if (child is T result)
            {
                foundChilds.Add(result);
            }
            else
            {
                foundChilds.AddRange(GetVisualChilds<T>(child));
            }
        }

        return foundChilds;
    }

}


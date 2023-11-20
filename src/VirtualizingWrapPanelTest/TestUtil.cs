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

public record class Item(string Name, int Width, int Height);

internal class TestUtil
{
    public static VirtualizingWrapPanel CreateVirtualizingWrapPanel(
        double width, 
        double height,
        IList<Item>? items = null,
        int itemCount = 1000)
    {
        var itemsControl = new VirtualizingItemsControl();
        itemsControl.ItemsPanel = new ItemsPanelTemplate(new FrameworkElementFactory(typeof(VirtualizingWrapPanel)));
        itemsControl.ItemsSource = items ?? Enumerable.Range(1, itemCount).Select(x => new Item("Item " + x, 100, 100)).ToList();
        itemsControl.ItemTemplate = CreateItemTemplate();
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

    public static void AssertItemPosition(VirtualizingWrapPanel vwp, string item, int x, int y)
    {
        var itemContainer = Assert.Single(vwp.Children.OfType<FrameworkElement>().Where(x => ((Item)x.DataContext).Name == item));
        var position = itemContainer.TranslatePoint(new Point(0, 0), vwp);
        Assert.Equal(x, position.X);
        Assert.Equal(y, position.Y);
    }

    public static void AssertPanelPosition(VirtualizingWrapPanel vwp, int x, int y) 
    {
        var position = vwp.TranslatePoint(new Point(0, 0), vwp.ItemsControl);
        Assert.Equal(x, position.X);
        Assert.Equal(y, position.Y);
    }

    private static DataTemplate CreateItemTemplate()
    {
        var stringReader = new StringReader("""
            <DataTemplate xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"> 
                <Border Width="{Binding Width}" Height="{Binding Height}" Background="Blue">
                    <TextBlock Text="{Binding Name}"/>
                </Border>
            </DataTemplate>
            """);
        var xmlReader = XmlReader.Create(stringReader);
        return (DataTemplate)XamlReader.Load(xmlReader);
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


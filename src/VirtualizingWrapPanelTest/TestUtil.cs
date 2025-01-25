using System.Collections;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Xml;
using WpfToolkit.Controls;
using Assert = Xunit.Assert;

namespace VirtualizingWrapPanelTest;

public record class TestItem(string Name, int Width, int Height, string? Group = null);

internal class TestUtil
{
    public const int DefaultItemWidth = 100;
    public const int DefaultItemHeight = 100;

    public static VirtualizingWrapPanel CreateVirtualizingWrapPanel(
        double width,
        double height,
        int itemCount = 1000)
    {
        return CreateVirtualizingWrapPanel(width, height, GenerateItems(itemCount));
    }

    public static VirtualizingWrapPanel CreateVirtualizingWrapPanel(
        double width,
        double height,
        IEnumerable items)
    {
        var itemsControl = CreateVirtualizingItemsControl(width, height, items);
        return GetVisualChild<VirtualizingWrapPanel>(itemsControl)!;
    }

    public static ListBox CreateListBox(
        double width,
        double height,
        IEnumerable items)
    {
        var listBox = new ListBox();
        SetupAndShowItemsControl(listBox, width, height, items);
        return listBox;
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

    public static ObservableCollection<TestItem> GenerateItems(int itemCount, int groupSize = 100)
    {
        return new ObservableCollection<TestItem>(Enumerable.Range(1, itemCount)
            .Select(i => new TestItem("Item " + i, DefaultItemWidth, DefaultItemHeight, "Group " + ((i - 1) / groupSize + 1))));
    }

    public static ObservableCollection<TestItem> GenerateItemsWithRandomGroupSizes(int itemCount, int minGroupSize = 50, int maxGroupSize = 150)
    {
        int currentGroupNumber = 1;
        int groupSize = Random.Shared.Next(minGroupSize, maxGroupSize + 1);
        int getGroupNumber()
        {
            int groupNumber = currentGroupNumber;
            if (--groupSize == 0)
            {
                groupSize = Random.Shared.Next(minGroupSize, maxGroupSize + 1);
                currentGroupNumber++;
            }
            return groupNumber;
        }
        return new ObservableCollection<TestItem>(Enumerable.Range(1, itemCount)
            .Select(i => new TestItem("Item " + i, DefaultItemWidth, DefaultItemHeight, "Group " + getGroupNumber())));
    }

    public static FrameworkElement AssertItemRealized(VirtualizingPanel virtualizingPanel, string itemName)
    {
        var itemContainer = FindItemContainer(virtualizingPanel, itemName);
        Assert.True(itemContainer != null, $"{itemName} is not realized, but should be");
        return itemContainer;
    }

    public static void AssertItemRangeRealized(VirtualizingPanel vwp, int start, int end)
    {
        for (int i = start; i <= end; i++)
        {
            AssertItemRealized(vwp, "Item " + i);
        }
    }

    public static void AssertItemNotRealized(VirtualizingPanel vwp, string itemName)
    {
        var itemContainer = FindItemContainer(vwp, itemName);
        Assert.True(itemContainer == null, $"{itemName} is realized, but should not be");
    }

    public static void AssertItem(VirtualizingPanel virtualizingPanel, string itemName, int x, int y, int width = DefaultItemWidth, int height = DefaultItemHeight)
    {
        var itemContainer = AssertItemRealized(virtualizingPanel, itemName);
        var position = itemContainer.TranslatePoint(new Point(0, 0), virtualizingPanel);
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

    private static FrameworkElement? FindItemContainer(VirtualizingPanel virtualizingPanel, string itemName)
    {
        return (FrameworkElement?)GetVisualChild(virtualizingPanel,
            child => child is FrameworkElement fe && fe.DataContext is TestItem testItem && testItem.Name == itemName);
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

    public static DependencyObject? GetVisualChild(DependencyObject parent, Func<DependencyObject, bool> condition)
    {
        int childCount = VisualTreeHelper.GetChildrenCount(parent);

        for (int i = 0; i < childCount; i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);

            if (condition(child))
            {
                return child;
            }

            if (GetVisualChild(child, condition) is { } childRecursive)
            {
                return childRecursive;
            }
        }
        return null;
    }

    public static T? GetVisualChild<T>(DependencyObject parent) where T : DependencyObject
    {
        int childCount = VisualTreeHelper.GetChildrenCount(parent);

        for (int i = 0; i < childCount; i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);

            if (child is T result)
            {
                return result;
            }

            if (GetVisualChild<T>(child) is T childRecursive)
            {
                return childRecursive;
            }
        }
        return null;
    }

    public static List<T> GetVisualChilds<T>(DependencyObject parent) where T : DependencyObject
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


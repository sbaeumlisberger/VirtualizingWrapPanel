using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Xml;
using WpfToolkit.Controls;
using Assert = Xunit.Assert;

namespace VirtualizingWrapPanelTest;

public record class TestItem(string Name, int Width, int Height, string? Group = null);

public static class TestUtil
{
    public static readonly bool Debug = Debugger.IsAttached;

    public const int DefaultItemWidth = 100;
    public const int DefaultItemHeight = 100;

    public static VirtualizingWrapPanel CreateVirtualizingWrapPanel(
        double width,
        double height,
        int itemCount = 1000,
        Window? window = null)
    {
        return CreateVirtualizingWrapPanel(width, height, GenerateItems(itemCount), window);
    }

    public static VirtualizingWrapPanel CreateVirtualizingWrapPanel(
        double width,
        double height,
        IEnumerable items,
        Window? window = null)
    {
        var itemsControl = CreateVirtualizingItemsControl(width, height, items, window);
        return GetVisualChild<VirtualizingWrapPanel>(itemsControl)!;
    }

    public static ListBox CreateListBox(
        double width,
        double height,
        IEnumerable items,
        Window? window = null)
    {
        var listBox = new ListBox();
        SetupAndShowItemsControl(listBox, width, height, items, window);
        return listBox;
    }

    public static VirtualizingItemsControl CreateVirtualizingItemsControl(
        double width,
        double height,
        IEnumerable items,
        Window? window = null)
    {
        var itemsControl = new VirtualizingItemsControl();
        SetupAndShowItemsControl(itemsControl, width, height, items, window);
        return itemsControl;
    }

    private static void SetupAndShowItemsControl(
        ItemsControl itemsControl,
        double width,
        double height,
        IEnumerable items,
        Window? window = null)
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
        itemsControl.Background = new SolidColorBrush(Colors.LightGray);

        itemsControl.ItemContainerStyle = new Style()
        {
            Setters =
            {
                new Setter(Control.MarginProperty, new Thickness(0)),
                new Setter(Control.PaddingProperty, new Thickness(0)),
                new Setter(Control.BorderThicknessProperty, new Thickness(0))
            }
        };

        window ??= new Window();

        window.Content = itemsControl;

        if (!Debug)
        {
            window.Width = 0;
            window.Height = 0;
            window.WindowStyle = WindowStyle.None;
            window.ShowInTaskbar = false;
            window.ShowActivated = false;
        }

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
        Assert.Equal(x, (int)Math.Round(position.X));
        Assert.Equal(y, (int)Math.Round(position.Y));
        Assert.Equal(width, (int)Math.Round(itemContainer.ActualWidth));
        Assert.Equal(height, (int)Math.Round(itemContainer.ActualHeight));
    }

    public static void AssertPanelPosition(VirtualizingWrapPanel vwp, int x, int y)
    {
        var position = vwp.TranslatePoint(new Point(0, 0), vwp.ItemsControl);
        Assert.Equal(x, (int)Math.Round(position.X));
        Assert.Equal(y, (int)Math.Round(position.Y));
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

    public static List<FrameworkElement> FindItemContainers(VirtualizingPanel virtualizingPanel)
    {
        return GetVisualChilds<ListBoxItem>(virtualizingPanel).Cast<FrameworkElement>().ToList();
    }

    private static DataTemplate CreateDefaultItemTemplate()
    {
        return CreateDateTemplate("""
            <DataTemplate xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"> 
                <Grid Width="{Binding Width}" Height="{Binding Height}">
                    <Border Background="Red" Opacity="0.5" BorderBrush="Blue" BorderThickness="1"/>
                    <TextBlock VerticalAlignment="Bottom" Text="{Binding Name}"/>
                </Grid>
            </DataTemplate>
            """);
    }

    public static DataTemplate CreateDefaultGroupHeaderTemplate(int headerHeight = 20) 
    {
        return CreateDateTemplate($"""
            <DataTemplate xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"> 
                <Border Background="Green" Height="{headerHeight}"/>
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


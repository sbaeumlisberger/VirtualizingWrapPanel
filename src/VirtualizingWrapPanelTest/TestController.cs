using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using WpfToolkit.Controls;

namespace VirtualizingWrapPanelTest;

public class TestController
{
    public const int DefaultItemsControlWidth = 500;
    public const int DefaultItemsControlHeight = 400;

    public const int DefaulTestItemWidth = 100;
    public const int DefaulTestItemHeight = 100;

    public const int DefaultGroupHeaderHeight = 20;

    public static readonly bool Debug = Debugger.IsAttached;

    public Style DefaultItemContainerStyle { get; } = new()
    {
        Setters =
        {
            new Setter(Control.MarginProperty, new Thickness(0)),
            new Setter(Control.PaddingProperty, new Thickness(0)),
            new Setter(Control.BorderThicknessProperty, new Thickness(0)),
            new Setter(Control.HorizontalContentAlignmentProperty, HorizontalAlignment.Stretch),
            new Setter(Control.VerticalContentAlignmentProperty, VerticalAlignment.Stretch)
        }
    };

    public IList<TestItem> Items { get; private set; }

    public double ItemsControlWidth { get; }
    public double ItemsControlHeight { get; }

    public double FirstChildWidth => VirtualizingWrapPanel.Children[0].DesiredSize.Width;
    public double FirstChildHeight => VirtualizingWrapPanel.Children[0].DesiredSize.Height;

    public double ViewportWidth => ScrollInfo.ViewportWidth;
    public double ViewportHeight => ScrollInfo.ViewportHeight;

    public double HorizontalOffset => ScrollInfo.HorizontalOffset;
    public double VerticalOffset => ScrollInfo.VerticalOffset;

    public double ExtentWidth => ScrollInfo.ExtentWidth;
    public double ExtentHeight => ScrollInfo.ExtentHeight;

    public Size DesiredSize => VirtualizingWrapPanel.DesiredSize;

    public List<FrameworkElement> Children => VirtualizingWrapPanel.Children.Cast<FrameworkElement>().ToList();

    public TestItem? FocusedItem => (TestItem?)((FrameworkElement)Keyboard.FocusedElement)?.DataContext;

    public int ItemsPerRow => (int)Math.Floor(ViewportWidth / (double)FirstChildWidth);
    public int ItemsPerPage => ItemsPerRow * (int)Math.Floor(ViewportHeight / (double)FirstChildHeight);

    public IReadOnlyList<Size> ItemSizesCache => VirtualizingWrapPanel.ItemSizesCache;

    private IScrollInfo ScrollInfo => (IScrollInfo)TestUtil.GetVisualChild<VirtualizingPanel>(itemsControl)!;
    private VirtualizingPanel VirtualizingPanel => TestUtil.GetVisualChild<VirtualizingPanel>(itemsControl)!;
    private VirtualizingWrapPanel VirtualizingWrapPanel => TestUtil.GetVisualChild<VirtualizingWrapPanel>(itemsControl)!;

    private readonly Window window = new Window();

    private readonly ItemsControl itemsControl;

    private readonly TestViewModel viewModel = new TestViewModel();

    private ICollectionView collectionView;

    private TestController(ItemsControl itemsControl, IList<TestItem> items, double width, double height)
    {
        this.itemsControl = itemsControl;
        Items = items;
        collectionView = CollectionViewSource.GetDefaultView(Items);
        ItemsControlWidth = width;
        ItemsControlHeight = height;
        SetupAndShowItemsControl();
        itemsControl.UpdateLayout();
    }

    public static TestController CreateListBoxWithVirtualizingWrapPanel()
    {
        return new TestController(new ListBox(), GenerateItems(), DefaultItemsControlWidth, DefaultItemsControlHeight);
    }

    public static TestController CreateListBoxWithVirtualizingWrapPanel(
        IList<TestItem> items,
        double width = DefaultItemsControlWidth,
        double height = DefaultItemsControlHeight)
    {
        return new TestController(new ListBox(), items, width, height);
    }

    public static ObservableCollection<TestItem> GenerateItems(int itemCount = 10_000, int groupSize = 100)
    {
        return new ObservableCollection<TestItem>(Enumerable.Range(1, itemCount)
            .Select(i => new TestItem("Item " + i, DefaulTestItemWidth, DefaulTestItemHeight, "Group " + ((i - 1) / groupSize + 1))));
    }

    public static ObservableCollection<TestItem> GenerateItemsWithRandomGroupSizes(int itemCount = 10_000, int minGroupSize = 50, int maxGroupSize = 150)
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
            .Select(i => new TestItem("Item " + i, DefaulTestItemWidth, DefaulTestItemHeight, "Group " + getGroupNumber())));
    }

    public static ObservableCollection<TestItem> GenerateDifferentSizedItems(int itemCount = 10_000, int groupSize = 100)
    {
        return new ObservableCollection<TestItem>(Enumerable.Range(1, itemCount)
            .Select(i => new TestItem("Item " + i, Random.Shared.Next(101) + 50, Random.Shared.Next(101) + 50, "Group " + ((i - 1) / groupSize + 1))));
    }

    public async Task SetItemsSourceAsync(IList<TestItem> items)
    {
        Items = items;
        collectionView = CollectionViewSource.GetDefaultView(Items);
        itemsControl.ItemsSource = collectionView;
        await UpdateLayoutAsync();
    }

    public async Task ScrollLineDownAsync()
    {
        ScrollInfo.LineDown();
        await UpdateLayoutAsync();
    }

    public async Task ScrollLineUpAsync()
    {
        ScrollInfo.LineUp();
        await UpdateLayoutAsync();
    }

    public async Task ScrollPageDownAsync()
    {
        ScrollInfo.PageDown();
        await UpdateLayoutAsync();
    }

    public async Task ScrollPageUpAsync()
    {
        ScrollInfo.PageUp();
        await UpdateLayoutAsync();
    }

    public async Task ScrollPageRightAsync()
    {
        ScrollInfo.PageRight();
        await UpdateLayoutAsync();
    }

    public async Task ScrollMouseWheelDownAsync()
    {
        ScrollInfo.MouseWheelDown();
        await UpdateLayoutAsync();
    }

    public async Task ScrollMouseWheelUpAsync()
    {
        ScrollInfo.MouseWheelUp();
        await UpdateLayoutAsync();
    }

    public async Task SetHorizontalOffsetAsync(double horizontalOffset)
    {
        ScrollInfo.SetHorizontalOffset(horizontalOffset);
        await UpdateLayoutAsync();
    }

    public async Task SetVerticalOffsetAsync(double verticalOffset)
    {
        ScrollInfo.SetVerticalOffset(verticalOffset);
        await UpdateLayoutAsync();
    }

    public async Task SendKeyEventAsync(Key key)
    {
        if (Keyboard.FocusedElement is not UIElement focusedElement || !itemsControl.IsAncestorOf(focusedElement))
        {
            if (!itemsControl.Focus()) throw new Exception($"Failed to focus ItemsControl");
        }
        var keyEventArgs = new KeyEventArgs(Keyboard.PrimaryDevice, PresentationSource.FromVisual(itemsControl), 0, key)
        {
            RoutedEvent = Keyboard.KeyDownEvent
        };
        InputManager.Current.ProcessInput(keyEventArgs);
        await UpdateLayoutAsync();
    }

    public async Task ScrollToEndAsync()
    {
        while (VerticalOffset < ExtentHeight - ViewportHeight)
        {
            ScrollInfo.SetVerticalOffset(ExtentHeight - ViewportHeight);
            await UpdateLayoutAsync();
        }
    }

    public async Task ScrollIntoViewAsync(TestItem item)
    {
        if (itemsControl is not ListBox listBox)
        {
            throw new InvalidOperationException("ItemsControl is not a ListBox");
        }
        listBox.ScrollIntoView(item);
        await UpdateLayoutAsync();
    }

    public async Task BringIndexIntoViewAsync(int itemIndex)
    {
        VirtualizingPanel.BringIndexIntoViewPublic(itemIndex);
        await UpdateLayoutAsync();
    }

    public async Task<Rect> MakeVisibleAsync(Visual visual, Rect rectangle)
    {
        Rect visibleRect = ScrollInfo.MakeVisible(visual, rectangle);
        await UpdateLayoutAsync();
        return visibleRect;
    }

    public async Task ResizePanelAsync(double width, double height)
    {
        if (itemsControl is ListBox listBox)
        {
            // Adjust for the internal border of the ListBox
            width += 2;
            height += 2;
        }
        itemsControl.Width = width;
        itemsControl.Height = height;
        await UpdateLayoutAsync();
    }

    public async Task SetHorizontalAlignmentAsync(HorizontalAlignment horizontalAlignment)
    {
        VirtualizingWrapPanel.HorizontalAlignment = horizontalAlignment;
        await UpdateLayoutAsync();
    }

    public async Task SetSpacingModeAsync(SpacingMode spacingMode)
    {
        VirtualizingWrapPanel.SpacingMode = spacingMode;
        await UpdateLayoutAsync();
    }

    public async Task SetStretchItemsAsync(bool stretchItems)
    {
        VirtualizingWrapPanel.StretchItems = stretchItems;
        await UpdateLayoutAsync();
    }

    public async Task SetItemContainerStyleAsync(Style style)
    {
        itemsControl.ItemContainerStyle = style;
        await UpdateLayoutAsync();
    }

    public async Task SetVirtualizationModeAsync(VirtualizationMode virtualizationMode)
    {
        VirtualizingPanel.SetVirtualizationMode(itemsControl, virtualizationMode);
        await UpdateLayoutAsync();
    }

    public async Task SetItemSizeAsync(Size itemSize)
    {
        VirtualizingWrapPanel.ItemSize = itemSize;
        await UpdateLayoutAsync();
    }
    public async Task SetItemTemplateAsync(string itemTemplate)
    {
        itemsControl.ItemTemplate = TestUtil.CreateDataTemplate(itemTemplate);
        await UpdateLayoutAsync();
    }

    public async Task SetItemTemplateAsync(DataTemplate itemTemplate)
    {
        itemsControl.ItemTemplate = itemTemplate;
        await UpdateLayoutAsync();
    }

    public async Task SetAllowDifferentSizedItemsAsync(bool allowDifferentSizedItems)
    {
        VirtualizingWrapPanel.AllowDifferentSizedItems = allowDifferentSizedItems;
        await UpdateLayoutAsync();
    }

    public async Task SetGroupingAsync(bool grouping)
    {
        if (grouping)
        {
            collectionView.GroupDescriptions.Add(new PropertyGroupDescription(nameof(TestItem.Group)));

            var panelFactory = new FrameworkElementFactory(typeof(VirtualizingStackPanel));
            panelFactory.SetBinding(VirtualizingStackPanel.OrientationProperty, new Binding
            {
                Source = viewModel,
                Path = new PropertyPath(nameof(viewModel.OrientationParent)),
                Mode = BindingMode.OneWay
            });

            itemsControl.GroupStyle.Add(new GroupStyle()
            {
                HeaderTemplate = TestUtil.CreateDefaultGroupHeaderTemplate(DefaultGroupHeaderHeight),
                Panel = new ItemsPanelTemplate(panelFactory)
            });
        }
        else
        {
            collectionView.GroupDescriptions.Clear();
            itemsControl.GroupStyle.Clear();
        }

        await UpdateLayoutAsync();
    }

    public async Task SetOrientationAsync(Orientation orientation)
    {
        viewModel.Orientation = orientation;
        viewModel.OrientationParent = orientation == Orientation.Horizontal ? Orientation.Vertical : Orientation.Horizontal;
    }

    public async Task SetScrollUnitAsync(ScrollUnit scrollUnit)
    {
        VirtualizingPanel.SetScrollUnit(itemsControl, scrollUnit);
        await UpdateLayoutAsync();
    }

    public async Task SetVerticalScrollBarVisibility(ScrollBarVisibility visibility)
    {
        ScrollViewer.SetVerticalScrollBarVisibility(itemsControl, visibility);
        await UpdateLayoutAsync();
    }

    public async Task SetHorizontalScrollBarVisibility(ScrollBarVisibility visibility)
    {
        ScrollViewer.SetHorizontalScrollBarVisibility(itemsControl, visibility);
        await UpdateLayoutAsync();
    }

    public async Task SetCacheLengthAsync(VirtualizationCacheLength cacheLength)
    {
        VirtualizingPanel.SetCacheLength(itemsControl, cacheLength);
        await UpdateLayoutAsync();
    }

    public async Task SetCacheLengthUnitAsync(VirtualizationCacheLengthUnit cacheLengthUnit)
    {
        VirtualizingPanel.SetCacheLengthUnit(itemsControl, cacheLengthUnit);
        await UpdateLayoutAsync();
    }

    public FrameworkElement? GetContainerForItem(TestItem item)
    {
        return (FrameworkElement?)itemsControl.ItemContainerGenerator.ContainerFromItem(item);
    }

    public async Task FocusItemAsync(TestItem item)
    {
        var container = GetContainerForItem(item);
        if (container is null)
        {
            throw new Exception($"Item {item.Name} is not realized");
        }

        bool wasFocused = container.Focus();
        if (!wasFocused)
        {
            throw new Exception($"Failed to focus item {item.Name}");
        }

        await UpdateLayoutAsync();
    }

    public Point GetVirtualizingWrapPanelPosition()
    {
        var position = VirtualizingWrapPanel.TranslatePoint(new Point(0, 0), itemsControl);
        return itemsControl is ListBox ? new Point(position.X - 1, position.Y - 1) : position;
    }

    public Point GetContainerPosition(string itemName)
    {
        return GetContainerBounds(Items.First(item => item.Name == itemName)).TopLeft;
    }

    public Rect GetContainerBounds(string itemName)
    {
        return GetContainerBounds(Items.First(item => item.Name == itemName));
    }

    /// <summary>
    /// Gets the bounds of the container of the specified item relative to the viewport or <see cref="Rect.Empty"/> if the item is not realized.
    /// </summary>
    public Rect GetContainerBounds(TestItem item)
    {
        var itemContainer = GetContainerForItem(item);

        if (itemContainer == null)
        {
            return Rect.Empty;
        }

        var offset = itemContainer.TranslatePoint(new Point(0, 0), VirtualizingPanel);

        return new Rect(offset.X, offset.Y, itemContainer.ActualWidth, itemContainer.ActualHeight);
    }

    public List<TestItem> GetItemsInViewport()
    {
        return [.. Items.Where(item =>
        {
            var containerBounds = GetContainerBounds(item);
            return !containerBounds.IsEmpty
                && containerBounds.Bottom > 0
                && containerBounds.Top < ViewportHeight;
        })];
    }

    public async Task UpdateLayoutAsync()
    {
        itemsControl.UpdateLayout();
        await window.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.Render);
    }

    private void SetupAndShowItemsControl()
    {
        ScrollViewer.SetVerticalScrollBarVisibility(itemsControl, ScrollBarVisibility.Hidden);
        ScrollViewer.SetHorizontalScrollBarVisibility(itemsControl, ScrollBarVisibility.Hidden);

        VirtualizingPanel.SetIsVirtualizingWhenGrouping(itemsControl, true);

        var itemsPanelFactory = new FrameworkElementFactory(typeof(VirtualizingWrapPanel));
        itemsPanelFactory.SetBinding(VirtualizingWrapPanel.OrientationProperty, new Binding
        {
            Source = viewModel,
            Path = new PropertyPath(nameof(viewModel.Orientation)),
            Mode = BindingMode.OneWay
        });

        itemsControl.ItemsPanel = new ItemsPanelTemplate(itemsPanelFactory);
        itemsControl.ItemsSource = collectionView;
        itemsControl.ItemTemplate = CreateDefaulTestItemTemplate();
        itemsControl.Width = ItemsControlWidth;
        itemsControl.Height = ItemsControlHeight;
        itemsControl.Padding = new Thickness(0);
        itemsControl.Margin = new Thickness(0);
        itemsControl.BorderThickness = new Thickness(0);
        itemsControl.Background = new SolidColorBrush(Colors.White);

        if (itemsControl is ListBox listBox)
        {
            // Adjust for the internal border of the ListBox
            listBox.Width += 2;
            listBox.Height += 2;
        }

        itemsControl.ItemContainerStyle = DefaultItemContainerStyle;

        window.Content = itemsControl;
        window.Background = new SolidColorBrush(Colors.Black);

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

    private static DataTemplate CreateDefaulTestItemTemplate()
    {
        return TestUtil.CreateDataTemplate("""
            <DataTemplate xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"> 
                <Grid MinWidth="{Binding Width}" MinHeight="{Binding Height}">
                    <Border Background="Red" Opacity="0.5" BorderBrush="Blue" BorderThickness="1"/>
                    <TextBlock HorizontalAlignment="Center" VerticalAlignment="Center" Text="{Binding Name}"/>
                </Grid>
            </DataTemplate>
            """);
    }
}

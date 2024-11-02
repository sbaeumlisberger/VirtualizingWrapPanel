using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using WpfToolkit.Controls;

namespace VirtualizingWrapPanelSamples
{
    class MainWindowModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<TestItem> Items { get; } = new ObservableCollection<TestItem>();

        public int RenderedItemsCount { get => renderedItemsCount; set => SetField(ref renderedItemsCount, value); }

        public long MemoryUsageInMB { get => memoryUsageInMB; private set => SetField(ref memoryUsageInMB, value); }
        public bool IsAutoRefreshMemoryUsageEnabled { get => isAutoRefreshMemoryUsageEnabled; set => SetField(ref isAutoRefreshMemoryUsageEnabled, value); }

        public VirtualizationCacheLengthUnit[] AvailableCacheUnits { get; } = (VirtualizationCacheLengthUnit[])Enum.GetValues(typeof(VirtualizationCacheLengthUnit));
        public VirtualizationMode[] AvailableVirtualizationModes { get; } = (VirtualizationMode[])Enum.GetValues(typeof(VirtualizationMode));
        public Orientation[] AvailableOrientations { get; } = (Orientation[])Enum.GetValues(typeof(Orientation));
        public SpacingMode[] AvailableSpacingModes { get; } = (SpacingMode[])Enum.GetValues(typeof(SpacingMode));
        public ScrollUnit[] AvailableScrollUnits { get; } = (ScrollUnit[])Enum.GetValues(typeof(ScrollUnit));
        public ScrollBarVisibility[] AvailableScrollBarVisibilities { get; } = (ScrollBarVisibility[])Enum.GetValues(typeof(ScrollBarVisibility));

        public Orientation Orientation { get => orientation; set => SetField(ref orientation, value); }
        public Orientation OrientationGroupPanel { get => orientationGroupPanel; set => SetField(ref orientationGroupPanel, value); }
        public VirtualizationCacheLengthUnit CacheUnit { get => cacheUnit; set => SetField(ref cacheUnit, value); }
        public VirtualizationCacheLength CacheLength { get => cacheLength; set => SetField(ref cacheLength, value); }
        public VirtualizationMode VirtualizationMode { get => virtualizationMode; set => SetField(ref virtualizationMode, value); }
        public SpacingMode SpacingMode { get => spacingMode; set => SetField(ref spacingMode, value); }
        public bool StretchItems { get => stretchItems; set => SetField(ref stretchItems, value); }
        public ScrollUnit ScrollUnit { get => scrollUnit; set => SetField(ref scrollUnit, value); }
        public bool IsScrollByPixel => ScrollUnit == ScrollUnit.Pixel;
        public bool IsScrollByItem => ScrollUnit == ScrollUnit.Item;
        public double ScrollLineDelta { get => scrollLineDelta; set => SetField(ref scrollLineDelta, value); }
        public double MouseWheelDelta { get => mouseWheelDelta; set => SetField(ref mouseWheelDelta, value); }
        public int ScrollLineDeltaItem { get => scrollLineDeltaItem; set => SetField(ref scrollLineDeltaItem, value); }
        public int MouseWheelDeltaItem { get => mouseWheelDeltaItem; set => SetField(ref mouseWheelDeltaItem, value); }
        public ScrollBarVisibility HorizontalScrollBarVisibility { get => horizontalScrollBarVisibility; set => SetField(ref horizontalScrollBarVisibility, value); }
        public ScrollBarVisibility VerticalScrollBarVisibility { get => verticalScrollBarVisibility; set => SetField(ref verticalScrollBarVisibility, value); }
        public Size ItemSize { get => itemSize; set => SetField(ref itemSize, value); }
        public bool IsGrouping { get => isGrouping; set => SetField(ref isGrouping, value); }
        public bool IsGridLayoutEnabled { get => isGridLayoutEnabled; set => SetField(ref isGridLayoutEnabled, value); }
        public bool UseLazyLoadingItems { get => useLazyLoadingItems; set => SetField(ref useLazyLoadingItems, value); }
        public bool UseItemSizeProvider { get => useItemSizeProvider; set => SetField(ref useItemSizeProvider, value); }

        public bool IsWrappingKeyboardNavigationEnabled { get => isWrappingKeyboardNavigationEnabled; set => SetField(ref isWrappingKeyboardNavigationEnabled, value); }

        public IItemSizeProvider ItemSizeProvider { get; } = new TestItemSizeProvider();

        private int renderedItemsCount = 0;

        private long memoryUsageInMB = 0;
        private bool isAutoRefreshMemoryUsageEnabled = false;

        private VirtualizationCacheLengthUnit cacheUnit = VirtualizationCacheLengthUnit.Page;
        private VirtualizationCacheLength cacheLength = new VirtualizationCacheLength(1);
        private VirtualizationMode virtualizationMode = VirtualizationMode.Recycling;
        private Orientation orientation = Orientation.Horizontal;
        private Orientation orientationGroupPanel = Orientation.Vertical;
        private SpacingMode spacingMode = SpacingMode.Uniform;
        private bool stretchItems = false;
        private ScrollUnit scrollUnit = ScrollUnit.Pixel;
        private double scrollLineDelta = 16.0;
        private double mouseWheelDelta = 48.0;
        private int scrollLineDeltaItem = 1;
        private int mouseWheelDeltaItem = 3;
        private ScrollBarVisibility horizontalScrollBarVisibility = ScrollBarVisibility.Auto;
        private ScrollBarVisibility verticalScrollBarVisibility = ScrollBarVisibility.Auto;
        private Size itemSize = Size.Empty;
        private bool isGrouping = false;
        private bool isGridLayoutEnabled = true;
        private bool useLazyLoadingItems = false;
        private bool useItemSizeProvider = false;

        private bool isWrappingKeyboardNavigationEnabled = false;

        private readonly Random random = new Random();

        private readonly DispatcherTimer memoryUsageRefreshTimer;

        public ICollectionView CollectionView { get; }

        public MainWindowModel()
        {
            AddItems();
            memoryUsageRefreshTimer = new DispatcherTimer()
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            memoryUsageRefreshTimer.Tick += (s, a) => RefreshMemoryUsage();
            PropertyChanged += MainWindowModel_PropertyChanged;

            CollectionView = CollectionViewSource.GetDefaultView(Items);
        }

        public void InsertItemAtRandomPosition()
        {
            int number = Items.Any() ? Items.Select(item => item.Number).Max() + 1 : 1;
            Items.Add(new TestItem("Group " + random.Next(50), number));
        }

        public void AddItems()
        {
            int newCount = Items.Count + 5000;
            for (int i = Items.Count; i < newCount; i++)
            {
                Items.Add(new TestItem("Group " + i / 100, i + 1));
            }
        }

        public void RemoveRandomItem()
        {
            if (Items.Any())
            {
                int index = random.Next(Items.Count);
                Items.RemoveAt(index);
            }
        }

        public void RemoveItem(TestItem item)
        {
            Items.Remove(item);
        }

        public void RemoveAllItems()
        {
            Items.Clear();
        }

        public void RandomizeItems()
        {
            Items.Clear();
            int count = random.Next(500, 5000);
            for (int i = 0; i < count; i++)
            {
                Items.Add(new TestItem("Group " + i / 100, i + 1));
            }
        }

        public void RefreshMemoryUsage()
        {
            GC.GetTotalMemory(true);
            using (Process process = Process.GetCurrentProcess())
            {
                MemoryUsageInMB = process.PrivateMemorySize64 / (1024 * 1024);
            }
        }

        private void MainWindowModel_PropertyChanged(object? sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(CacheUnit):
                    UpdateCacheLength();
                    break;
                case nameof(IsAutoRefreshMemoryUsageEnabled):
                    UpdateMemoryUsageRefreshTimer();
                    break;
                case nameof(ScrollUnit):
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsScrollByPixel)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsScrollByItem)));
                    break;
                case nameof(Orientation):
                    OrientationGroupPanel = Orientation == Orientation.Horizontal ? Orientation.Vertical : Orientation.Horizontal;
                    break;
            }
        }

        private void UpdateCacheLength()
        {
            switch (CacheUnit)
            {
                case VirtualizationCacheLengthUnit.Item:
                    CacheLength = new VirtualizationCacheLength(10, 10);
                    break;
                case VirtualizationCacheLengthUnit.Page:
                    CacheLength = new VirtualizationCacheLength(1, 1);
                    break;
                case VirtualizationCacheLengthUnit.Pixel:
                    CacheLength = new VirtualizationCacheLength(100, 100);
                    break;
            }
        }

        private void UpdateMemoryUsageRefreshTimer()
        {
            if (IsAutoRefreshMemoryUsageEnabled)
            {
                memoryUsageRefreshTimer.Start();
            }
            else
            {
                memoryUsageRefreshTimer.Stop();
            }
        }

        private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(value, field))
            {
                return false;
            }
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }
    }
}

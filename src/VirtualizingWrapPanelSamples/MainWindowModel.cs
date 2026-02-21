using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using WpfToolkit.Controls;

namespace VirtualizingWrapPanelSamples
{
    class MainWindowModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<TestItem> Items { get; private set => SetProperty(ref field, value); } = new ObservableCollection<TestItem>();
        public ICollectionView CollectionView { get; private set => SetProperty(ref field, value); }

        public int RenderedItemsCount { get; set => SetProperty(ref field, value); } = 0;

        public long MemoryUsageInMB { get; private set => SetProperty(ref field, value); } = 0;
        public bool IsAutoRefreshMemoryUsageEnabled { get; set => SetProperty(ref field, value); } = false;

        public VirtualizationCacheLengthUnit[] AvailableCacheUnits { get; } = (VirtualizationCacheLengthUnit[])Enum.GetValues(typeof(VirtualizationCacheLengthUnit));
        public VirtualizationMode[] AvailableVirtualizationModes { get; } = (VirtualizationMode[])Enum.GetValues(typeof(VirtualizationMode));
        public Orientation[] AvailableOrientations { get; } = (Orientation[])Enum.GetValues(typeof(Orientation));
        public SpacingMode[] AvailableSpacingModes { get; } = (SpacingMode[])Enum.GetValues(typeof(SpacingMode));
        public ItemAlignment[] AvailableItemAlignments { get; } = (ItemAlignment[])Enum.GetValues(typeof(ItemAlignment));
        public ScrollUnit[] AvailableScrollUnits { get; } = (ScrollUnit[])Enum.GetValues(typeof(ScrollUnit));
        public ScrollBarVisibility[] AvailableScrollBarVisibilities { get; } = (ScrollBarVisibility[])Enum.GetValues(typeof(ScrollBarVisibility));

        public Orientation Orientation { get; set => SetProperty(ref field, value); } = Orientation.Horizontal;
        public Orientation OrientationGroupPanel { get; set => SetProperty(ref field, value); } = Orientation.Vertical;
        public VirtualizationCacheLengthUnit CacheUnit { get; set => SetProperty(ref field, value); } = VirtualizationCacheLengthUnit.Page;
        public VirtualizationCacheLength CacheLength { get; set => SetProperty(ref field, value); } = new VirtualizationCacheLength(1);
        public VirtualizationMode VirtualizationMode { get; set => SetProperty(ref field, value); } = VirtualizationMode.Recycling;
        public SpacingMode SpacingMode { get; set => SetProperty(ref field, value); } = SpacingMode.Uniform;
        public bool StretchItems { get; set => SetProperty(ref field, value); } = false;
        public ScrollUnit ScrollUnit { get; set => SetProperty(ref field, value); } = ScrollUnit.Pixel;
        public bool IsScrollByPixel => ScrollUnit == ScrollUnit.Pixel;
        public bool IsScrollByItem => ScrollUnit == ScrollUnit.Item;
        public double ScrollLineDelta { get; set => SetProperty(ref field, value); } = 16.0;
        public double MouseWheelDelta { get; set => SetProperty(ref field, value); } = 48.0;
        public int ScrollLineDeltaItem { get; set => SetProperty(ref field, value); } = 1;
        public int MouseWheelDeltaItem { get; set => SetProperty(ref field, value); } = 3;
        public ScrollBarVisibility HorizontalScrollBarVisibility { get; set => SetProperty(ref field, value); } = ScrollBarVisibility.Auto;
        public ScrollBarVisibility VerticalScrollBarVisibility { get; set => SetProperty(ref field, value); } = ScrollBarVisibility.Auto;
        public Size ItemSize { get; set => SetProperty(ref field, value); } = Size.Empty;
        public bool IsGrouping { get; set => SetProperty(ref field, value); } = false;
        public bool IsGridLayoutEnabled { get; set => SetProperty(ref field, value); } = true;
        public bool UseLazyLoadingItems { get; set => SetProperty(ref field, value); } = false;
        public bool UseItemSizeProvider { get; set => SetProperty(ref field, value); } = false;
        public ItemAlignment ItemAlignment { get; set => SetProperty(ref field, value); } = ItemAlignment.Start;

        public bool IsWrappingKeyboardNavigationEnabled { get; set => SetProperty(ref field, value); } = false;

        public IItemSizeProvider ItemSizeProvider { get; } = new TestItemSizeProvider();

        public ICommand AddItemCommand => field ??= new SimpleCommand(parameter => AddItems(int.Parse((string)parameter!)));

        private readonly DispatcherTimer memoryUsageRefreshTimer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(1) };

        public MainWindowModel()
        {
            memoryUsageRefreshTimer.Tick += (s, a) => RefreshMemoryUsage();
            PropertyChanged += MainWindowModel_PropertyChanged;
            CollectionView = CollectionViewSource.GetDefaultView(Items);
            AddItems(100_000);
        }

        public void AddItem()
        {
            int number = Items.Any() ? (Items.Select(item => item.Number).Max() + 1) : 1;
            Items.Add(new TestItem((number - 1) % 100 + 1, number));
        }

        public void AddItems(int count)
        {
            if (Items.Count == 0)
            {
                Items = new ObservableCollection<TestItem>();
            }

            int number = Items.Any() ? (Items.Select(item => item.Number).Max() + 1) : 1;
            int newCount = Items.Count + count;
            for (int i = Items.Count; i < newCount; i++)
            {
                Items.Add(new TestItem((number - 1) % 100 + 1, number));
                number++;
            }

            CollectionView = CollectionViewSource.GetDefaultView(Items);
        }

        public void RemoveItem(TestItem item)
        {
            Items.Remove(item);
        }

        public void RemoveAllItems()
        {
            Items.Clear();
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
                case nameof(IsGrouping):
                    UpdateCollectionViewGrouping();
                    break;
            }
        }

        private void UpdateCollectionViewGrouping()
        {
            if (IsGrouping)
            {
                CollectionView.GroupDescriptions.Add(new PropertyGroupDescription(nameof(TestItem.Group)));
            }
            else
            {
                CollectionView.GroupDescriptions.Clear();
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

        private bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
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

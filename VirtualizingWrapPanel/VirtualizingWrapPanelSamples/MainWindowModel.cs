using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using WpfToolkit.Controls;

namespace VirtualizingWrapPanelSamples {

    class MainWindowModel : INotifyPropertyChanged {

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<TestItem> Items { get; } = new ObservableCollection<TestItem>();

        public int RenderedItemsCount { get => renderedItemsCount; set => SetField(ref renderedItemsCount, value); }

        public long MemoryUsageInMB { get => memoryUsageInMB; private set => SetField(ref memoryUsageInMB, value); }
        public bool IsAutoRefreshMemoryUsageEnabled { get => isAutoRefreshMemoryUsageEnabled; set => SetField(ref isAutoRefreshMemoryUsageEnabled, value); }
        
        public Orientation[] AvailableOrientations { get; } = (Orientation[])Enum.GetValues(typeof(Orientation));
        public VirtualizationCacheLengthUnit[] AvailableCacheUnits { get; } = (VirtualizationCacheLengthUnit[])Enum.GetValues(typeof(VirtualizationCacheLengthUnit));
        public ScrollUnit[] AvailableScrollUnits { get; } = (ScrollUnit[])Enum.GetValues(typeof(ScrollUnit));
        public VirtualizationMode[] AvailableVirtualizationModes { get; } = (VirtualizationMode[]) Enum.GetValues(typeof(VirtualizationMode));
        public SpacingMode[] AvailableSpacingModes { get; } = (SpacingMode[])Enum.GetValues(typeof(SpacingMode));

        public Orientation Orientation {
            get => orientation;
            set {
                SetField(ref orientation, value);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(OrientationInverted)));
            }
        }
        public Orientation OrientationInverted => Orientation == Orientation.Vertical ? Orientation.Horizontal : Orientation.Vertical;

        public VirtualizationCacheLengthUnit CacheUnit { get => cacheUnit; set => SetField(ref cacheUnit, value); }
        public VirtualizationCacheLength CacheLength { get => cacheLength; set => SetField(ref cacheLength, value); }
        public ScrollUnit ScrollUnit { get => scrollUnit; set => SetField(ref scrollUnit, value); }
        public VirtualizationMode VirtualizationMode { get => virtualizationMode; set => SetField(ref virtualizationMode, value); }
        public SpacingMode SpacingMode { get => spacingMode; set => SetField(ref spacingMode, value); }

        private int renderedItemsCount = 0;

        private long memoryUsageInMB = 0;
        private bool isAutoRefreshMemoryUsageEnabled = false;

        private Orientation orientation = Orientation.Vertical;
        private VirtualizationCacheLengthUnit cacheUnit = VirtualizationCacheLengthUnit.Page;
        private VirtualizationCacheLength cacheLength = new VirtualizationCacheLength(1);
        private ScrollUnit scrollUnit = ScrollUnit.Pixel;
        private VirtualizationMode virtualizationMode = VirtualizationMode.Standard;
        private SpacingMode spacingMode = SpacingMode.Uniform;

        private readonly Random random = new Random();

        private readonly DispatcherTimer memoryUsageRefreshTimer;

        public ICollectionView CollectionView { get; }

        public MainWindowModel() {
            AddItems();
            memoryUsageRefreshTimer = new DispatcherTimer() {
                Interval = TimeSpan.FromSeconds(1)
            };
            memoryUsageRefreshTimer.Tick += (s, a) => RefreshMemoryUsage();
            PropertyChanged += MainWindowModel_PropertyChanged;

            CollectionView = CollectionViewSource.GetDefaultView(Items);
        }

        public void InsertItemAtRandomPosition() {
            int index = random.Next(Items.Count);
            Items.Insert(index, new TestItem("Group " + new Random().Next(250), Items.Count));
        }

        public void AddItems() {
            int newCount = Items.Count + 5000;
            for (int i = Items.Count; i < newCount; i++) {
                Items.Add(new TestItem("Group " + i/20, i + 1));
            }
        }

        public void RemoveRandomItem() {
            if (Items.Any()) {
                int index = random.Next(Items.Count);
                Items.RemoveAt(index);
            }
        }

        public void RemoveAllItems() {
            Items.Clear();
        }

        public void RefreshMemoryUsage() {
            GC.GetTotalMemory(true);
            using (Process process = Process.GetCurrentProcess()) {
                MemoryUsageInMB = process.PrivateMemorySize64 / (1024 * 1024);
            }
        }

        private void MainWindowModel_PropertyChanged(object sender, PropertyChangedEventArgs args) {
            switch (args.PropertyName) {
                case nameof(CacheUnit):
                    UpdateCacheLength();
                    break;
                case nameof(IsAutoRefreshMemoryUsageEnabled):
                    UpdateMemoryUsageRefreshTimer();
                    break;
            }
        }

        private void UpdateCacheLength() {
            switch (CacheUnit) {
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

        private void UpdateMemoryUsageRefreshTimer() {
            if (IsAutoRefreshMemoryUsageEnabled) {
                memoryUsageRefreshTimer.Start();
            }
            else {
                memoryUsageRefreshTimer.Stop();
            }
        }

        private bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null) {
            if (Equals(value, field)) {
                return false;
            }
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }

    }

}
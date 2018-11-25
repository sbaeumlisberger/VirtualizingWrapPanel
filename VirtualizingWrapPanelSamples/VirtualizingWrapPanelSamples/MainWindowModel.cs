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
using System.Windows.Threading;

namespace VirtualizingWrapPanelSamples {

    class MainWindowModel : INotifyPropertyChanged {

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<TestItem> Items { get; } = new ObservableCollection<TestItem>();

        public int RenderedItemsCount { get => renderedItemsCount; set => SetField(ref renderedItemsCount, value); }

        public long MemoryUsageInMB { get => memoryUsageInMB; private set => SetField(ref memoryUsageInMB, value); }
        public bool IsAutoRefreshMemoryUsageEnabled { get => isAutoRefreshMemoryUsageEnabled; set => SetField(ref isAutoRefreshMemoryUsageEnabled, value); }

        public VirtualizationCacheLengthUnit[] AvailableCacheUnits { get; } = (VirtualizationCacheLengthUnit[])Enum.GetValues(typeof(VirtualizationCacheLengthUnit));
        public ScrollUnit[] AvailableScrollUnits { get; } = (ScrollUnit[])Enum.GetValues(typeof(ScrollUnit));

        public VirtualizationCacheLengthUnit CacheUnit { get => cacheUnit; set => SetField(ref cacheUnit, value); }
        public VirtualizationCacheLength CacheLength { get => cacheLength; set => SetField(ref cacheLength, value); }
        public ScrollUnit ScrollUnit { get => scrollUnit; set => SetField(ref scrollUnit, value); }

        private int renderedItemsCount = 0;

        private long memoryUsageInMB = 0;
        private bool isAutoRefreshMemoryUsageEnabled = false;

        private VirtualizationCacheLengthUnit cacheUnit = VirtualizationCacheLengthUnit.Item;
        private VirtualizationCacheLength cacheLength = new VirtualizationCacheLength(10);
        private ScrollUnit scrollUnit = ScrollUnit.Item;

        private readonly Random random = new Random();

        private readonly DispatcherTimer memoryUsageRefreshTimer;

        public MainWindowModel() {
            AddItems();
            memoryUsageRefreshTimer = new DispatcherTimer() {
                Interval = TimeSpan.FromSeconds(1)
            };
            memoryUsageRefreshTimer.Tick += (s, a) => RefreshMemoryUsage();
            PropertyChanged += MainWindowModel_PropertyChanged;
        }

        public void InsertItemAtRandomPosition() {
            int index = random.Next(Items.Count);
            Items.Insert(index, new TestItem(Items.Count));
        }

        public void AddItems() {
            int newCount = Items.Count + 5000;
            for (int i = Items.Count; i < newCount; i++) {
                Items.Add(new TestItem(i + 1));
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
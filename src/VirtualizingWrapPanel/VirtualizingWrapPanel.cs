using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;

namespace WpfToolkit.Controls
{
    /// <summary>
    /// A implementation of a wrap panel that supports virtualization and can be used in horizontal and vertical orientation.
    /// </summary>
    public class VirtualizingWrapPanel : VirtualizingPanelBase
    {
        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(
            nameof(Orientation),
            typeof(Orientation),
            typeof(VirtualizingWrapPanel),
            new FrameworkPropertyMetadata(
                Orientation.Horizontal,
                FrameworkPropertyMetadataOptions.AffectsMeasure,
                (obj, args) => ((VirtualizingWrapPanel)obj).Orientation_Changed()));

        public static readonly DependencyProperty ItemSizeProperty = DependencyProperty.Register(
            nameof(ItemSize),
            typeof(Size),
            typeof(VirtualizingWrapPanel),
            new FrameworkPropertyMetadata(
                Size.Empty,
                FrameworkPropertyMetadataOptions.AffectsMeasure,
                (obj, args) => ((VirtualizingWrapPanel)obj).ItemSize_Changed()));

        public static readonly DependencyProperty AllowDifferentSizedItemsProperty = DependencyProperty.Register(
            nameof(AllowDifferentSizedItems),
            typeof(bool), typeof(VirtualizingWrapPanel),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.AffectsMeasure,
                (obj, args) => ((VirtualizingWrapPanel)obj).AllowDifferentSizedItems_Changed()));

        public static readonly DependencyProperty ItemSizeProviderProperty = DependencyProperty.Register(
            nameof(ItemSizeProvider),
            typeof(IItemSizeProvider),
            typeof(VirtualizingWrapPanel),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.AffectsMeasure,
                (obj, args) => ((VirtualizingWrapPanel)obj).ItemSizeProvider_Changed()));

        public static readonly DependencyProperty ItemAlignmentProperty = DependencyProperty.Register(
            nameof(ItemAlignment),
            typeof(ItemAlignment),
            typeof(VirtualizingWrapPanel),
            new FrameworkPropertyMetadata(ItemAlignment.Start, FrameworkPropertyMetadataOptions.AffectsArrange));

        public static readonly DependencyProperty SpacingModeProperty = DependencyProperty.Register(
            nameof(SpacingMode),
            typeof(SpacingMode),
            typeof(VirtualizingWrapPanel),
            new FrameworkPropertyMetadata(SpacingMode.Uniform, FrameworkPropertyMetadataOptions.AffectsArrange));

        public static readonly DependencyProperty StretchItemsProperty = DependencyProperty.Register(
            nameof(StretchItems),
            typeof(bool),
            typeof(VirtualizingWrapPanel),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsArrange));

        public static readonly DependencyProperty IsGridLayoutEnabledProperty = DependencyProperty.Register(
            nameof(IsGridLayoutEnabled),
            typeof(bool),
            typeof(VirtualizingWrapPanel),
            new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsArrange));

        public static readonly DependencyProperty FallbackItemSizeProperty = DependencyProperty.Register(
            nameof(FallbackItemSize),
            typeof(Size),
            typeof(VirtualizingWrapPanel),
            new FrameworkPropertyMetadata(
                new Size(64, 64), 
                FrameworkPropertyMetadataOptions.AffectsMeasure,
                (obj, args) => ((VirtualizingWrapPanel)obj).FallbackItemSize_Changed()));

        /// <summary>
        /// Gets or sets a value that specifies the orientation in which items are arranged before wrapping. The default value is <see cref="Orientation.Horizontal"/>.
        /// </summary>
        public Orientation Orientation { get => (Orientation)GetValue(OrientationProperty); set => SetValue(OrientationProperty, value); }

        /// <summary>
        /// Gets or sets a value that specifies the size of the items. The default value is <see cref="Size.Empty"/>. 
        /// If the value is <see cref="Size.Empty"/> the item size is determined by measuring the first realized item.
        /// </summary>
        public Size ItemSize { get => (Size)GetValue(ItemSizeProperty); set => SetValue(ItemSizeProperty, value); }

        /// <summary>
        /// Specifies whether items can have different sizes. The default value is false. If this property is enabled, 
        /// it is strongly recommended to also set the <see cref="ItemSizeProvider"/> property. Otherwise, the position 
        /// of the items is not always guaranteed to be correct.
        /// </summary>
        public bool AllowDifferentSizedItems { get => (bool)GetValue(AllowDifferentSizedItemsProperty); set => SetValue(AllowDifferentSizedItemsProperty, value); }

        /// <summary>
        /// Specifies an instance of <see cref="IItemSizeProvider"/> which provides the size of the items. In order to allow
        /// different sized items, also enable the <see cref="AllowDifferentSizedItems"/> property.
        /// </summary>
        public IItemSizeProvider? ItemSizeProvider { get => (IItemSizeProvider?)GetValue(ItemSizeProviderProperty); set => SetValue(ItemSizeProviderProperty, value); }

        /// <summary>
        /// Specifies how the item are aligned on the cross axis. The default value is <see cref="ItemAlignment.Start"/>.
        /// This property only applies when the <see cref="AllowDifferentSizedItems"/> property is enabled.
        /// </summary>
        public ItemAlignment ItemAlignment { get => (ItemAlignment)GetValue(ItemAlignmentProperty); set => SetValue(ItemAlignmentProperty, value); }

        /// <summary>
        /// Gets or sets the spacing mode used when arranging the items. The default value is <see cref="SpacingMode.Uniform"/>.
        /// </summary>
        public SpacingMode SpacingMode { get => (SpacingMode)GetValue(SpacingModeProperty); set => SetValue(SpacingModeProperty, value); }

        /// <summary>
        /// Gets or sets a value that specifies if the items get stretched to fill up remaining space. The default value is false.
        /// </summary>
        /// <remarks>
        /// The MaxWidth and MaxHeight properties of the ItemContainerStyle can be used to limit the stretching. 
        /// In this case the use of the remaining space will be determined by the SpacingMode property. 
        /// </remarks>
        public bool StretchItems { get => (bool)GetValue(StretchItemsProperty); set => SetValue(StretchItemsProperty, value); }

        /// <summary>
        /// Specifies whether the items are arranged in a grid-like layout. The default value is <c>true</c>.
        /// When set to <c>true</c>, the items are arranged based on the number of items that can fit in a line. 
        /// When set to <c>false</c>, the items are arranged based on the number of items that are actually placed in the line. 
        /// </summary>
        /// <remarks>
        /// If <see cref="AllowDifferentSizedItems"/> is enabled, this property has no effect and the items are always 
        /// arranged based on the number of items that are actually placed in the line.
        /// </remarks>
        public bool IsGridLayoutEnabled { get => (bool)GetValue(IsGridLayoutEnabledProperty); set => SetValue(IsGridLayoutEnabledProperty, value); }

        /// <summary>
        /// Assumed size for not realized items used when <see cref="AllowDifferentSizedItems"></see> is enabled 
        /// and no <see cref="ItemSizeProvider"/> is set. The default value is 64 x 64.
        /// </summary>
        public Size FallbackItemSize { get => (Size)GetValue(FallbackItemSizeProperty); set => SetValue(FallbackItemSizeProperty, value); }

        /// <summary>
        /// Gets value that indicates whether the <see cref="VirtualizingPanel"/> can virtualize items 
        /// that are grouped or organized in a hierarchy.
        /// </summary>
        /// <returns>always true for <see cref="VirtualizingWrapPanel"/></returns>
        protected override bool CanHierarchicallyScrollAndVirtualizeCore => true;

        private static readonly Size InfiniteSize = new Size(double.PositiveInfinity, double.PositiveInfinity);

        private ItemContainerManager ItemContainerManager
        {
            get
            {
                if (_itemContainerManager is null)
                {
                    _itemContainerManager = new ItemContainerManager(
                        ItemContainerGenerator,
                        InternalChildren.Contains,
                        AddInternalChild,
                        child => RemoveInternalChildRange(InternalChildren.IndexOf(child), 1));
                    _itemContainerManager.ItemsChanged += ItemContainerManager_ItemsChanged;
                }
                return _itemContainerManager;
            }
        }
        private ItemContainerManager? _itemContainerManager;

        private double ViewportSizeMainAxis => GetSizeOnMainAxis(ViewportSize);
        private double ViewportSizeCrossAxis => GetSizeOnCrossAxis(ViewportSize);
        private double ExtentMainAxis => GetSizeOnMainAxis(Extent);
        private double ExtentCrossAxis => GetSizeOnCrossAxis(Extent);
        private double ScrollOffsetMainAxis => GetPositionOnMainAxis(ScrollOffset);
        private double ScrollOffsetCrossAxis => GetPositionOnCrossAxis(ScrollOffset);

        /// <summary>
        /// The cache length before and after the viewport. 
        /// </summary>
        private VirtualizationCacheLength cacheLength;

        /// <summary>
        /// The Unit of the cache length. Can be Pixel, Item or Page. 
        /// When the ItemsOwner is a group item it can only be pixel or item.
        /// </summary>
        private VirtualizationCacheLengthUnit cacheLengthUnit;

        private Size sizeOfFirstItem = Size.Empty;

        private List<Size> itemSizesCache = [];

        private int startItemIndex = -1;
        private int endItemIndex = -1;

        private double startItemOffsetMainAxis = 0;
        private double startItemOffsetCrossAxis = 0;

        private int bringIntoViewItemIndex = -1;
        private FrameworkElement? bringIntoViewContainer;

        private UIElement? keptRealizedFocusedContainer;

        #region cache for frequently read dependency properties
        private Orientation orientation = Orientation.Horizontal;
        private Size itemSize = Size.Empty;
        private IItemSizeProvider? itemSizeProvider;
        private ReadOnlyCollection<object> items = new ReadOnlyCollection<object>([]);
        private Size fallbackItemSize = new Size(64, 64);
        #endregion

        public void ClearItemSizeCache()
        {
            itemSizesCache = AllowDifferentSizedItems ? Utils.NewUninitializedList<Size>(Items.Count) : [];
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            VerifyItemsControl();

            items = Items;

            /* When grouping gets enabled, the items panel defined in the GroupStyle may not 
             * be used immediately. In this case the itemms are of type CollectionViewGroup. 
             * To ensure the items panel is updated and  to prevent incorrect visualization, 
             * a desired size of 0 x 0 is returned and no further processing happens. */
            if (items.FirstOrDefault() is CollectionViewGroup)
            {
                return new Size(0, 0);
            }

            ItemContainerManager.IsRecycling = IsRecycling;

            MeasureBringIntoViewContainer();

            UpdateViewportSize(availableSize);
            RealizeAndVirtualizeItems();
            UpdateExtent();

            const double Tolerance = 0.001;
            if (ItemsOwner is not IHierarchicalVirtualizationAndScrollInfo
                && ScrollOffsetCrossAxis != 0
                && ScrollOffsetCrossAxis + ViewportSizeCrossAxis > ExtentCrossAxis + Tolerance)
            {
                ScrollOffset = CreatePoint(ScrollOffsetMainAxis, Math.Max(0, ExtentCrossAxis - ViewportSizeCrossAxis));
                ScrollOwner?.InvalidateScrollInfo();
                RealizeAndVirtualizeItems();
            }

            DisconnectRecycledContainers();

            Size desiredSize = CalculateDesiredSize(availableSize);

            return desiredSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            ViewportSize = finalSize;

            ArrangeBringIntoViewContainer();
            ArrangeKeptRealizedFocusedContainer();

            if (startItemIndex == -1)
            {
                return finalSize;
            }

            if (ItemContainerManager.RealizedContainers.Count < endItemIndex - startItemIndex + 1)
            {
                throw new InvalidOperationException("The items must be distinct and must not change their hash code.");
            }

            bool hierarchical = ItemsOwner is IHierarchicalVirtualizationAndScrollInfo;
            double offsetMainAxis = startItemOffsetMainAxis + GetPositionOnMainAxis(ScrollOffset);
            double offsetCrossAxis = hierarchical ? startItemOffsetCrossAxis : startItemOffsetCrossAxis - GetPositionOnCrossAxis(ScrollOffset);
            double lineSizeCrossAxis = 0;
            var rowChilds = new List<UIElement>();
            var childSizes = new List<Size>();

            var items = Items; // local variable for performance
            for (int i = startItemIndex; i <= endItemIndex; i++)
            {
                var item = items[i];
                var child = ItemContainerManager.RealizedContainers[item];

                Size upfrontKnownItemSize = GetUpfrontKnownItemSizeOrEmpty(item);

                Size childSize = !upfrontKnownItemSize.IsEmpty ? upfrontKnownItemSize : child.DesiredSize;

                if (rowChilds.Count > 0 && offsetMainAxis + GetSizeOnMainAxis(childSize) > GetSizeOnMainAxis(finalSize))
                {
                    ArrangeLine(GetSizeOnMainAxis(finalSize), rowChilds, childSizes, offsetCrossAxis, hierarchical);
                    offsetMainAxis = 0;
                    offsetCrossAxis += lineSizeCrossAxis;
                    lineSizeCrossAxis = 0;
                    rowChilds.Clear();
                    childSizes.Clear();
                }

                offsetMainAxis += GetSizeOnMainAxis(childSize);
                lineSizeCrossAxis = Math.Max(lineSizeCrossAxis, GetSizeOnCrossAxis(childSize));
                rowChilds.Add(child);
                childSizes.Add(childSize);
            }

            if (rowChilds.Count > 0)
            {
                ArrangeLine(GetSizeOnMainAxis(finalSize), rowChilds, childSizes, offsetCrossAxis, hierarchical);
            }

            return finalSize;
        }

        protected override void BringIndexIntoView(int index)
        {
            if (index < 0 || index >= Items.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index), $"The argument {nameof(index)} must be >= 0 and < the count of items.");
            }

            var container = (FrameworkElement)ItemContainerManager.Realize(index);

            bringIntoViewItemIndex = index;
            bringIntoViewContainer = container;

            // make sure the container is measured and arranged before calling BringIntoView        
            InvalidateMeasure();
            UpdateLayout();

            container.BringIntoView();
        }

        protected override void OnClearChildren()
        {
            RemoveInternalChildRange(0, InternalChildren.Count);
            ItemContainerManager.OnClearChildren();
            base.OnClearChildren();
        }

        private void ItemContainerManager_ItemsChanged(object? sender, ItemsChangedEventArgs e)
        {
            items = Items;

            if (bringIntoViewItemIndex >= Items.Count)
            {
                bringIntoViewItemIndex = -1;
                bringIntoViewContainer = null;
            }

            if (AllowDifferentSizedItems)
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Remove:
                        int removeIndex = ItemContainerManager.IndexFromGeneratorPosition(e.Position);
                        itemSizesCache.RemoveAt(removeIndex);
                        break;
                    case NotifyCollectionChangedAction.Replace:
                        int replaceIndex = ItemContainerManager.IndexFromGeneratorPosition(e.Position);
                        itemSizesCache[replaceIndex] = default;
                        break;
                    case NotifyCollectionChangedAction.Add:
                        int addIndex = ItemContainerManager.IndexFromGeneratorPosition(e.Position);
                        itemSizesCache.Insert(addIndex, default);
                        break;
                    case NotifyCollectionChangedAction.Move:
                        int oldIndex = ItemContainerManager.IndexFromGeneratorPosition(e.OldPosition);
                        int newIndex = ItemContainerManager.IndexFromGeneratorPosition(e.Position);
                        var itemSize = itemSizesCache[oldIndex];
                        itemSizesCache.RemoveAt(oldIndex);
                        itemSizesCache.Insert(newIndex, itemSize);
                        break;
                    case NotifyCollectionChangedAction.Reset:
                        itemSizesCache = Utils.NewUninitializedList<Size>(Items.Count);
                        if (ItemSizeProvider is null)
                        {
                            ScrollOffset = new Point(0, 0);
                        }
                        break;
                    default:
                        throw new NotSupportedException("The action " + e.Action + " is not supported.");
                }
            }
        }

        private void Orientation_Changed()
        {
            orientation = Orientation;

            MouseWheelScrollDirection = Orientation == Orientation.Horizontal
                                        ? ScrollDirection.Vertical
                                        : ScrollDirection.Horizontal;

            SetVerticalOffset(0);
            SetHorizontalOffset(0);
        }

        private void AllowDifferentSizedItems_Changed()
        {
            itemSizesCache = AllowDifferentSizedItems ? Utils.NewUninitializedList<Size>(Items.Count) : [];

            var children = InternalChildren;
            for (int i = 0; i < children.Count; i++)
            {
                children[i].InvalidateMeasure();
            }
        }

        private void ItemSizeProvider_Changed()
        {
            itemSizeProvider = ItemSizeProvider;
        }

        private void ItemSize_Changed()
        {
            itemSize = ItemSize;

            var children = InternalChildren;
            for (int i = 0; i < children.Count; i++)
            {
                children[i].InvalidateMeasure();
            }
        }

        private void FallbackItemSize_Changed()
        { 
            fallbackItemSize = FallbackItemSize;
        }

        private Rect GetViewportFromGroupItem(IHierarchicalVirtualizationAndScrollInfo groupItem)
        {
            double viewportX = groupItem.Constraints.Viewport.Location.X;
            double viewportY = groupItem.Constraints.Viewport.Location.Y;
            double viewportWidth = Math.Max(groupItem.Constraints.Viewport.Size.Width, 0);
            double viewportHeight = Math.Max(groupItem.Constraints.Viewport.Size.Height, 0);

            if (VisualTreeHelper.GetParent(this) is ItemsPresenter itemsPresenter)
            {
                var margin = itemsPresenter.Margin;

                if (orientation == Orientation.Horizontal)
                {
                    viewportWidth = Math.Max(0, viewportWidth - (margin.Left + margin.Right));
                }
                else
                {
                    viewportHeight = Math.Max(0, viewportHeight - (margin.Top + margin.Bottom));
                }
            }

            if (orientation == Orientation.Horizontal)
            {
                viewportY = Math.Max(0, viewportY - groupItem.HeaderDesiredSizes.PixelSize.Height);
                double visibleHeaderHeight = Math.Max(0, groupItem.HeaderDesiredSizes.PixelSize.Height - Math.Max(0, groupItem.Constraints.Viewport.Location.Y));
                viewportHeight = Math.Max(0, viewportHeight - visibleHeaderHeight);
            }
            else
            {
                viewportHeight = Math.Max(0, viewportHeight - groupItem.HeaderDesiredSizes.PixelSize.Height);
            }

            return new Rect(viewportX, viewportY, viewportWidth, viewportHeight);
        }

        private void MeasureBringIntoViewContainer()
        {
            if (bringIntoViewContainer is not null && !bringIntoViewContainer.IsMeasureValid)
            {
                var upfrontKnownItemSize = GetUpfrontKnownItemSizeOrEmpty(Items[bringIntoViewItemIndex]);
                bringIntoViewContainer.Measure(!upfrontKnownItemSize.IsEmpty ? upfrontKnownItemSize : InfiniteSize);

                if (!AllowDifferentSizedItems && sizeOfFirstItem.IsEmpty)
                {
                    sizeOfFirstItem = bringIntoViewContainer.DesiredSize;
                }
            }
        }

        private void ArrangeBringIntoViewContainer()
        {
            if (bringIntoViewContainer is not null)
            {
                bool hierarchical = ItemsOwner is IHierarchicalVirtualizationAndScrollInfo;
                var offset = FindItemOffset(bringIntoViewItemIndex);
                offset = new Point(offset.X - ScrollOffset.X, hierarchical ? offset.Y : offset.Y - ScrollOffset.Y);
                var upfrontKnownItemSize = GetUpfrontKnownItemSizeOrEmpty(Items[bringIntoViewItemIndex]);
                var size = !upfrontKnownItemSize.IsEmpty ? upfrontKnownItemSize : bringIntoViewContainer.DesiredSize;
                bringIntoViewContainer.Arrange(new Rect(offset, size));
            }
        }
                
        private void ArrangeKeptRealizedFocusedContainer() 
        {
            if (keptRealizedFocusedContainer is not null)
            {
                int index = ItemContainerGenerator.IndexFromContainer(keptRealizedFocusedContainer);

                if (index < startItemIndex)
                {
                    keptRealizedFocusedContainer.Arrange(new Rect(
                        new Point(-keptRealizedFocusedContainer.DesiredSize.Width, -keptRealizedFocusedContainer.DesiredSize.Height), 
                        keptRealizedFocusedContainer.DesiredSize));
                }
                else
                {
                    keptRealizedFocusedContainer.Arrange(new Rect(
                        new Point(ViewportWidth, ViewportHeight), 
                        keptRealizedFocusedContainer.DesiredSize));
                }
            }
        }

        private void RealizeAndVirtualizeItems()
        {
            if (AllowDifferentSizedItems)
            {
                RealizeAndVirtualizeItems_DifferentSizedItems();
            }
            else
            {
                RealizeAndVirtualizeItems_UniformSizedItems();
            }
        }

        private void RealizeAndVirtualizeItems_DifferentSizedItems()
        {
            if(itemSizesCache.Count != Items.Count) // panel is resued in grouping scenario
            {
                itemSizesCache = Utils.NewUninitializedList<Size>(Items.Count);
            }

            FindStartIndexAndOffset_DifferentSizedItems();
            VirtualizeItems();
            RealizeItemsAndFindEndIndex_DifferentSizedItems();
            VirtualizeItems();
        }

        private void RealizeAndVirtualizeItems_UniformSizedItems()
        {
            CalculateItemRange_UniformSizedItems();
            VirtualizeItems();
            RealizeItems();
        }

        private void CalculateItemRange_UniformSizedItems()
        {
            if (Items.Count == 0 || ViewportSizeMainAxis == 0)
            {
                startItemIndex = -1;
                endItemIndex = -1;
                startItemOffsetCrossAxis = 0;
                startItemOffsetMainAxis = 0;
                return;
            }

            Size uniformItemSize = !itemSize.IsEmpty ? itemSize : sizeOfFirstItem;

            if (uniformItemSize.IsEmpty)
            {
                var itemContainer = ItemContainerManager.Realize(0);
                itemContainer.Measure(InfiniteSize);
                sizeOfFirstItem = itemContainer.DesiredSize;
                uniformItemSize = sizeOfFirstItem;
            }

            double itemSizeOnMainAxis = GetSizeOnMainAxis(uniformItemSize);
            double itemSizeOnCrossAxis = GetSizeOnCrossAxis(uniformItemSize);

            int itemsPerLine = Math.Max((int)Math.Floor(ViewportSizeMainAxis / itemSizeOnMainAxis), 1);

            double startPositionOnCrossAxis = CalculateStartOffsetCrossAxis();
            double endPositonOnCrossAxis = CalculateEndOffsetCrossAxis();

            int startLine = (int)Math.Floor(startPositionOnCrossAxis / itemSizeOnCrossAxis);
            startItemIndex = startLine * itemsPerLine;

            int lineAfterEnd = (int)Math.Ceiling(endPositonOnCrossAxis / itemSizeOnCrossAxis);
            endItemIndex = Math.Clamp(lineAfterEnd * itemsPerLine - 1, 0, Items.Count - 1);

            if (cacheLengthUnit == VirtualizationCacheLengthUnit.Item)
            {
                startItemIndex = Math.Max(startItemIndex - (int)cacheLength.CacheBeforeViewport, 0);
                endItemIndex = Math.Min(endItemIndex + (int)cacheLength.CacheAfterViewport, Items.Count - 1);
            }

            startItemOffsetMainAxis = (startItemIndex % itemsPerLine) * itemSizeOnMainAxis;
            startItemOffsetCrossAxis = (startItemIndex / itemsPerLine) * itemSizeOnCrossAxis;
        }

        private void RealizeItems()
        {
            if (startItemIndex == -1)
            {
                return;
            }

            for (int itemIndex = startItemIndex; itemIndex <= endItemIndex; itemIndex++)
            {
                if (itemIndex == 0)
                {
                    sizeOfFirstItem = Size.Empty;
                }

                var container = ItemContainerManager.Realize(itemIndex);

                if (container == bringIntoViewContainer)
                {
                    bringIntoViewItemIndex = -1;
                    bringIntoViewContainer = null;
                }

                if (!container.IsMeasureValid)
                {
                    container.Measure(!itemSize.IsEmpty ? itemSize : (!sizeOfFirstItem.IsEmpty ? sizeOfFirstItem : InfiniteSize));
                }

                if (itemIndex == 0)
                {
                    sizeOfFirstItem = container.DesiredSize;
                }
            }
        }

        private Size GetItemSizeForScrolling()
        {
            if (!itemSize.IsEmpty)
            {
                return itemSize;
            }
            else if (!AllowDifferentSizedItems)
            {
                return !sizeOfFirstItem.IsEmpty ? sizeOfFirstItem : fallbackItemSize;
            }
            else
            {
                return CalculateAverageItemSize();
            }
        }

        private Point FindItemOffset(int itemIndex)
        {
            if (!AllowDifferentSizedItems)
            {
                Size uniformItemSize = !itemSize.IsEmpty ? itemSize : sizeOfFirstItem;
                var itemsPerLine = Math.Max((int)Math.Floor(ViewportSizeMainAxis / GetSizeOnMainAxis(uniformItemSize)), 1);
                int lineIndex = itemIndex / itemsPerLine;
                int itemIndexInLine = itemIndex % itemsPerLine;
                double offsetMainAxis = itemIndexInLine * GetSizeOnMainAxis(uniformItemSize);
                double offsetCrossAxis = lineIndex * GetSizeOnCrossAxis(uniformItemSize);
                return CreatePoint(offsetMainAxis, offsetCrossAxis);
            }
            else
            {
                double offsetMainAxis = 0, offsetCrossAxis = 0, lineSizeCrossAxis = 0;

                for (int i = 0; i <= itemIndex; i++)
                {
                    Size itemSize = GetItemSizeOrFallback(i);

                    if (offsetMainAxis != 0 && offsetMainAxis + GetSizeOnMainAxis(itemSize) > ViewportSizeMainAxis)
                    {
                        offsetMainAxis = 0;
                        offsetCrossAxis += lineSizeCrossAxis;
                        lineSizeCrossAxis = 0;
                    }

                    if (i != itemIndex)
                    {
                        offsetMainAxis += GetSizeOnMainAxis(itemSize);
                        lineSizeCrossAxis = Math.Max(lineSizeCrossAxis, GetSizeOnCrossAxis(itemSize));
                    }
                }

                return CreatePoint(offsetMainAxis, offsetCrossAxis);
            }
        }

        private void UpdateViewportSize(Size availableSize)
        {
            Size newViewportSize;

            if (ItemsOwner is IHierarchicalVirtualizationAndScrollInfo groupItem)
            {
                Rect viewport = GetViewportFromGroupItem(groupItem);
                ScrollOffset = viewport.Location;
                newViewportSize = viewport.Size;
                cacheLength = groupItem.Constraints.CacheLength;
                cacheLengthUnit = groupItem.Constraints.CacheLengthUnit;

                // Retain the current viewport size if the new viewport size received from the parent virtualizing panel is zero.
                // This is necessary for the BringIndexIntoView function to work.
                if (newViewportSize.Width == 0 && newViewportSize.Height == 0)
                {
                    return;
                }
            }
            else
            {
                newViewportSize = availableSize;
                cacheLength = GetCacheLength(ItemsOwner);
                cacheLengthUnit = GetCacheLengthUnit(ItemsOwner);
            }

            if (newViewportSize != ViewportSize)
            {
                ViewportSize = newViewportSize;
                InvalidateScrollInfo();
            }
        }

        private void FindStartIndexAndOffset_DifferentSizedItems()
        {
            if (ViewportSize.Width == 0 && ViewportSize.Height == 0)
            {
                startItemIndex = -1;
                startItemOffsetMainAxis = 0;
                startItemOffsetCrossAxis = 0;
                return;
            }

            double startOffsetCrossAxis = CalculateStartOffsetCrossAxis();

            if (startOffsetCrossAxis <= 0)
            {
                startItemIndex = Items.Count > 0 ? 0 : -1;
                startItemOffsetMainAxis = 0;
                startItemOffsetCrossAxis = 0;
                return;
            }

            startItemIndex = -1;

            double offsetMainAxis = 0, offsetCrossAxis = 0, lineSizeCrossAxis = 0;
            int indexOfFirstRowItem = 0;

            int itemsCount = items.Count; // local variable for performance
            for (int itemIndex = 0; itemIndex < itemsCount; itemIndex++)
            {
                Size itemSize = GetItemSizeOrFallback(itemIndex);
                double itemSizeOnMainAxis = GetSizeOnMainAxis(itemSize);
                double itemSizeOnCrossAxis = GetSizeOnCrossAxis(itemSize);

                if (offsetMainAxis + itemSizeOnMainAxis > ViewportSizeMainAxis && offsetMainAxis != 0)
                {
                    offsetMainAxis = 0;
                    offsetCrossAxis += lineSizeCrossAxis;
                    lineSizeCrossAxis = 0;
                    indexOfFirstRowItem = itemIndex;
                }
                offsetMainAxis += itemSizeOnMainAxis;
                lineSizeCrossAxis = Math.Max(lineSizeCrossAxis, itemSizeOnCrossAxis);

                if (offsetCrossAxis + lineSizeCrossAxis > startOffsetCrossAxis)
                {
                    if (cacheLengthUnit == VirtualizationCacheLengthUnit.Item)
                    {
                        startItemIndex = Math.Max(indexOfFirstRowItem - (int)cacheLength.CacheBeforeViewport, 0);
                        var itemOffset = FindItemOffset(startItemIndex);
                        startItemOffsetMainAxis = GetPositionOnMainAxis(itemOffset);
                        startItemOffsetCrossAxis = GetPositionOnCrossAxis(itemOffset);
                    }
                    else
                    {
                        startItemIndex = indexOfFirstRowItem;
                        startItemOffsetMainAxis = 0;
                        startItemOffsetCrossAxis = offsetCrossAxis;
                    }
                    break;
                }
            }

            // make sure that at least one item is realized to allow correct calculation of the extend
            if (startItemIndex == -1 && Items.Count > 0)
            {
                startItemIndex = Items.Count - 1;
                startItemOffsetMainAxis = offsetMainAxis;
                startItemOffsetCrossAxis = offsetCrossAxis;
            }
        }

        private void RealizeItemsAndFindEndIndex_DifferentSizedItems()
        {
            if (startItemIndex == -1)
            {
                endItemIndex = -1;
                return;
            }

            int newEndItemIndex = Items.Count - 1;
            bool endItemIndexFound = false;

            double endOffsetCrossAxis = CalculateEndOffsetCrossAxis();

            double offsetMainAxis = startItemOffsetMainAxis;
            double offsetCrossAxis = startItemOffsetCrossAxis;
            double lineSizeCrossAxis = 0;

            for (int itemIndex = startItemIndex; itemIndex <= newEndItemIndex; itemIndex++)
            {
                object item = Items[itemIndex];

                var container = ItemContainerManager.Realize(itemIndex);

                if (container == bringIntoViewContainer)
                {
                    bringIntoViewItemIndex = -1;
                    bringIntoViewContainer = null;
                }

                Size upfrontKnownItemSize = GetUpfrontKnownItemSizeOrEmpty(item);

                if (!container.IsMeasureValid)
                {
                    container.Measure(!upfrontKnownItemSize.IsEmpty ? upfrontKnownItemSize : InfiniteSize);
                }

                Size containerSize = !upfrontKnownItemSize.IsEmpty ? upfrontKnownItemSize : container.DesiredSize;
                itemSizesCache[itemIndex] = containerSize;

                if (offsetMainAxis != 0 && offsetMainAxis + GetSizeOnMainAxis(containerSize) > ViewportSizeMainAxis)
                {
                    offsetMainAxis = 0;
                    offsetCrossAxis += lineSizeCrossAxis;
                    lineSizeCrossAxis = 0;
                }

                offsetMainAxis += GetSizeOnMainAxis(containerSize);
                lineSizeCrossAxis = Math.Max(lineSizeCrossAxis, GetSizeOnCrossAxis(containerSize));

                if (endItemIndexFound == false)
                {
                    if (offsetCrossAxis >= endOffsetCrossAxis)
                    {
                        endItemIndexFound = true;

                        newEndItemIndex = itemIndex;

                        if (cacheLengthUnit == VirtualizationCacheLengthUnit.Item)
                        {
                            newEndItemIndex = Math.Min(newEndItemIndex + (int)cacheLength.CacheAfterViewport, Items.Count - 1);
                            // loop continues until newEndItemIndex is reached
                        }
                    }
                }
            }

            endItemIndex = newEndItemIndex;
        }

        private void VirtualizeItems()
        {
            keptRealizedFocusedContainer = null;

            var itemsToBeRealized = Utils.HashSetOfRange(Items, startItemIndex, endItemIndex);

            foreach (var (item, container) in ItemContainerManager.RealizedContainers.ToList())
            {
                if (container == bringIntoViewContainer)
                {
                    continue;
                }

                if (container.IsKeyboardFocusWithin) 
                {
                    keptRealizedFocusedContainer = container;
                    continue;
                }

                if (!itemsToBeRealized.Contains(item))
                {
                    ItemContainerManager.Virtualize(item);
                }
            }
        }

        private void UpdateExtent()
        {
            var viewportForExtentCalculation = GetViewportForExtentCalculation();

            Size extent = AllowDifferentSizedItems
                ? CalculateExtent_DifferentSizedItems(viewportForExtentCalculation)
                : CalculateExtent_UniformSizedItems(viewportForExtentCalculation);

            if (extent != Extent)
            {
                Extent = extent;
                InvalidateScrollInfo();
            }
        }

        private Size CalculateExtent_UniformSizedItems(Size viewportForExtentCalculation)
        {
            Size uniformItemSize = !itemSize.IsEmpty ? itemSize : sizeOfFirstItem;

            if (uniformItemSize.IsEmpty)
            {
                return new Size(0, 0);
            }

            double itemSizeOnMainAxis = GetSizeOnMainAxis(uniformItemSize);
            double itemSizeOnCrossAxis = GetSizeOnCrossAxis(uniformItemSize);

            double viewportSizeMainAxis = GetSizeOnMainAxis(viewportForExtentCalculation);

            int itemsPerLine = Math.Max((int)Math.Floor(viewportSizeMainAxis / itemSizeOnMainAxis), 1);
            int linesCount = (int)Math.Ceiling(items.Count / (double)itemsPerLine);

            double extendMainAxis = Math.Min(itemsPerLine, items.Count) * itemSizeOnMainAxis;
            double extendCrossAxis = linesCount * itemSizeOnCrossAxis;

            return CreateSize(extendMainAxis, extendCrossAxis);
        }

        private Size CalculateExtent_DifferentSizedItems(Size viewportForExtentCalculation)
        {
            if (startItemIndex == -1)
            {
                return new Size(0, 0);
            }

            double viewportSizeMainAxis = GetSizeOnMainAxis(viewportForExtentCalculation);   

            double estimatedExtentCrossAxis = 0;
            double estimatedExtentMainAxis = viewportSizeMainAxis; // initialize with viewport for a more stable extent

            double positionMainAxisSize = 0;
            double lineSizeCrossAxis = 0;

            int itemsCount = items.Count; // local variable for performance
            for (int itemIndex = 0; itemIndex < itemsCount; itemIndex++)
            {
                var itemSize = GetItemSizeOrFallback(itemIndex);
                double itemSizeOnMainAxis = GetSizeOnMainAxis(itemSize);
                double itemSizeOnCrossAxis = GetSizeOnCrossAxis(itemSize);

                if (positionMainAxisSize != 0 && positionMainAxisSize + itemSizeOnMainAxis > viewportSizeMainAxis)
                {
                    estimatedExtentCrossAxis += lineSizeCrossAxis;
                    positionMainAxisSize = 0;
                    lineSizeCrossAxis = 0;
                }

                positionMainAxisSize += itemSizeOnMainAxis;
                lineSizeCrossAxis = Math.Max(lineSizeCrossAxis, itemSizeOnCrossAxis);
                estimatedExtentMainAxis = Math.Max(estimatedExtentMainAxis, positionMainAxisSize);
            }            
            estimatedExtentCrossAxis += lineSizeCrossAxis;

            return CreateSize(estimatedExtentMainAxis, estimatedExtentCrossAxis);
        }

        /// <summary>
        /// To guarantee a stable extent, the viewport used for calculating the extent
        /// must exclude the size of the auto scrollbars that might appear.
        /// </summary>
        private Size GetViewportForExtentCalculation()
        {
            Size viewport = ViewportSize;

            var scrollOwner = ScrollOwner;

            if (ItemsOwner is GroupItem groupItem
                && VisualTreeHelper.GetParent(groupItem) is IScrollInfo parentScrollInfo
                && parentScrollInfo.ScrollOwner is { } parentScrollOwner)
            {
                scrollOwner = parentScrollOwner;
            }

            if (orientation == Orientation.Horizontal
                && scrollOwner?.VerticalScrollBarVisibility == ScrollBarVisibility.Auto
                && scrollOwner?.ComputedVerticalScrollBarVisibility == Visibility.Collapsed)
            {
                viewport.Width -= SystemParameters.VerticalScrollBarWidth;
            }

            if (orientation == Orientation.Vertical
                && scrollOwner?.HorizontalScrollBarVisibility == ScrollBarVisibility.Auto
                && scrollOwner?.ComputedHorizontalScrollBarVisibility == Visibility.Collapsed)
            {
                viewport.Height -= SystemParameters.HorizontalScrollBarHeight;
            }

            return viewport;
        }

        /// <summary>
        /// Disconnects recycled containers that were not reused from the visual tree 
        /// so that they do not interfere with things like Arrange, keyboard navigation, etc.
        /// </summary>
        private void DisconnectRecycledContainers()
        {
            var children = InternalChildren;
            var realizedContainers = new HashSet<UIElement>(ItemContainerManager.RealizedContainers.Values);

            for (int i = children.Count - 1; i >= 0; i--)
            {
                if (!realizedContainers.Contains(children[i]))
                {
                    RemoveInternalChildRange(i, 1);
                }
            }
        }

        private Size CalculateDesiredSize(Size availableSize)
        {
            double desiredWidth = Math.Min(availableSize.Width, Extent.Width);
            double desiredHeight = Math.Min(availableSize.Height, Extent.Height);
            return new Size(desiredWidth, desiredHeight);
        }

        private double CalculateStartOffsetCrossAxis()
        {
            double cache = 0;

            if (cacheLengthUnit == VirtualizationCacheLengthUnit.Page)
            {
                cache = cacheLength.CacheAfterViewport * ViewportSizeCrossAxis;
            }
            else if (cacheLengthUnit == VirtualizationCacheLengthUnit.Pixel)
            {
                cache = cacheLength.CacheAfterViewport;
            }

            return Math.Max(ScrollOffsetCrossAxis - cache, 0);
        }

        private double CalculateEndOffsetCrossAxis()
        {
            double cache = 0;

            if (cacheLengthUnit == VirtualizationCacheLengthUnit.Page)
            {
                cache = cacheLength.CacheAfterViewport * ViewportSizeCrossAxis;
            }
            else if (cacheLengthUnit == VirtualizationCacheLengthUnit.Pixel)
            {
                cache = cacheLength.CacheAfterViewport;
            }

            return ScrollOffsetCrossAxis + ViewportSizeCrossAxis + cache;
        }

        private Size GetUpfrontKnownItemSizeOrEmpty(object item)
        {
            if (!itemSize.IsEmpty)
            {
                return itemSize;
            }
            if (!AllowDifferentSizedItems)
            {
                return sizeOfFirstItem;
            }
            if (itemSizeProvider is not null)
            {
                return itemSizeProvider.GetSizeForItem(item);
            }
            return Size.Empty;
        }

        private Size GetItemSizeOrFallback(int itemIndex)
        {
            if (itemSizeProvider is not null)
            {
                return itemSizeProvider.GetSizeForItem(items[itemIndex]);
            }

            var cachedItemSize = itemSizesCache[itemIndex];

            if (cachedItemSize != default)
            {
                return cachedItemSize;
            }

            return fallbackItemSize;
        }

        private void ArrangeLine(double lineSizeMainAxis, List<UIElement> children, List<Size> childSizes, double lineOffsetCrossAxis, bool hierarchical)
        {
            double spaceTakenMainAxis;
            double stretchAmount = 0;

            if (AllowDifferentSizedItems)
            {
                spaceTakenMainAxis = childSizes.Sum(GetSizeOnMainAxis);

                if (StretchItems)
                {
                    double unusedSpace = lineSizeMainAxis - spaceTakenMainAxis;
                    stretchAmount = unusedSpace / children.Count;
                    spaceTakenMainAxis = lineSizeMainAxis;
                }
            }
            else
            {
                double childSizeMainAxis = GetSizeOnMainAxis(childSizes[0]);
                int itemsPerRow = IsGridLayoutEnabled ? Math.Max(1, (int)Math.Floor(lineSizeMainAxis / childSizeMainAxis)) : children.Count;

                if (StretchItems)
                {
                    var firstChild = (FrameworkElement)children[0];
                    double childMaxSizeMainAxis = orientation == Orientation.Horizontal ? firstChild.MaxWidth : firstChild.MaxHeight;
                    double stretchedChildSizeMainAxis = Math.Max(childSizeMainAxis, Math.Min(lineSizeMainAxis / itemsPerRow, childMaxSizeMainAxis));
                    stretchAmount = stretchedChildSizeMainAxis - childSizeMainAxis;
                    spaceTakenMainAxis = itemsPerRow * stretchedChildSizeMainAxis;
                }
                else
                {
                    spaceTakenMainAxis = itemsPerRow * childSizeMainAxis;
                }
            }

            double innerSpacing = 0;
            double outerSpacing = 0;

            if (spaceTakenMainAxis < lineSizeMainAxis)
            {
                CalculateRowSpacing(lineSizeMainAxis, children, spaceTakenMainAxis, out innerSpacing, out outerSpacing);
            }

            double positionMainAxis = (hierarchical ? 0 : -GetPositionOnMainAxis(ScrollOffset)) + outerSpacing;

            double lineSizeCrossAxis = Enumerable.Range(0, children.Count).Select(i => GetSizeOnCrossAxis(childSizes[i])).Max();

            for (int i = 0; i < children.Count; i++)
            {
                var child = children[i];
                Size childSize = childSizes[i];
                double mainAxisSize = GetSizeOnMainAxis(childSize) + stretchAmount;
                double crossAxisSize = GetsArrangeSizeCrossAxis(child, childSize, lineSizeCrossAxis);
                double positionCrossAxis = GetArrangePositionCrossAxis(child, crossAxisSize, lineOffsetCrossAxis, lineSizeCrossAxis);
                child.Arrange(CreateRect(positionMainAxis, positionCrossAxis, mainAxisSize, crossAxisSize));
                positionMainAxis += GetSizeOnMainAxis(childSize) + stretchAmount + innerSpacing;
            }
        }

        private double GetArrangePositionCrossAxis(UIElement child, double childArrangeCrossAxisSize, double linePositionCrossAxis, double lineSizeCrossAxis)
        {
            if (ItemAlignment == ItemAlignment.End)
            {
                return linePositionCrossAxis + (lineSizeCrossAxis - childArrangeCrossAxisSize);
            }
            if (ItemAlignment == ItemAlignment.Center)
            {
                return linePositionCrossAxis + (lineSizeCrossAxis - childArrangeCrossAxisSize) / 2;
            }
            return linePositionCrossAxis;
        }

        private double GetsArrangeSizeCrossAxis(UIElement child, Size childSize, double lineSizeCrossAxis)
        {
            if (ItemAlignment == ItemAlignment.Stretch)
            {
                if (child is FrameworkElement fe)
                {
                    if (orientation == Orientation.Horizontal && double.IsNaN(fe.Height))
                    {
                        return Math.Min(lineSizeCrossAxis, fe.MaxHeight);
                    }
                    if (orientation == Orientation.Vertical && double.IsNaN(fe.Width))
                    {
                        return Math.Min(lineSizeCrossAxis, fe.MaxWidth);
                    }
                }
                return lineSizeCrossAxis;
            }
            return GetSizeOnCrossAxis(childSize);
        }

        private void CalculateRowSpacing(double rowWidth, List<UIElement> children, double summedUpChildWidth, out double innerSpacing, out double outerSpacing)
        {
            int childCount;

            if (AllowDifferentSizedItems)
            {
                childCount = children.Count;
            }
            else
            {
                childCount = IsGridLayoutEnabled ? (int)Math.Max(1, Math.Floor(rowWidth / GetSizeOnMainAxis(sizeOfFirstItem))) : children.Count;
            }

            double unusedWidth = Math.Max(0, rowWidth - summedUpChildWidth);

            switch (SpacingMode)
            {
                case SpacingMode.Uniform:
                    innerSpacing = outerSpacing = unusedWidth / (childCount + 1);
                    break;

                case SpacingMode.BetweenItemsOnly:
                    innerSpacing = unusedWidth / Math.Max(childCount - 1, 1);
                    outerSpacing = 0;
                    break;

                case SpacingMode.StartAndEndOnly:
                    innerSpacing = 0;
                    outerSpacing = unusedWidth / 2;
                    break;

                case SpacingMode.None:
                default:
                    innerSpacing = 0;
                    outerSpacing = 0;
                    break;
            }
        }

        private Size CalculateAverageItemSize()
        {
            var itemSizes = itemSizesCache.Where(size => size != default).ToArray();

            if (itemSizes.Length > 0)
            {
                return new Size(
                    Math.Round(itemSizes.Average(size => size.Width)),
                    Math.Round(itemSizes.Average(size => size.Height)));
            }

            return fallbackItemSize;
        }

        #region scroll info

        // TODO determine exact scroll amount for item based scrolling when AllowDifferentSizedItems is true

        protected override double GetLineUpScrollAmount()
        {
            return -Math.Min(GetItemSizeForScrolling().Height * ScrollLineDeltaItem, ViewportSize.Height);
        }

        protected override double GetLineDownScrollAmount()
        {
            return Math.Min(GetItemSizeForScrolling().Height * ScrollLineDeltaItem, ViewportSize.Height);
        }

        protected override double GetLineLeftScrollAmount()
        {
            return -Math.Min(GetItemSizeForScrolling().Width * ScrollLineDeltaItem, ViewportSize.Width);
        }

        protected override double GetLineRightScrollAmount()
        {
            return Math.Min(GetItemSizeForScrolling().Width * ScrollLineDeltaItem, ViewportSize.Width);
        }

        protected override double GetMouseWheelUpScrollAmount()
        {
            return -Math.Min(GetItemSizeForScrolling().Height * MouseWheelDeltaItem, ViewportSize.Height);
        }

        protected override double GetMouseWheelDownScrollAmount()
        {
            return Math.Min(GetItemSizeForScrolling().Height * MouseWheelDeltaItem, ViewportSize.Height);
        }

        protected override double GetMouseWheelLeftScrollAmount()
        {
            return -Math.Min(GetItemSizeForScrolling().Width * MouseWheelDeltaItem, ViewportSize.Width);
        }

        protected override double GetMouseWheelRightScrollAmount()
        {
            return Math.Min(GetItemSizeForScrolling().Width * MouseWheelDeltaItem, ViewportSize.Width);
        }

        protected override double GetPageUpScrollAmount()
        {
            return -ViewportSize.Height;
        }

        protected override double GetPageDownScrollAmount()
        {
            return ViewportSize.Height;
        }

        protected override double GetPageLeftScrollAmount()
        {
            return -ViewportSize.Width;
        }

        protected override double GetPageRightScrollAmount()
        {
            return ViewportSize.Width;
        }

        #endregion

        #region orientation aware helper methods

        private double GetPositionOnMainAxis(Point point) => orientation == Orientation.Horizontal ? point.X : point.Y;
        private double GetPositionOnCrossAxis(Point point) => orientation == Orientation.Horizontal ? point.Y : point.X;
        private double GetSizeOnMainAxis(Size size) => orientation == Orientation.Horizontal ? size.Width : size.Height;
        private double GetSizeOnCrossAxis(Size size) => orientation == Orientation.Horizontal ? size.Height : size.Width;

        private Point CreatePoint(double positionOnMainAxis, double positionOnCrossAxis)
        {
            return orientation == Orientation.Horizontal
                ? new Point(positionOnMainAxis, positionOnCrossAxis)
                : new Point(positionOnCrossAxis, positionOnMainAxis);
        }

        private Size CreateSize(double sizeOnMainAxis, double sizeOnCrossAxis)
        {
            return orientation == Orientation.Horizontal
             ? new Size(sizeOnMainAxis, sizeOnCrossAxis)
             : new Size(sizeOnCrossAxis, sizeOnMainAxis);
        }

        private Rect CreateRect(double positionOnMainAxis, double positionOnCrossAxis, double sizeOnMainAxis, double sizeOnCrossAxis)
        {
            return orientation == Orientation.Horizontal
             ? new Rect(positionOnMainAxis, positionOnCrossAxis, sizeOnMainAxis, sizeOnCrossAxis)
             : new Rect(positionOnCrossAxis, positionOnMainAxis, sizeOnCrossAxis, sizeOnMainAxis);
        }

        #endregion
    }
}

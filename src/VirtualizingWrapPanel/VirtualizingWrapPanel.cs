using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;

namespace WpfToolkit.Controls
{
    /// <summary>
    /// A implementation of a wrap panel that supports virtualization and can be used in horizontal and vertical orientation.
    /// </summary>
    public class VirtualizingWrapPanel : VirtualizingPanelBase
    {
        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(nameof(Orientation), typeof(Orientation), typeof(VirtualizingWrapPanel), new FrameworkPropertyMetadata(Orientation.Horizontal, FrameworkPropertyMetadataOptions.AffectsMeasure, (obj, args) => ((VirtualizingWrapPanel)obj).Orientation_Changed()));

        public static readonly DependencyProperty ItemSizeProperty = DependencyProperty.Register(nameof(ItemSize), typeof(Size), typeof(VirtualizingWrapPanel), new FrameworkPropertyMetadata(Size.Empty, FrameworkPropertyMetadataOptions.AffectsMeasure, (obj, args) => ((VirtualizingWrapPanel)obj).ItemSize_Changed()));

        public static readonly DependencyProperty ItemSizeProviderProperty = DependencyProperty.Register(nameof(ItemSizeProvider), typeof(IItemSizeProvider), typeof(VirtualizingWrapPanel), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure));

        public static readonly DependencyProperty AllowDifferentSizedItemsProperty = DependencyProperty.Register(nameof(AllowDifferentSizedItems), typeof(bool), typeof(VirtualizingWrapPanel), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsMeasure, (obj, args) => ((VirtualizingWrapPanel)obj).AllowDifferentSizedItems_Changed()));

        public static readonly DependencyProperty SpacingModeProperty = DependencyProperty.Register(nameof(SpacingMode), typeof(SpacingMode), typeof(VirtualizingWrapPanel), new FrameworkPropertyMetadata(SpacingMode.Uniform, FrameworkPropertyMetadataOptions.AffectsArrange));

        public static readonly DependencyProperty StretchItemsProperty = DependencyProperty.Register(nameof(StretchItems), typeof(bool), typeof(VirtualizingWrapPanel), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsArrange));

        /// <summary>
        /// Gets or sets a value that specifies the orientation in which items are arranged. The default value is <see cref="Orientation.Horizontal"/>.
        /// </summary>
        public Orientation Orientation { get => (Orientation)GetValue(OrientationProperty); set => SetValue(OrientationProperty, value); }

        /// <summary>
        /// Gets or sets a value that specifies the size of the items. The default value is <see cref="Size.Empty"/>. 
        /// If the value is <see cref="Size.Empty"/> the item size is determined by measuring the first realized item.
        /// </summary>
        public Size ItemSize { get => (Size)GetValue(ItemSizeProperty); set => SetValue(ItemSizeProperty, value); }

        /// <summary>
        /// Specifies an instance of <see cref="IItemSizeProvider"/> which provides the size of the items. In order to allow
        /// different sized items, also enable the <see cref="AllowDifferentSizedItems"/> property.
        /// </summary>
        public IItemSizeProvider? ItemSizeProvider { get => (IItemSizeProvider?)GetValue(ItemSizeProviderProperty); set => SetValue(ItemSizeProviderProperty, value); }

        /// <summary>
        /// Specifies whether items can have different sizes. The default value is false. If this property is enabled, 
        /// it is strongly recommended to also set the <see cref="ItemSizeProvider"/> property. Otherwise, the position 
        /// of the items is not always guaranteed to be correct.
        /// </summary>
        public bool AllowDifferentSizedItems { get => (bool)GetValue(AllowDifferentSizedItemsProperty); set => SetValue(AllowDifferentSizedItemsProperty, value); }

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
        /// Gets value that indicates whether the <see cref="VirtualizingPanel"/> can virtualize items 
        /// that are grouped or organized in a hierarchy.
        /// </summary>
        /// <returns>always true for <see cref="VirtualizingWrapPanel"/></returns>
        protected override bool CanHierarchicallyScrollAndVirtualizeCore => true;

        protected override bool HasLogicalOrientation => true;

        protected override Orientation LogicalOrientation => Orientation == Orientation.Horizontal ? Orientation.Vertical : Orientation.Horizontal;
        
        private static readonly Size FallbackSize = new Size(48, 48);

        private IItemContainerManager ItemContainerManager
        {
            get
            {
                if (_itemContainerManager is null)
                {
                    _itemContainerManager = new ItemContainerManager(
                        ItemContainerGenerator,
                        AddInternalChild,
                        child => RemoveInternalChildRange(InternalChildren.IndexOf(child), 1));
                    _itemContainerManager.ItemsChanged += ItemContainerManager_ItemsChanged;
                }
                return _itemContainerManager;
            }
        }
        private IItemContainerManager? _itemContainerManager;

        /// <summary>
        /// The cache length before and after the viewport. 
        /// </summary>
        private VirtualizationCacheLength cacheLength;
        /// <summary>
        /// The Unit of the cache length. Can be Pixel, Item or Page. 
        /// When the ItemsOwner is a group item it can only be pixel or item.
        /// </summary>
        private VirtualizationCacheLengthUnit cacheLengthUnit;

        private Size? sizeOfFirstItem;
        private Dictionary<object, Size> itemSizesCache = new Dictionary<object, Size>(); // TODO allow clear cache?
        private Size? averageItemSizeCache;

        private int itemsInKnownExtend = 0;

        private int startItemIndex = -1;
        private int endItemIndex = -1;

        private double startItemOffsetX = 0;
        private double startItemOffsetY = 0;

        private double knownExtendX = 0;
        private double knownExtendY = 0;

        private int bringIntoViewIndex = -1;
        private FrameworkElement? bringIntoViewContainer;

        private void ItemContainerManager_ItemsChanged(object? sender, ItemContainerManagerItemsChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove
                || e.Action == NotifyCollectionChangedAction.Replace)
            {
                foreach (var key in itemSizesCache.Keys.Except(Items).ToList())
                {
                    itemSizesCache.Remove(key);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                itemSizesCache.Clear();
            }

            itemsInKnownExtend = 0; // force recalucaltion of extend
        }

        private void Orientation_Changed()
        {
            MouseWheelScrollDirection = Orientation == Orientation.Horizontal
                                        ? ScrollDirection.Vertical
                                        : ScrollDirection.Horizontal;
            SetVerticalOffset(0);
            SetHorizontalOffset(0);
        }

        private void AllowDifferentSizedItems_Changed()
        {
            foreach (var child in InternalChildren.Cast<UIElement>())
            {
                child.InvalidateMeasure();
            }
        }

        private void ItemSize_Changed()
        {
            foreach (var child in InternalChildren.Cast<UIElement>())
            {
                child.InvalidateMeasure();
            }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            if (ShouldIgnoreMeasure())
            {
                return availableSize;
            }

            ItemContainerManager.IsRecycling = IsRecycling;

            if (ItemsOwner is IHierarchicalVirtualizationAndScrollInfo groupItem)
            {
                var viewport = groupItem.Constraints.Viewport;
                var headerSize = groupItem.HeaderDesiredSizes.PixelSize;

                double viewportWidth = Math.Max(viewport.Size.Width, 0);
                double viewporteHeight = Math.Max(viewport.Size.Height, 0);

                if (Orientation == Orientation.Vertical)
                {
                    viewporteHeight = Math.Max(viewporteHeight - headerSize.Height, 0);
                }

                var viewportSize = new Size(viewportWidth, viewporteHeight);

                cacheLength = groupItem.Constraints.CacheLength;
                cacheLengthUnit = groupItem.Constraints.CacheLengthUnit;

                var desiredSize = Measure(availableSize, viewportSize, viewport.Location, itemsOwnerIsGroupItem: true);

                return desiredSize;
            }
            else
            {
                cacheLength = GetCacheLength(ItemsOwner);
                cacheLengthUnit = GetCacheLengthUnit(ItemsOwner);

                return Measure(availableSize, availableSize, ScrollOffset, itemsOwnerIsGroupItem: false);
            }
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            bool hierarchical = ItemsOwner is IHierarchicalVirtualizationAndScrollInfo;

            ViewportSize = finalSize;

            if (bringIntoViewContainer is not null)
            {
                var offset = FindItemOffset(bringIntoViewIndex); // already oriented (TODO)
                offset = new Point(offset.X - GetX(ScrollOffset), hierarchical ? offset.Y : offset.Y - GetY(ScrollOffset));
                Debug.WriteLine(offset);
                bringIntoViewContainer.Arrange(new Rect(offset, bringIntoViewContainer.DesiredSize));
            }

            foreach (var cachedContainer in ItemContainerManager.CachedContainers)
            {
                cachedContainer.Arrange(new Rect(0, 0, 0, 0));
            }

            double x = startItemOffsetX + GetX(ScrollOffset);
            double y = hierarchical ? startItemOffsetY : startItemOffsetY - GetY(ScrollOffset);
            double rowHeight = 0;
            var rowChilds = new List<IItemContainerInfo>();
            var childSizes = new List<Size>();

            foreach (var child in ItemContainerManager.RealizedContainers
                .Where(container => container.UIElement != bringIntoViewContainer)
                .OrderBy(container => ItemContainerManager.FindItemIndexOfContainer(container)))
            {
                Size? upfrontKnownItemSize = GetUpfrontKnownItemSize(child.Item);

                Size childSize = upfrontKnownItemSize ?? itemSizesCache[child.Item];

                if (x != 0 && x + GetWidth(childSize) > GetWidth(finalSize))
                {
                    ArrangeRow(GetWidth(finalSize), rowChilds, childSizes, y, hierarchical);
                    x = 0;
                    y += rowHeight;
                    rowHeight = 0;
                    rowChilds.Clear();
                    childSizes.Clear();
                }

                x += GetWidth(childSize);
                rowHeight = Math.Max(rowHeight, GetHeight(childSize));
                rowChilds.Add(child);
                childSizes.Add(childSize);
            }

            if (rowChilds.Any())
            {
                ArrangeRow(GetWidth(finalSize), rowChilds, childSizes, y, hierarchical);
            }

            return finalSize;
        }

        protected override void BringIndexIntoView(int index)
        {
            if (index < 0 || index >= Items.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index), $"The argument {nameof(index)} must be >= 0 and < the count of items.");
            }

            var container = ItemContainerManager.Realize(index, out bool _, out bool isNewContainer);


            bringIntoViewIndex = index;
            bringIntoViewContainer = (FrameworkElement)container.UIElement;

            Dispatcher.BeginInvoke(DispatcherPriority.Loaded, () =>
            {
                ItemContainerManager.Virtualize(ItemContainerInfo.For(bringIntoViewContainer, bringIntoViewIndex));
                bringIntoViewIndex = -1;
                bringIntoViewContainer = null;
            });

            bringIntoViewContainer.BringIntoView();
        }

        private Size Measure(Size availableSize, Size viewportSize, Point scrollOffset, bool itemsOwnerIsGroupItem)
        {
            if (bringIntoViewContainer is not null && !bringIntoViewContainer.IsMeasureValid)
            {
                bringIntoViewContainer.Measure(GetUpfrontKnownItemSize(Items[bringIntoViewIndex]) ?? availableSize);

                if (sizeOfFirstItem is null)
                {
                    sizeOfFirstItem = bringIntoViewContainer.DesiredSize;
                }
            }

            bool invalidateScrollInfo = false;
            ScrollOffset = scrollOffset;
            averageItemSizeCache = null;

            if (GetWidth(viewportSize) != GetWidth(ViewportSize))
            {
                knownExtendY = 0;
            }

            if (itemsOwnerIsGroupItem && viewportSize.Width == 0 && viewportSize.Height == 0)
            {
                if (bringIntoViewContainer is null)
                {
                    return new Size(0, 0);
                }
            }
            else
            {
                UpdateViewport(viewportSize, ref invalidateScrollInfo);

                FindStartIndexAndOffset();
                VirtualizeItemsBeforeStartIndex();
                RealizeItemsAndFindEndIndex();
                VirtualizeItemsAfterEndIndex();
            }

            UpdateExtent(ref invalidateScrollInfo);

            if (invalidateScrollInfo)
            {
                ScrollOwner?.InvalidateScrollInfo();
            }

            double desiredWidth = Math.Min(GetWidth(availableSize), GetWidth(Extent));
            double desiredHeight = Math.Min(GetHeight(availableSize), GetHeight(Extent));

            if (itemsOwnerIsGroupItem)
            {
                desiredWidth = Math.Max(desiredWidth, GetWidth(viewportSize));
                desiredHeight = Math.Max(desiredHeight, GetHeight(viewportSize));
            }

            return CreateSize(desiredWidth, desiredHeight);
        }

        public Size GetAverageItemSize()
        {
            if (ItemSize != Size.Empty)
            {
                return ItemSize;
            }
            else if (!AllowDifferentSizedItems)
            {
                return sizeOfFirstItem ?? FallbackSize;
            }
            else
            {
                if (averageItemSizeCache is null && itemSizesCache.Values.Any())
                {
                    averageItemSizeCache = CalculateAverageSize(itemSizesCache.Values);
                }
                return averageItemSizeCache ?? FallbackSize;
            }
        }

        private Point FindItemOffset(int itemIndex)
        {
            double x = 0, y = 0, rowHeight = 0;

            for (int i = 0; i <= itemIndex; i++)
            {
                Size itemSize = GetAssumedItemSize(Items[i]);

                if (x + GetWidth(itemSize) > GetWidth(ViewportSize))
                {
                    x = 0;
                    y += rowHeight;
                    rowHeight = 0;
                }

                if (i != itemIndex)
                {
                    x += GetWidth(itemSize);
                    rowHeight = Math.Max(rowHeight, GetHeight(itemSize));
                }
            }

            return CreatePoint(x, y);
        }


        private void UpdateViewport(Size availableSize, ref bool invalidateScrollInfo)
        {
            bool viewportChanged = availableSize != ViewportSize;

            if (viewportChanged)
            {
                ViewportSize = availableSize;
                invalidateScrollInfo = true;
            }
        }

        private void FindStartIndexAndOffset()
        {
            double startOffsetY = DetermineStartOffsetY();

            if (startOffsetY <= 0)
            {
                startItemIndex = 0;
                startItemOffsetX = 0;
                startItemOffsetY = 0;
                return;
            }

            double x = 0, y = 0, rowHeight = 0;
            int indexOfFirstRowItem = 0;

            int itemIndex = 0;
            foreach (var item in Items) // foreach seems to be faster than a for loop
            {
                Size itemSize = GetAssumedItemSize(item);

                if (x + GetWidth(itemSize) > GetWidth(ViewportSize) && x != 0)
                {
                    x = 0;
                    y += rowHeight;
                    rowHeight = 0;
                    indexOfFirstRowItem = itemIndex;
                }
                x += GetWidth(itemSize);
                rowHeight = Math.Max(rowHeight, GetHeight(itemSize));

                if (y + rowHeight > startOffsetY)
                {
                    if (cacheLengthUnit == VirtualizationCacheLengthUnit.Item)
                    {
                        startItemIndex = Math.Max(indexOfFirstRowItem - (int)cacheLength.CacheBeforeViewport, 0);
                        var itemOffset = FindItemOffset(startItemIndex);
                        startItemOffsetX = GetX(itemOffset);
                        startItemOffsetY = GetY(itemOffset);
                    }
                    else
                    {
                        startItemIndex = indexOfFirstRowItem;
                        startItemOffsetX = 0;
                        startItemOffsetY = y;
                    }
                    break;
                }

                itemIndex++;
            }

            // make sure that at least one item is realized to allow correct calculation of the extend
            if (startItemIndex == -1 && Items.Count > 0)
            {
                startItemIndex = Items.Count - 1;
                startItemOffsetX = 0;
                startItemOffsetY = 0;
            }
        }

        private void RealizeItemsAndFindEndIndex()
        {
            if (startItemIndex == -1)
            {
                endItemIndex = -1;
                knownExtendX = 0;
                knownExtendY = 0;
                itemsInKnownExtend = 0;
                return;
            }

            int newEndItemIndex = Items.Count - 1;

            double endOffsetY = DetermineEndOffsetY();

            double x = startItemOffsetX;
            double y = startItemOffsetY;
            double rowHeight = 0;

            knownExtendX = 0;

            for (int itemIndex = startItemIndex; itemIndex < Items.Count; itemIndex++)
            {
                if (itemIndex == 0)
                {
                    sizeOfFirstItem = null;
                }

                object item = Items[itemIndex];

                var container = ItemContainerManager.Realize(itemIndex, out bool _, out bool isNewContainer);

                Size? upfrontKnownItemSize = GetUpfrontKnownItemSize(item);

                if (!container.IsMeasureValid)
                {
                    Size availableSize = upfrontKnownItemSize ?? new Size(double.PositiveInfinity, double.PositiveInfinity);
                    container.Measure(availableSize);
                }

                var containerSize = DetermineContainerSize(container, upfrontKnownItemSize);

                if (!AllowDifferentSizedItems && sizeOfFirstItem == null)
                {
                    sizeOfFirstItem = containerSize;
                }

                if (x + GetWidth(containerSize) > GetWidth(ViewportSize) && x != 0)
                {
                    x = 0;
                    y += rowHeight;
                    rowHeight = 0;
                }

                x += GetWidth(containerSize);
                knownExtendX = Math.Max(x, knownExtendX);
                rowHeight = Math.Max(rowHeight, GetHeight(containerSize));

                if (newEndItemIndex == Items.Count - 1)
                {
                    if (!AllowDifferentSizedItems
                        && itemIndex + 1 < Items.Count
                        && x + sizeOfFirstItem!.Value.Width > GetWidth(ViewportSize)
                        && y + rowHeight >= endOffsetY)
                    {
                        newEndItemIndex = itemIndex;
                    }
                    else if (y >= endOffsetY)
                    {
                        newEndItemIndex = itemIndex;
                    }

                    if (newEndItemIndex != Items.Count - 1 && cacheLengthUnit == VirtualizationCacheLengthUnit.Item)
                    {
                        newEndItemIndex = Math.Min(newEndItemIndex + (int)cacheLength.CacheAfterViewport, Items.Count - 1);
                    }
                }

                if (itemIndex >= newEndItemIndex)
                {
                    break;
                }
            }

            endItemIndex = newEndItemIndex;
            knownExtendY = Math.Max(y + rowHeight, knownExtendY);
            itemsInKnownExtend = Math.Max(endItemIndex + 1, itemsInKnownExtend);
        }

        private Size DetermineContainerSize(IItemContainerInfo container, Size? upfrontKnownItemSize)
        {
            if (AllowDifferentSizedItems)
            {
                if (upfrontKnownItemSize is not null)
                {
                    return upfrontKnownItemSize.Value;
                }
                itemSizesCache[container.Item] = container.DesiredSize;
                return container.DesiredSize;
            }
            else
            {
                return upfrontKnownItemSize ?? container.DesiredSize;
            }
        }

        private void VirtualizeItemsBeforeStartIndex()
        {
            var containers = ItemContainerManager.RealizedContainers.ToList();
            foreach (var container in containers)
            {
                int itemIndex = ItemContainerManager.FindItemIndexOfContainer(container);

                if (container.UIElement != bringIntoViewContainer && itemIndex < startItemIndex)
                {
                    ItemContainerManager.Virtualize(container);
                }
            }
        }

        private void VirtualizeItemsAfterEndIndex()
        {
            var containers = ItemContainerManager.RealizedContainers.ToList();
            foreach (var container in containers)
            {
                int itemIndex = ItemContainerManager.FindItemIndexOfContainer(container);

                if (container.UIElement != bringIntoViewContainer && itemIndex > endItemIndex)
                {
                    ItemContainerManager.Virtualize(container);
                }
            }
        }

        private void UpdateExtent(ref bool invalidateScrollInfo)
        {
            Size extent;

            if (!AllowDifferentSizedItems)
            {
                if (ItemSize != Size.Empty)
                {
                    extent = CalculateExtentForSameSizeItems(ItemSize);
                }
                else
                {
                    extent = CalculateExtentForSameSizeItems(sizeOfFirstItem ?? FallbackSize);
                }
            }
            else
            {
                if (itemsInKnownExtend == 0)
                {
                    extent = CalculateExtentForSameSizeItems(FallbackSize);
                }
                else
                {
                    double estimatedExtend = ((double)Items.Count / itemsInKnownExtend) * knownExtendY;
                    extent = CreateSize(knownExtendX, estimatedExtend);
                }
            }

            if (extent != Extent)
            {
                Extent = extent;
                invalidateScrollInfo = true;
            }

            if (GetY(ScrollOffset) + GetHeight(ViewportSize) > GetHeight(Extent))
            {
                ScrollOffset = CreatePoint(GetX(ScrollOffset), Math.Max(0, GetHeight(Extent) - GetHeight(ViewportSize)));
                invalidateScrollInfo = true;
            }
        }

        private Size CalculateExtentForSameSizeItems(Size itemSize)
        {
            int itemsPerRow = (int)Math.Max(1, Math.Floor(GetWidth(ViewportSize) / GetWidth(itemSize)));
            double extentY = Math.Ceiling(((double)Items.Count) / itemsPerRow) * GetHeight(itemSize);
            return CreateSize(knownExtendX, extentY);
        }

        private double DetermineStartOffsetY()
        {
            double cacheLength = 0;

            if (cacheLengthUnit == VirtualizationCacheLengthUnit.Page)
            {
                cacheLength = this.cacheLength.CacheBeforeViewport * GetHeight(ViewportSize);
            }
            else if (cacheLengthUnit == VirtualizationCacheLengthUnit.Pixel)
            {
                cacheLength = this.cacheLength.CacheBeforeViewport;
            }

            return Math.Max(GetY(ScrollOffset) - cacheLength, 0);
        }

        private double DetermineEndOffsetY()
        {
            double cacheLength = 0;

            if (cacheLengthUnit == VirtualizationCacheLengthUnit.Page)
            {
                cacheLength = this.cacheLength.CacheAfterViewport * GetHeight(ViewportSize);
            }
            else if (cacheLengthUnit == VirtualizationCacheLengthUnit.Pixel)
            {
                cacheLength = this.cacheLength.CacheAfterViewport;
            }

            return Math.Max(GetY(ScrollOffset), 0) + GetHeight(ViewportSize) + cacheLength;
        }

        private Size? GetUpfrontKnownItemSize(object item)
        {
            if (ItemSize != Size.Empty)
            {
                return ItemSize;
            }
            if (!AllowDifferentSizedItems && sizeOfFirstItem != null)
            {
                return sizeOfFirstItem;
            }
            if (ItemSizeProvider != null)
            {
                var size = ItemSizeProvider.GetSizeForItem(item);
                itemSizesCache[item] = size;
                return size;
            }
            return null;
        }

        private Size GetAssumedItemSize(object item)
        {
            if (GetUpfrontKnownItemSize(item) is Size upfrontKnownItemSize)
            {
                return upfrontKnownItemSize;
            }

            if (itemSizesCache.TryGetValue(item, out Size cachedItemSize))
            {
                return cachedItemSize;
            }

            return GetAverageItemSize();
        }

        private void ArrangeRow(double rowWidth, List<IItemContainerInfo> children, List<Size> childSizes, double y, bool hierarchical)
        {
            double extraWidth = 0;
            double innerSpacing = 0;
            double outerSpacing = 0;

            if (StretchItems) // TODO: handle MaxWidth/MaxHeight and apply spacing
            {
                if (AllowDifferentSizedItems)
                {
                    double summedUpChildWidth = childSizes.Sum(childSize => GetWidth(childSize));
                    double unusedWidth = rowWidth - summedUpChildWidth;
                    extraWidth = unusedWidth / children.Count;
                }
                else
                {
                    double childsWidth = GetWidth(sizeOfFirstItem!.Value);
                    var itemsPerRow = Math.Max(1, Math.Floor(rowWidth / childsWidth));
                    double unusedWidth = rowWidth - itemsPerRow * childsWidth;
                    extraWidth = unusedWidth / itemsPerRow;
                }
            }
            else
            {
                CalculateRowSpacing(rowWidth, children, childSizes, out innerSpacing, out outerSpacing);
            }

            double x = hierarchical ? outerSpacing : -GetX(ScrollOffset) + outerSpacing;

            for (int i = 0; i < children.Count; i++)
            {
                var child = children[i];
                Size childSize = childSizes[i];
                child.Arrange(CreateRect(x, y, GetWidth(childSize) + extraWidth, GetHeight(childSize)));
                x += GetWidth(childSize) + extraWidth + innerSpacing;
            }
        }

        private void CalculateRowSpacing(double rowWidth, List<IItemContainerInfo> children, List<Size> childSizes, out double innerSpacing, out double outerSpacing)
        {
            int childCount;
            double summedUpChildWidth;

            if (AllowDifferentSizedItems)
            {
                childCount = children.Count;
                summedUpChildWidth = childSizes.Sum(childSize => GetWidth(childSize));
            }
            else
            {
                childCount = (int)Math.Max(1, Math.Floor(rowWidth / GetWidth(sizeOfFirstItem!.Value)));
                summedUpChildWidth = childCount * GetWidth(sizeOfFirstItem.Value);
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

        private Size CalculateAverageSize(ICollection<Size> sizes)
        {
            if (sizes.Any())
            {
                return new Size(sizes.Average(size => size.Width), sizes.Average(size => size.Height));
            }
            return Size.Empty;
        }

        #region scroll info

        // TODO determine line height

        protected override double GetLineUpScrollAmount()
        {
            return -Math.Min(GetAverageItemSize().Height * ScrollLineDeltaItem, ViewportSize.Height);
        }

        protected override double GetLineDownScrollAmount()
        {
            return Math.Min(GetAverageItemSize().Height * ScrollLineDeltaItem, ViewportSize.Height);
        }

        protected override double GetLineLeftScrollAmount()
        {
            return -Math.Min(GetAverageItemSize().Width * ScrollLineDeltaItem, ViewportSize.Width);
        }

        protected override double GetLineRightScrollAmount()
        {
            return Math.Min(GetAverageItemSize().Width * ScrollLineDeltaItem, ViewportSize.Width);
        }

        protected override double GetMouseWheelUpScrollAmount()
        {
            return -Math.Min(GetAverageItemSize().Height * MouseWheelDeltaItem, ViewportSize.Height);
        }

        protected override double GetMouseWheelDownScrollAmount()
        {
            return Math.Min(GetAverageItemSize().Height * MouseWheelDeltaItem, ViewportSize.Height);
        }

        protected override double GetMouseWheelLeftScrollAmount()
        {
            return -Math.Min(GetAverageItemSize().Width * MouseWheelDeltaItem, ViewportSize.Width);
        }

        protected override double GetMouseWheelRightScrollAmount()
        {
            return Math.Min(GetAverageItemSize().Width * MouseWheelDeltaItem, ViewportSize.Width);
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

        private double GetX(Point point) => Orientation == Orientation.Horizontal ? point.X : point.Y;
        private double GetY(Point point) => Orientation == Orientation.Horizontal ? point.Y : point.X;
        private double GetWidth(Size size) => Orientation == Orientation.Horizontal ? size.Width : size.Height;
        private double GetHeight(Size size) => Orientation == Orientation.Horizontal ? size.Height : size.Width;
        private Point CreatePoint(double x, double y) => Orientation == Orientation.Horizontal ? new Point(x, y) : new Point(y, x);
        private Size CreateSize(double width, double height) => Orientation == Orientation.Horizontal ? new Size(width, height) : new Size(height, width);
        private Rect CreateRect(double x, double y, double width, double height) => Orientation == Orientation.Horizontal ? new Rect(x, y, width, height) : new Rect(y, x, height, width);

        #endregion
    }
}

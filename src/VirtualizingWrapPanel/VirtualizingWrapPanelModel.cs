using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;

namespace WpfToolkit.Controls;

internal class VirtualizingWrapPanelModel : VirtualizingPanelModelBase
{
    private static readonly Size FallbackSize = new Size(48, 48);

    public Orientation Orientation { get; set; } = Orientation.Horizontal;

    public Size FixedItemSize { get; set; } = Size.Empty;
    public IItemSizeProvider? ItemSizeProvider { get; set; } = null;
    public bool AllowDifferentSizedItems { get; set; } = false;

    public bool StretchItems { get; set; }
    public SpacingMode SpacingMode { get; set; }

    private Size? sizeOfFirstItem;
    private Dictionary<object, Size> itemSizesCache = new Dictionary<object, Size>();
    private Size? averageItemSizeCache;

    private int itemsInKnownExtend = 0;

    private int startItemIndex = -1;
    private int endItemIndex = -1;

    private double startItemOffsetX = 0;
    private double startItemOffsetY = 0;

    private double knownExtendX = 0;
    private double knownExtendY = 0;

    private readonly IItemContainerManager itemContainerManager;
    private readonly IChildrenCollection childrenCollection;

    private readonly List<object> items;

    public VirtualizingWrapPanelModel(IItemContainerManager itemContainerManager, IChildrenCollection childrenCollection)
    {
        this.itemContainerManager = itemContainerManager;
        this.childrenCollection = childrenCollection;
        itemContainerManager.ItemsChanged += ItemContainerManager_ItemsChanged;
        items = new List<object>(itemContainerManager.Items);
    }

    private void ItemContainerManager_ItemsChanged(object? sender, ItemContainerManagerItemsChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Remove
            || e.Action == NotifyCollectionChangedAction.Replace)
        {
            foreach (var item in items.Except(itemContainerManager.Items))
            {
                itemSizesCache.Remove(item);
            }
            if (!itemContainerManager.IsRecycling)
            {
                foreach (var container in e.RemovedContainers)
                {
                    childrenCollection.RemoveChild(container);
                }
            }
        }
        else if (e.Action == NotifyCollectionChangedAction.Reset)
        {
            itemSizesCache.Clear();
            // childrenCollection is cleared automatically
        }

        itemsInKnownExtend = 0; // force recalucaltion of extend

        items.Clear();
        items.AddRange(itemContainerManager.Items);
    }

    public Size OnMeasure(Size availableSize)
    {
        return OnMeasure(availableSize, availableSize, ScrollOffset);
    }

    public Size OnMeasure(Size availableSize, Size viewportSize, Point scrollOffset)
    {
        bool invalidateScrollInfo = false;
        ScrollOffset = scrollOffset;
        averageItemSizeCache = null;

        if (GetWidth(viewportSize) != GetWidth(ViewportSize))
        {
            knownExtendY = 0;
        }

        UpdateViewport(viewportSize, ref invalidateScrollInfo);
        FindStartIndexAndOffset();
        VirtualizeItemsBeforeStartIndex();
        RealizeItemsAndFindEndIndex();
        VirtualizeItemsAfterEndIndex();
        UpdateExtent(ref invalidateScrollInfo);

        if (invalidateScrollInfo)
        {
            InvalidateScrollInfo();
        }

        double desiredWidth = Math.Min(GetWidth(availableSize), GetWidth(Extent));
        double desiredHeight = Math.Min(GetHeight(availableSize), GetHeight(Extent));
        return CreateSize(desiredWidth, desiredHeight);
    }

    public Size OnArrange(Size finalSize, bool hierarchical)
    {
        foreach (var cachedContainer in itemContainerManager.CachedContainers)
        {
            cachedContainer.Arrange(new Rect(0, 0, 0, 0));
        }

        double x = startItemOffsetX + GetX(ScrollOffset);
        double y = hierarchical ? startItemOffsetY : startItemOffsetY - GetY(ScrollOffset);
        double rowHeight = 0;
        var rowChilds = new List<IItemContainerInfo>();
        var childSizes = new List<Size>();

        foreach (var child in itemContainerManager.RealizedContainers
            .OrderBy(container => itemContainerManager.FindItemIndexOfContainer(container)))
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

    public Size GetAverageItemSize()
    {
        if (FixedItemSize != Size.Empty)
        {
            return FixedItemSize;
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

    public void BringIndexIntoView(int itemIndex)
    {
        if (itemIndex < 0 || itemIndex >= items.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(itemIndex), $"The argument {nameof(itemIndex)} must be >= 0 and < the count of items.");
        }

        var itemOffset = FindItemOffset(itemIndex);

        if (GetY(itemOffset) < GetY(ScrollOffset)
            || GetY(itemOffset) + GetHeight(GetAssumedItemSize(items[itemIndex])) > GetY(ScrollOffset))
        {
            if (Orientation == Orientation.Horizontal)
            {
                SetVerticalOffset(GetY(itemOffset));
            }
            else
            {
                SetHorizontalOffset(GetY(itemOffset));
            }
        }
    }

    private Point FindItemOffset(int itemIndex)
    {
        double x = 0, y = 0, rowHeight = 0;

        for (int i = 0; i <= itemIndex; i++)
        {
            Size itemSize = GetAssumedItemSize(items[i]);

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

        ViewportSize = availableSize;

        if (viewportChanged)
        {
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
        foreach (var item in items) // foreach seems to be faster than a for loop
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

            if (y + rowHeight >= startOffsetY)
            {
                if (CacheLengthUnit == VirtualizationCacheLengthUnit.Item)
                {
                    startItemIndex = Math.Max(indexOfFirstRowItem - (int)CacheLength.CacheBeforeViewport, 0);
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

        int newEndItemIndex = items.Count - 1;

        double endOffsetY = DetermineEndOffsetY();

        double x = startItemOffsetX;
        double y = startItemOffsetY;
        double rowHeight = 0;

        knownExtendX = 0;

        for (int itemIndex = startItemIndex, childIndex = 0; itemIndex < items.Count; itemIndex++, childIndex++)
        {
            if (itemIndex == 0)
            {
                sizeOfFirstItem = null;
            }

            object item = items[itemIndex];

            var container = itemContainerManager.Realize(itemIndex, out bool _, out bool isNewContainer);
            if (isNewContainer)
            {
                childrenCollection.AddChild(container);
            }

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

            if (newEndItemIndex == items.Count - 1)
            {
                if (!AllowDifferentSizedItems
                    && itemIndex + 1 < items.Count
                    && x + sizeOfFirstItem!.Value.Width > GetWidth(ViewportSize)
                    && y + rowHeight >= endOffsetY)
                {
                    newEndItemIndex = itemIndex;
                }
                else if (y >= endOffsetY)
                {
                    newEndItemIndex = itemIndex;
                }

                if (newEndItemIndex != items.Count - 1 && CacheLengthUnit == VirtualizationCacheLengthUnit.Item)
                {
                    newEndItemIndex = Math.Min(newEndItemIndex + (int)CacheLength.CacheAfterViewport, items.Count - 1);
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
        var containers = itemContainerManager.RealizedContainers.ToList();
        foreach (var container in containers)
        {
            int itemIndex = itemContainerManager.FindItemIndexOfContainer(container);

            if (itemIndex < startItemIndex)
            {
                Virtualize(container);
            }
        }
    }

    private void VirtualizeItemsAfterEndIndex()
    {
        var containers = itemContainerManager.RealizedContainers.ToList();
        foreach (var container in containers)
        {
            int itemIndex = itemContainerManager.FindItemIndexOfContainer(container);

            if (itemIndex > endItemIndex)
            {
                Virtualize(container);
            }
        }
    }

    private void Virtualize(IItemContainerInfo container)
    {
        if (itemContainerManager.Virtualize(container))
        {
            childrenCollection.RemoveChild(container);
        }
    }

    private void UpdateExtent(ref bool invalidateScrollInfo)
    {
        Size extent;

        if (!AllowDifferentSizedItems)
        {
            if (FixedItemSize != Size.Empty)
            {
                extent = CalculateExtentForSameSizeItems(FixedItemSize);
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
                double estimatedExtend = ((double)items.Count / itemsInKnownExtend) * knownExtendY;
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
        double extentY = Math.Ceiling(((double)items.Count) / itemsPerRow) * GetHeight(itemSize);
        return CreateSize(knownExtendX, extentY);
    }

    private double DetermineStartOffsetY()
    {
        double cacheLength = 0;

        if (CacheLengthUnit == VirtualizationCacheLengthUnit.Page)
        {
            cacheLength = CacheLength.CacheBeforeViewport * GetHeight(ViewportSize);
        }
        else if (CacheLengthUnit == VirtualizationCacheLengthUnit.Pixel)
        {
            cacheLength = CacheLength.CacheBeforeViewport;
        }

        return Math.Max(GetY(ScrollOffset) - cacheLength, 0);
    }

    private double DetermineEndOffsetY()
    {
        double cacheLength = 0;

        if (CacheLengthUnit == VirtualizationCacheLengthUnit.Page)
        {
            cacheLength = CacheLength.CacheAfterViewport * GetHeight(ViewportSize);
        }
        else if (CacheLengthUnit == VirtualizationCacheLengthUnit.Pixel)
        {
            cacheLength = CacheLength.CacheAfterViewport;
        }

        return Math.Max(GetY(ScrollOffset), 0) + GetHeight(ViewportSize) + cacheLength;
    }

    private Size? GetUpfrontKnownItemSize(object item)
    {
        if (FixedItemSize != Size.Empty)
        {
            return FixedItemSize;
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
            double summedUpChildWidth = childSizes.Sum(childSize => GetWidth(childSize));
            double unusedWidth = rowWidth - summedUpChildWidth;
            extraWidth = unusedWidth / children.Count;
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

    protected double GetX(Point point) => Orientation == Orientation.Horizontal ? point.X : point.Y;
    protected double GetY(Point point) => Orientation == Orientation.Horizontal ? point.Y : point.X;
    protected double GetWidth(Size size) => Orientation == Orientation.Horizontal ? size.Width : size.Height;
    protected double GetHeight(Size size) => Orientation == Orientation.Horizontal ? size.Height : size.Width;
    protected Point CreatePoint(double x, double y) => Orientation == Orientation.Horizontal ? new Point(x, y) : new Point(y, x);
    protected Size CreateSize(double width, double height) => Orientation == Orientation.Horizontal ? new Size(width, height) : new Size(height, width);
    protected Rect CreateRect(double x, double y, double width, double height) => Orientation == Orientation.Horizontal ? new Rect(x, y, width, height) : new Rect(y, x, height, width);

    #endregion
}

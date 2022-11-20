using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace WpfToolkit.Controls;

internal class VirtualizingWrapPanelModel
{
    public Size Extent { get; private set; } = new Size(0, 0);
    public Size ViewportSize { get; private set; } = new Size(0, 0);
    public Point ScrollOffset { get; private set; } = new Point(0, 0);
    public VirtualizationCacheLength CacheLength { get; set; }
    public VirtualizationCacheLengthUnit CacheLengthUnit { get; set; }
    public Orientation Orientation { get; set; }

    public Size FixedItemSize { get; set; } = Size.Empty;
    public IItemSizeProvider? ItemSizeProvider { get; set; } = null;
    public bool AllowDifferentSizedItems { get; set; } = false;

    public bool StretchItems { get; set; }
    public SpacingMode SpacingMode { get; set; }
    public IReadOnlyList<object> Items { get; set; } = new List<object>();

    private Size? firstRealizedItemSize;
    private Dictionary<object, Size> itemSizesCache = new Dictionary<object, Size>();
    private Dictionary<object, Size> itemSizesCacheForProvider = new Dictionary<object, Size>();

    private int itemsInKnownExtend = 0;

    private int startItemIndex = -1;
    private int endItemIndex = -1;

    private double startItemOffsetY = 0;

    private double knownExtendX = 0;
    private double knownExtendY = 0;

    private readonly IItemContainerManger itemContainerManager;

    private static int i = 0;
    private int instance;

    public VirtualizingWrapPanelModel(IItemContainerManger itemContainerManager)
    {
        instance = i++;
        Debug.WriteLine("VirtualizingWrapPanelModel " + instance);
        this.itemContainerManager = itemContainerManager;
    }

    public Size OnMeasure(Size availableSize, Size viewportSize, Point scrollOffset)
    {
        if (scrollOffset.X < 0 || scrollOffset.Y < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(scrollOffset));
        }

        ScrollOffset = scrollOffset;
        UpdateViewport(viewportSize);
        FindStartIndexAndOffset();
        VirtualizeItemsBeforeStartIndex();
        RealizeItemsAndFindEndIndex();
        VirtualizeItemsAfterEndIndex();
        UpdateExtent();
        double desiredWidth = GetWidth(viewportSize);
        double desiredHeight = Math.Min(GetHeight(availableSize), GetHeight(Extent));
        return CreateSize(desiredWidth, desiredHeight);
    }

    public Size OnArrange(Size finalSize, Point scrollOffset)
    {
        double x = GetX(scrollOffset);
        double y = startItemOffsetY - GetY(scrollOffset);
        double rowHeight = 0;
        var rowChilds = new List<IArrangeable>();
        var childSizes = new List<Size>();

        var realizedContainers = itemContainerManager.RealizedContainers;
        for (int childIndex = 0; childIndex < realizedContainers.Count; childIndex++)
        {
            IArrangeable child = realizedContainers[childIndex];

            int itemIndex = itemContainerManager.ItemIndexForChildIndex(childIndex);
            Size? upfrontKnownItemSize = GetUpfrontKnownItemSize(itemIndex);

            Size childSize = upfrontKnownItemSize ?? child.DesiredSize;

            if (x + GetWidth(childSize) > GetWidth(finalSize))
            {
                ArrangeRow(GetWidth(finalSize), rowChilds, childSizes, y);
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
            ArrangeRow(GetWidth(finalSize), rowChilds, childSizes, y);
        }

        return finalSize;
    }

    public Size GetAverageItemSize()
    {
        if (FixedItemSize != Size.Empty)
        {
            return FixedItemSize;
        }
        var itemSizes = itemSizesCache.Select(entry => entry.Value).ToList();
        return new Size(itemSizes.Average(size => size.Width), itemSizes.Average(size => size.Height));
    }

    public void OnItemsChanged(NotifyCollectionChangedAction action)
    {
        if (action == NotifyCollectionChangedAction.Remove
            || action == NotifyCollectionChangedAction.Replace
            || action == NotifyCollectionChangedAction.Reset)
        {
            var removedCachedItems = itemSizesCache.Keys.Except(Items).ToList(); // TODO test performance

            foreach (var removedItem in removedCachedItems)
            {
                itemSizesCache.Remove(removedItem);
            }
        }

        int itemsInKnownExtend = Items.TakeWhile(item => itemSizesCache.ContainsKey(item)).Count();

        if (itemsInKnownExtend != this.itemsInKnownExtend)
        {
            RecalulateKnownExtend();
        }
    }

    public double FindOffsetOfItem(int itemIndex)
    {
        double x = 0, y = 0, rowHeight = 0;

        for (int i = 0; i <= itemIndex; i++)
        {
            Size itemSize = GetItemSize(i);

            if (x + GetWidth(itemSize) > GetWidth(ViewportSize))
            {
                x = 0;
                y += rowHeight;
                rowHeight = 0;
            }

            x += GetWidth(itemSize);
            rowHeight = Math.Max(rowHeight, GetHeight(itemSize));
        }

        return y;
    }

    private void RecalulateKnownExtend()
    {
        double x = 0, y = 0, rowHeight = 0;
        int itemCount = 0;
        foreach (var entry in itemSizesCache)
        {
            Size itemSize = entry.Value;
            if (x + GetWidth(itemSize) > GetWidth(ViewportSize) && x != 0)
            {
                x = 0;
                y += rowHeight;
                rowHeight = 0;
            }
            x += GetWidth(itemSize);
            rowHeight = Math.Max(rowHeight, GetHeight(itemSize));
            itemCount++;
        }
        knownExtendY = y + rowHeight;
        itemsInKnownExtend = itemCount;
    }

    private void UpdateViewport(Size availableSize)
    {
        bool viewportChanged = availableSize != ViewportSize;

        ViewportSize = availableSize;

        if (viewportChanged)
        {
            RecalulateKnownExtend();
        }
    }

    private void FindStartIndexAndOffset()
    {
        if (GetY(ScrollOffset) == 0)
        {
            startItemIndex = 0;
            startItemOffsetY = 0;
            return;
        }

        double startOffsetY = DetermineStartOffsetY();
        double x = 0, y = 0, rowHeight = 0;
        int indexOfFirstRowItem = 0;

        for (int itemIndex = 0; itemIndex < Items.Count; itemIndex++)
        {
            Size itemSize = GetItemSize(itemIndex);

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
                    startItemOffsetY = FindOffsetOfItem(startItemIndex);
                }
                else
                {
                    startItemIndex = indexOfFirstRowItem;
                    startItemOffsetY = y;
                }
                break;
            }
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

        double x = 0;
        double y = startItemOffsetY;
        double rowHeight = 0;

        knownExtendX = 0;

        for (int itemIndex = startItemIndex, childIndex = 0; itemIndex < Items.Count; itemIndex++, childIndex++)
        {
            object item = Items[itemIndex];

            if (itemIndex == 0) 
            {
                firstRealizedItemSize = null;
            }

            Size? upfrontKnownItemSize = GetUpfrontKnownItemSize(itemIndex);
            Size availableSize = upfrontKnownItemSize ?? new Size(double.PositiveInfinity, double.PositiveInfinity);
            Size desiredChildSize = itemContainerManager.RealizeItem(itemIndex, childIndex, availableSize);
            Size itemSize = upfrontKnownItemSize ?? desiredChildSize;

            itemSizesCache[item] = itemSize;

            if (firstRealizedItemSize == null || itemIndex == 0) 
            {
                firstRealizedItemSize = itemSize;
            }

            if (x + GetWidth(itemSize) > GetWidth(ViewportSize) && x != 0)
            {
                knownExtendX = Math.Max(x, knownExtendX);
                x = 0;
                y += rowHeight;
                rowHeight = 0;
            }

            x += GetWidth(itemSize);
            rowHeight = Math.Max(rowHeight, GetHeight(itemSize));

            if (newEndItemIndex == Items.Count - 1 && y >= endOffsetY)
            {
                if (CacheLengthUnit != VirtualizationCacheLengthUnit.Item)
                {
                    newEndItemIndex = itemIndex - 1;
                    itemContainerManager.VirtualizeItem(childIndex);
                }
                else
                {
                    newEndItemIndex = Math.Min(itemIndex - 1 + (int)CacheLength.CacheAfterViewport, Items.Count - 1);
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

    private void VirtualizeItemsBeforeStartIndex()
    {
        for (int childIndex = itemContainerManager.RealizedContainers.Count - 1; childIndex >= 0; childIndex--)
        {
            int itemIndex = itemContainerManager.ItemIndexForChildIndex(childIndex);

            if (itemIndex < startItemIndex)
            {
                itemContainerManager.VirtualizeItem(childIndex);
            }
        }
    }

    private void VirtualizeItemsAfterEndIndex()
    {
        for (int childIndex = itemContainerManager.RealizedContainers.Count - 1; childIndex >= 0; childIndex--)
        {
            int itemIndex = itemContainerManager.ItemIndexForChildIndex(childIndex);

            if (itemIndex > endItemIndex)
            {
                itemContainerManager.VirtualizeItem(childIndex);
            }
        }
    }

    private void UpdateExtent()
    {
        if (!AllowDifferentSizedItems)
        {
            if (FixedItemSize != Size.Empty)
            {
                Extent = CalculateExtentForSameSizeItems(FixedItemSize);
                return;
            }
            if (firstRealizedItemSize != null)
            {
                Extent = CalculateExtentForSameSizeItems(firstRealizedItemSize.Value);
                return;
            }
        }

        if (itemsInKnownExtend == 0)
        {
            Extent = CalculateExtentForSameSizeItems(new Size(16, 16)); // TODO using min width/height
        }
        else
        {
            double estimatedExtend = ((double)Items.Count / itemsInKnownExtend) * knownExtendY;
            Extent = CreateSize(knownExtendX, estimatedExtend);
        }

        // TODO currently set in VirtualizingPanelBaseV2
        //if (GetY(ScrollOffset) + GetHeight(ViewportSize) > GetHeight(Extent))
        //{
        //    ScrollOffset = CreatePoint(GetX(ScrollOffset), GetHeight(Extent) - GetHeight(ViewportSize));
        //}
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

        return GetY(ScrollOffset) + GetHeight(ViewportSize) + cacheLength;
    }

    private Size? GetUpfrontKnownItemSize(int itemIndex)
    {
        if (FixedItemSize != Size.Empty)
        {
            return FixedItemSize;
        }
        if (ItemSizeProvider != null)
        {
            var item = Items[itemIndex];
            if (!itemSizesCacheForProvider.TryGetValue(item, out Size size))
            {
                size = ItemSizeProvider.GetSizeForItem(item);
                itemSizesCacheForProvider.Add(item, size);
            }
            return size;
        }
        if (!AllowDifferentSizedItems && firstRealizedItemSize != null) 
        {
            return firstRealizedItemSize;
        }
        return null;
    }

    private Size GetItemSize(int itemIndex)
    {
        var item = Items[itemIndex];

        if (GetUpfrontKnownItemSize(itemIndex) is Size upfrontKnownItemSize)
        {
            return upfrontKnownItemSize;
        }

        if (itemSizesCache.TryGetValue(item, out Size cachedItemSize))
        {
            return cachedItemSize;
        }

        Size itemSize = MeasureItemTempoary(itemIndex);
        itemSizesCache.Add(item, itemSize);
        return itemSize;
    }

    private Size MeasureItemTempoary(int itemIndex)
    {
        Size availableSize = new Size(double.PositiveInfinity, double.PositiveInfinity);
        Size itemSize = itemContainerManager.RealizeItem(itemIndex, 0, availableSize);
        itemContainerManager.VirtualizeItem(0);
        return itemSize;
    }

    private void ArrangeRow(double rowWidth, List<IArrangeable> children, List<Size> childSizes, double y)
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

        double x = outerSpacing;

        for (int i = 0; i < children.Count; i++)
        {
            var child = children[i];
            Size childSize = childSizes[i];
            child.Arrange(CreateRect(x, y, GetWidth(childSize) + extraWidth, GetHeight(childSize)));
            x += GetWidth(childSize) + extraWidth + innerSpacing;
        }
    }

    private void CalculateRowSpacing(double rowWidth, List<IArrangeable> children, List<Size> childSizes, out double innerSpacing, out double outerSpacing)
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
            childCount = (int)Math.Floor(rowWidth / GetWidth(firstRealizedItemSize!.Value));
            summedUpChildWidth = childCount * GetWidth(firstRealizedItemSize.Value);
        }

        double unusedWidth = rowWidth - summedUpChildWidth;

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

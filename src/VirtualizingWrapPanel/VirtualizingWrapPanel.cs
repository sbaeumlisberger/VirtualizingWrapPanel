using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace WpfToolkit.Controls;

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

    protected override bool HasLogicalOrientation => true;

    protected override Orientation LogicalOrientation => Orientation == Orientation.Horizontal ? Orientation.Vertical : Orientation.Horizontal;

    private static readonly Size InfiniteSize = new Size(double.PositiveInfinity, double.PositiveInfinity);

    private double scrollOffsetMainAxis;
    private double scrollOffsetCrossAxis;

    private double viewportSizeMainAxis;
    private double viewportSizeCrossAxis;

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

    private Visibility previousCrossAxisScrollBarVisibiliy = Visibility.Collapsed;
    private double crossAxisScrollBarSizeOnMainAxis = 0;

    private readonly List<FrameworkElement> realizedContainers = [];

    // local fields to cache frequently read properties
    private Orientation orientation = Orientation.Horizontal;
    private Size itemSize = Size.Empty;
    private IItemSizeProvider? itemSizeProvider;
    private ReadOnlyCollection<object> items = new ReadOnlyCollection<object>([]);
    private int itemsCount = 0;
    private Size fallbackItemSize = new Size(64, 64);

    public void ClearItemSizeCache()
    {
        itemSizesCache = AllowDifferentSizedItems ? Utils.NewUninitializedList<Size>(Items.Count) : [];
    }

    #region event handlers

    protected override void OnClearChildren()
    {
        RemoveInternalChildRange(0, InternalChildren.Count);
        realizedContainers.Clear();
    }

    protected override void OnItemsChanged(object sender, ItemsChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Reset)
        {
            realizedContainers.Clear();
        }

        if (bringIntoViewItemIndex >= Items.Count)
        {
            bringIntoViewItemIndex = -1;
            bringIntoViewContainer = null;
        }

        if (AllowDifferentSizedItems)
        {
            UpdateItemSizesCache(e);
        }
    }

    private void UpdateItemSizesCache(ItemsChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Remove:
                int removeIndex = IndexFromGeneratorPosition(e.Position);
                itemSizesCache.RemoveAt(removeIndex);
                break;
            case NotifyCollectionChangedAction.Replace:
                int replaceIndex = IndexFromGeneratorPosition(e.Position);
                itemSizesCache[replaceIndex] = default;
                break;
            case NotifyCollectionChangedAction.Add:
                int addIndex = IndexFromGeneratorPosition(e.Position);
                itemSizesCache.Insert(addIndex, default);
                break;
            case NotifyCollectionChangedAction.Move:
                int oldIndex = IndexFromGeneratorPosition(e.OldPosition);
                int newIndex = IndexFromGeneratorPosition(e.Position);
                var itemSize = itemSizesCache[oldIndex];
                itemSizesCache.RemoveAt(oldIndex);
                itemSizesCache.Insert(newIndex, itemSize);
                break;
            case NotifyCollectionChangedAction.Reset:
                itemSizesCache = Utils.NewUninitializedList<Size>(Items.Count);
                if (ItemSizeProvider is null)
                {
                    SetHorizontalOffset(0);
                    SetVerticalOffset(0);
                }
                break;
            default:
                throw new NotSupportedException("The action " + e.Action + " is not supported.");
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

    #endregion

    #region measure

    protected override Size MeasureOverride(Size availableSize)
    {
        VerifyItemsControl();

        items = Items;
        itemsCount = items.Count;

        MeasureBringIntoViewContainer();

        UpdateViewport(availableSize);
        UpdateCacheProperties();
        RealizeAndVirtualizeItems();
        UpdateExtent();
        EnsureValidScrollOffset();

        Size desiredSize = CalculateDesiredSize(availableSize);

        return desiredSize;
    }

    private void MeasureBringIntoViewContainer()
    {
        if (bringIntoViewContainer is not null && !bringIntoViewContainer.IsMeasureValid)
        {
            var upfrontKnownItemSize = GetUpfrontKnownItemSizeOrEmpty(bringIntoViewItemIndex);
            bringIntoViewContainer.Measure(!upfrontKnownItemSize.IsEmpty ? upfrontKnownItemSize : InfiniteSize);

            if (!AllowDifferentSizedItems && sizeOfFirstItem.IsEmpty)
            {
                sizeOfFirstItem = bringIntoViewContainer.DesiredSize;
            }
        }
    }

    private void UpdateViewport(Size availableSize)
    {
        double newViewportSizeMainAxis;
        double newViewportSizeCrossAxis;

        if (ItemsOwner is IHierarchicalVirtualizationAndScrollInfo groupItem)
        {
            Rect viewport = GetViewportFromGroupItem(groupItem);

            HorizontalOffset = viewport.X;
            VerticalOffset = viewport.Y;

            scrollOffsetMainAxis = GetPositionOnMainAxis(viewport.Location);
            scrollOffsetCrossAxis = GetPositionOnCrossAxis(viewport.Location);
            newViewportSizeMainAxis = GetSizeOnMainAxis(viewport.Size);
            newViewportSizeCrossAxis = GetSizeOnCrossAxis(viewport.Size);
        }
        else
        {
            scrollOffsetMainAxis = orientation == Orientation.Horizontal ? HorizontalOffset : VerticalOffset;
            scrollOffsetCrossAxis = orientation == Orientation.Horizontal ? VerticalOffset : HorizontalOffset;
            newViewportSizeMainAxis = GetSizeOnMainAxis(availableSize);
            newViewportSizeCrossAxis = GetSizeOnCrossAxis(availableSize);
        }

        if (GetScrollOwner() is { } scrollOwner && GetCrossAxisScrollBarVisibility(scrollOwner) == ScrollBarVisibility.Auto)
        {
            var computedCrossAxisScrollBarVisibility = GetComputedCrossAxisScrollBarVisibility(scrollOwner);

            if (previousCrossAxisScrollBarVisibiliy == Visibility.Collapsed
                && computedCrossAxisScrollBarVisibility == Visibility.Visible)
            {
                crossAxisScrollBarSizeOnMainAxis = viewportSizeMainAxis - newViewportSizeMainAxis;
            }

            if (previousCrossAxisScrollBarVisibiliy == Visibility.Visible
                && computedCrossAxisScrollBarVisibility == Visibility.Collapsed
                && Utils.ApproximatelyEquals(newViewportSizeMainAxis - crossAxisScrollBarSizeOnMainAxis, viewportSizeMainAxis)
                && newViewportSizeCrossAxis == viewportSizeCrossAxis)
            {
                newViewportSizeMainAxis = viewportSizeMainAxis;
            }
            else
            {
                previousCrossAxisScrollBarVisibiliy = computedCrossAxisScrollBarVisibility;
            }
        }

        viewportSizeMainAxis = newViewportSizeMainAxis;
        viewportSizeCrossAxis = newViewportSizeCrossAxis;

        Size newViewportSize = CreateSize(newViewportSizeMainAxis, newViewportSizeCrossAxis);

        if (newViewportSize.Width != ViewportWidth || newViewportSize.Height != ViewportHeight)
        {
            ViewportWidth = newViewportSize.Width;
            ViewportHeight = newViewportSize.Height;
            InvalidateScrollInfo();
        }
    }

    private void UpdateCacheProperties()
    {
        if (ItemsOwner is IHierarchicalVirtualizationAndScrollInfo groupItem)
        {
            cacheLength = groupItem.Constraints.CacheLength;
            cacheLengthUnit = groupItem.Constraints.CacheLengthUnit;
        }
        else
        {
            cacheLength = GetCacheLength(ItemsOwner);
            cacheLengthUnit = GetCacheLengthUnit(ItemsOwner);
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

        DisconnectRecycledContainers();
    }

    private void RealizeAndVirtualizeItems_DifferentSizedItems()
    {
        if (itemSizesCache.Count != Items.Count) // panel is resued in grouping scenario
        {
            itemSizesCache = Utils.NewUninitializedList<Size>(Items.Count);
        }

        FindStartIndexAndOffset_DifferentSizedItems();
        VirtualizeItems(); // before new start index
        RealizeItemsAndFindEndIndex_DifferentSizedItems();
        VirtualizeItems(); // after new end index
    }

    private void FindStartIndexAndOffset_DifferentSizedItems()
    {
        if (ViewportWidth == 0 && ViewportHeight == 0)
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
        startItemOffsetMainAxis = 0;
        startItemOffsetCrossAxis = 0;

        double offsetMainAxis = 0, offsetCrossAxis = 0, lineSizeCrossAxis = 0;
        int indexOfFirstRowItem = 0;

        for (int itemIndex = 0; itemIndex < itemsCount; itemIndex++)
        {
            Size itemSize = GetItemSizeOrFallback(itemIndex);
            double itemSizeOnMainAxis = GetSizeOnMainAxis(itemSize);
            double itemSizeOnCrossAxis = GetSizeOnCrossAxis(itemSize);

            if (offsetMainAxis + itemSizeOnMainAxis > viewportSizeMainAxis && offsetMainAxis != 0)
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
    }

    private void RealizeItemsAndFindEndIndex_DifferentSizedItems()
    {
        if (startItemIndex == -1)
        {
            endItemIndex = -1;
            return;
        }

        int newEndItemIndex = itemsCount - 1;
        bool endItemIndexFound = false;

        double endOffsetCrossAxis = CalculateEndOffsetCrossAxis();

        double offsetMainAxis = startItemOffsetMainAxis;
        double offsetCrossAxis = startItemOffsetCrossAxis;
        double lineSizeCrossAxis = 0;

        for (int itemIndex = startItemIndex; itemIndex <= newEndItemIndex; itemIndex++)
        {
            var container = Realize(itemIndex);

            if (container == bringIntoViewContainer && IsInsideViewport(bringIntoViewContainer))
            {
                bringIntoViewItemIndex = -1;
                bringIntoViewContainer = null;
            }

            Size upfrontKnownItemSize = GetUpfrontKnownItemSizeOrEmpty(itemIndex);

            if (!container.IsMeasureValid)
            {
                container.Measure(!upfrontKnownItemSize.IsEmpty ? upfrontKnownItemSize : InfiniteSize);
            }

            Size containerSize = !upfrontKnownItemSize.IsEmpty ? upfrontKnownItemSize : container.DesiredSize;
            itemSizesCache[itemIndex] = containerSize;

            if (offsetMainAxis != 0 && offsetMainAxis + GetSizeOnMainAxis(containerSize) > viewportSizeMainAxis)
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
                        newEndItemIndex = Math.Min(newEndItemIndex + (int)cacheLength.CacheAfterViewport, itemsCount - 1);
                        // loop continues until newEndItemIndex is reached
                    }
                }
            }
        }

        endItemIndex = newEndItemIndex;
    }

    private void RealizeAndVirtualizeItems_UniformSizedItems()
    {
        CalculateItemRange_UniformSizedItems();
        VirtualizeItems();
        RealizeItems_UniformSizedItems();
    }

    private void CalculateItemRange_UniformSizedItems()
    {
        if (itemsCount == 0)
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
            var itemContainer = Realize(0);
            itemContainer.Measure(InfiniteSize);
            sizeOfFirstItem = itemContainer.DesiredSize;
            uniformItemSize = sizeOfFirstItem;
        }

        double itemSizeOnMainAxis = GetSizeOnMainAxis(uniformItemSize);
        double itemSizeOnCrossAxis = GetSizeOnCrossAxis(uniformItemSize);

        double startPositionOnCrossAxis = CalculateStartOffsetCrossAxis();
        double endPositonOnCrossAxis = CalculateEndOffsetCrossAxis();

        int itemsPerLine = Math.Max((int)Math.Floor(viewportSizeMainAxis / itemSizeOnMainAxis), 1);

        int startLine = (int)Math.Floor(startPositionOnCrossAxis / itemSizeOnCrossAxis);
        startItemIndex = startLine * itemsPerLine;

        int lineAfterEnd = (int)Math.Ceiling(endPositonOnCrossAxis / itemSizeOnCrossAxis);
        endItemIndex = Math.Clamp(lineAfterEnd * itemsPerLine - 1, 0, itemsCount - 1);

        if (cacheLengthUnit == VirtualizationCacheLengthUnit.Item)
        {
            startItemIndex = Math.Max(startItemIndex - (int)cacheLength.CacheBeforeViewport, 0);
            endItemIndex = Math.Min(endItemIndex + (int)cacheLength.CacheAfterViewport, itemsCount - 1);
        }

        startItemOffsetMainAxis = (startItemIndex % itemsPerLine) * itemSizeOnMainAxis;
        startItemOffsetCrossAxis = (startItemIndex / itemsPerLine) * itemSizeOnCrossAxis;
    }

    private void RealizeItems_UniformSizedItems()
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

            var container = Realize(itemIndex);

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

    private void VirtualizeItems()
    {
        keptRealizedFocusedContainer = null;

        foreach (var container in realizedContainers.ToList())
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

            var itemIndex = IndexFromContainer(container);

            if (itemIndex < startItemIndex || itemIndex > endItemIndex)
            {
                Virtualize(container);
            }
        }
    }

    /// <summary>
    /// Disconnects recycled containers that were not reused from the visual tree 
    /// so that they do not interfere with Arrange, keyboard navigation, etc.
    /// </summary>
    private void DisconnectRecycledContainers()
    {
        var children = InternalChildren;
        for (int i = children.Count - 1; i >= 0; i--)
        {
            if (!realizedContainers.Contains(children[i]))
            {
                RemoveInternalChildRange(i, 1);
            }
        }
    }

    private void UpdateExtent()
    {
        Size extent = AllowDifferentSizedItems
            ? CalculateExtend_DifferentSizedItems()
            : CalculateExtend_UniformSizedItems();

        if (extent.Width != ExtentWidth || extent.Height != ExtentHeight)
        {
            ExtentWidth = extent.Width;
            ExtentHeight = extent.Height;
            InvalidateScrollInfo();
        }
    }

    private Size CalculateExtend_DifferentSizedItems()
    {
        double extentMainAxis = 0;
        double extentCrossAxis = 0;

        double lineSizeMainAxis = 0;
        double lineSizeCrossAxis = 0;

        for (int itemIndex = 0; itemIndex < itemsCount; itemIndex++)
        {
            var itemSize = GetItemSizeOrFallback(itemIndex);
            double itemSizeOnMainAxis = GetSizeOnMainAxis(itemSize);
            double itemSizeOnCrossAxis = GetSizeOnCrossAxis(itemSize);

            if (lineSizeMainAxis != 0 && lineSizeMainAxis + itemSizeOnMainAxis > viewportSizeMainAxis)
            {
                extentMainAxis = Math.Max(extentMainAxis, lineSizeMainAxis);
                extentCrossAxis += lineSizeCrossAxis;

                lineSizeMainAxis = 0;
                lineSizeCrossAxis = 0;
            }

            lineSizeMainAxis += itemSizeOnMainAxis;
            lineSizeCrossAxis = Math.Max(lineSizeCrossAxis, itemSizeOnCrossAxis);
        }

        extentMainAxis = Math.Max(extentMainAxis, lineSizeMainAxis);
        extentCrossAxis += lineSizeCrossAxis;

        return CreateSize(extentMainAxis, extentCrossAxis);
    }

    private Size CalculateExtend_UniformSizedItems()
    {
        if (itemsCount == 0)
        {
            return new Size(0, 0);
        }

        Size uniformItemSize = !itemSize.IsEmpty ? itemSize : sizeOfFirstItem;
        double itemSizeOnMainAxis = GetSizeOnMainAxis(uniformItemSize);
        double itemSizeOnCrossAxis = GetSizeOnCrossAxis(uniformItemSize);

        int itemsPerLine = Math.Max((int)Math.Floor(viewportSizeMainAxis / itemSizeOnMainAxis), 1);
        int lineCount = (int)Math.Ceiling(itemsCount / (double)itemsPerLine);

        double extentMainAxis = Math.Min(itemsPerLine, itemsCount) * itemSizeOnMainAxis;
        double extentCrossAxis = lineCount * itemSizeOnCrossAxis;

        return CreateSize(extentMainAxis, extentCrossAxis);
    }

    private void EnsureValidScrollOffset()
    {
        if (ItemsOwner is IHierarchicalVirtualizationAndScrollInfo)
        {
            // scroll offset is managed by parent
            return;
        }

        if (ExtentWidth > ViewportWidth && HorizontalOffset + ViewportWidth > ExtentWidth)
        {
            HorizontalOffset = ExtentWidth - ViewportWidth;
            scrollOffsetMainAxis = orientation == Orientation.Horizontal ? HorizontalOffset : VerticalOffset;
            scrollOffsetCrossAxis = orientation == Orientation.Horizontal ? VerticalOffset : HorizontalOffset;
            RealizeAndVirtualizeItems();
            InvalidateScrollInfo();
        }

        if (ExtentHeight > ViewportHeight && VerticalOffset + ViewportHeight > ExtentHeight)
        {
            VerticalOffset = ExtentHeight - ViewportHeight;
            scrollOffsetMainAxis = orientation == Orientation.Horizontal ? HorizontalOffset : VerticalOffset;
            scrollOffsetCrossAxis = orientation == Orientation.Horizontal ? VerticalOffset : HorizontalOffset;
            RealizeAndVirtualizeItems();
            InvalidateScrollInfo();
        }
    }

    private Size CalculateDesiredSize(Size availableSize)
    {
        double desiredWidth = Math.Min(availableSize.Width, ExtentWidth);
        double desiredHeight = Math.Min(availableSize.Height, ExtentHeight);
        return new Size(desiredWidth, desiredHeight);
    }

    private double CalculateStartOffsetCrossAxis()
    {
        double cache = 0;

        if (cacheLengthUnit == VirtualizationCacheLengthUnit.Page)
        {
            cache = cacheLength.CacheAfterViewport * viewportSizeCrossAxis;
        }
        else if (cacheLengthUnit == VirtualizationCacheLengthUnit.Pixel)
        {
            cache = cacheLength.CacheAfterViewport;
        }

        return Math.Max(scrollOffsetCrossAxis - cache, 0);
    }

    private double CalculateEndOffsetCrossAxis()
    {
        double cache = 0;

        if (cacheLengthUnit == VirtualizationCacheLengthUnit.Page)
        {
            cache = cacheLength.CacheAfterViewport * viewportSizeCrossAxis;
        }
        else if (cacheLengthUnit == VirtualizationCacheLengthUnit.Pixel
            && !double.IsInfinity(cacheLength.CacheAfterViewport)) // ?
        {
            cache = cacheLength.CacheAfterViewport;
        }

        return scrollOffsetCrossAxis + viewportSizeCrossAxis + cache;
    }

    private Size GetUpfrontKnownItemSizeOrEmpty(int itemIndex)
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
            return itemSizeProvider.GetSizeForItem(items[itemIndex]);
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

    #endregion

    #region arrange

    protected override Size ArrangeOverride(Size finalSize)
    {
        ArrangeBringIntoViewContainer();
        ArrangeKeptRealizedFocusedContainer();

        if (startItemIndex == -1)
        {
            return finalSize;
        }

        bool hierarchical = ItemsOwner is IHierarchicalVirtualizationAndScrollInfo;
        double finalSizeMainAxis = GetSizeOnMainAxis(finalSize);
        double offsetMainAxis = startItemOffsetMainAxis + scrollOffsetMainAxis;
        double offsetCrossAxis = hierarchical ? startItemOffsetCrossAxis : startItemOffsetCrossAxis - scrollOffsetCrossAxis;
        double lineSizeCrossAxis = 0;
        var lineChilds = new List<UIElement>();
        var childSizes = new List<Size>();

        for (int index = startItemIndex; index <= endItemIndex; index++)
        {
            var child = ContainerFromIndex(index);

            Size upfrontKnownItemSize = GetUpfrontKnownItemSizeOrEmpty(index);

            Size childSize = !upfrontKnownItemSize.IsEmpty ? upfrontKnownItemSize : child.DesiredSize;

            if (lineChilds.Count > 0 && offsetMainAxis + GetSizeOnMainAxis(childSize) > finalSizeMainAxis)
            {
                ArrangeLine(finalSizeMainAxis, lineChilds, childSizes, offsetCrossAxis, hierarchical);
                offsetMainAxis = 0;
                offsetCrossAxis += lineSizeCrossAxis;
                lineSizeCrossAxis = 0;
                lineChilds.Clear();
                childSizes.Clear();
            }

            offsetMainAxis += GetSizeOnMainAxis(childSize);
            lineSizeCrossAxis = Math.Max(lineSizeCrossAxis, GetSizeOnCrossAxis(childSize));
            lineChilds.Add(child);
            childSizes.Add(childSize);
        }

        if (lineChilds.Count > 0)
        {
            ArrangeLine(finalSizeMainAxis, lineChilds, childSizes, offsetCrossAxis, hierarchical);
        }

        return finalSize;
    }

    private void ArrangeBringIntoViewContainer()
    {
        if (bringIntoViewContainer is not null)
        {
            bool hierarchical = ItemsOwner is IHierarchicalVirtualizationAndScrollInfo;
            var itemOffset = FindItemOffset(bringIntoViewItemIndex);
            itemOffset = new Point(itemOffset.X - HorizontalOffset, hierarchical ? itemOffset.Y : itemOffset.Y - VerticalOffset);
            var upfrontKnownItemSize = GetUpfrontKnownItemSizeOrEmpty(bringIntoViewItemIndex);
            var size = !upfrontKnownItemSize.IsEmpty ? upfrontKnownItemSize : bringIntoViewContainer.DesiredSize;
            bringIntoViewContainer.Arrange(new Rect(itemOffset, size));

            if (AllowDifferentSizedItems && itemSizeProvider is null)
            {
                bringIntoViewContainer.BringIntoView();
            }
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
            CalculateSpacing(lineSizeMainAxis, children, spaceTakenMainAxis, out innerSpacing, out outerSpacing);
        }

        double positionMainAxis = (hierarchical ? 0 : -scrollOffsetMainAxis) + outerSpacing;

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

    private void CalculateSpacing(double availableSpace, List<UIElement> children, double spaceTakenByChilds, out double innerSpacing, out double outerSpacing)
    {
        int childCount;

        if (AllowDifferentSizedItems)
        {
            childCount = children.Count;
        }
        else
        {
            childCount = IsGridLayoutEnabled ? (int)Math.Max(1, Math.Floor(availableSpace / GetSizeOnMainAxis(sizeOfFirstItem))) : children.Count;
        }

        double unusedSpace = Math.Max(0, availableSpace - spaceTakenByChilds);

        switch (SpacingMode)
        {
            case SpacingMode.Uniform:
                innerSpacing = outerSpacing = unusedSpace / (childCount + 1);
                break;

            case SpacingMode.BetweenItemsOnly:
                innerSpacing = unusedSpace / Math.Max(childCount - 1, 1);
                outerSpacing = 0;
                break;

            case SpacingMode.StartAndEndOnly:
                innerSpacing = 0;
                outerSpacing = unusedSpace / 2;
                break;

            case SpacingMode.None:
            default:
                innerSpacing = 0;
                outerSpacing = 0;
                break;
        }
    }


    #endregion

    #region container management

    public FrameworkElement Realize(int itemIndex)
    {
        var item = Items[itemIndex];

        var container = (FrameworkElement)ItemContainerGenerator.ContainerFromIndex(itemIndex);

        if (container is not null)
        {
            return container;
        }

        var generatorPosition = RecyclingItemContainerGenerator.GeneratorPositionFromIndex(itemIndex);
        using (RecyclingItemContainerGenerator.StartAt(generatorPosition, GeneratorDirection.Forward))
        {
            container = (FrameworkElement)RecyclingItemContainerGenerator.GenerateNext(out bool isNewContainer);

            realizedContainers.Add(container);

            if (isNewContainer || !InternalChildren.Contains(container))
            {
                AddInternalChild(container);
            }

            RecyclingItemContainerGenerator.PrepareItemContainer(container);

            return container;
        }
    }

    public void Virtualize(FrameworkElement container)
    {
        var generatorPosition = GeneratorPositionFromContainer(container);

        // Index is -1 when the item is already virtualized (can happen when grouping)
        if (generatorPosition.Index != -1)
        {
            if (IsRecycling)
            {
                RecyclingItemContainerGenerator.Recycle(generatorPosition, 1);
            }
            else
            {
                RecyclingItemContainerGenerator.Remove(generatorPosition, 1);
            }
        }

        realizedContainers.Remove(container);

        if (!IsRecycling)
        {
            RemoveInternalChildRange(InternalChildren.IndexOf(container), 1);
        }
    }

    private GeneratorPosition GeneratorPositionFromContainer(FrameworkElement container)
    {
        int itemIndex = ItemContainerGenerator.IndexFromContainer(container);
        return RecyclingItemContainerGenerator.GeneratorPositionFromIndex(itemIndex);
    }

    public int IndexFromGeneratorPosition(GeneratorPosition generatorPosition)
    {
        return RecyclingItemContainerGenerator.IndexFromGeneratorPosition(generatorPosition);
    }

    public FrameworkElement ContainerFromIndex(int itemIndex)
    {
        return (FrameworkElement)ItemContainerGenerator.ContainerFromIndex(itemIndex);
    }

    public int IndexFromContainer(FrameworkElement container)
    {
        return ItemContainerGenerator.IndexFromContainer(container);
    }

    #endregion

    #region scrolling

    protected override void BringIndexIntoView(int index)
    {
        if (index < 0 || index >= Items.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index), $"The argument {nameof(index)} must be >= 0 and < the count of items.");
        }

        var container = Realize(index);

        bringIntoViewItemIndex = index;
        bringIntoViewContainer = container;

        InvalidateMeasure();
        UpdateLayout();

        container.BringIntoView();
    }

    protected override double GetLineUpScrollAmount()
    {
        return -Math.Min(GetItemSizeForScrolling().Height * ScrollLineDeltaItem, ViewportHeight);
    }

    protected override double GetLineDownScrollAmount()
    {
        return Math.Min(GetItemSizeForScrolling().Height * ScrollLineDeltaItem, ViewportHeight);
    }

    protected override double GetLineLeftScrollAmount()
    {
        return -Math.Min(GetItemSizeForScrolling().Width * ScrollLineDeltaItem, ViewportWidth);
    }

    protected override double GetLineRightScrollAmount()
    {
        return Math.Min(GetItemSizeForScrolling().Width * ScrollLineDeltaItem, ViewportWidth);
    }

    protected override double GetMouseWheelUpScrollAmount()
    {
        return -Math.Min(GetItemSizeForScrolling().Height * MouseWheelDeltaItem, ViewportHeight);
    }

    protected override double GetMouseWheelDownScrollAmount()
    {
        return Math.Min(GetItemSizeForScrolling().Height * MouseWheelDeltaItem, ViewportHeight);
    }

    protected override double GetMouseWheelLeftScrollAmount()
    {
        return -Math.Min(GetItemSizeForScrolling().Width * MouseWheelDeltaItem, ViewportWidth);
    }

    protected override double GetMouseWheelRightScrollAmount()
    {
        return Math.Min(GetItemSizeForScrolling().Width * MouseWheelDeltaItem, ViewportWidth);
    }

    protected override double GetPageUpScrollAmount()
    {
        return -ViewportHeight;
    }

    protected override double GetPageDownScrollAmount()
    {
        return ViewportHeight;
    }

    protected override double GetPageLeftScrollAmount()
    {
        return -ViewportWidth;
    }

    protected override double GetPageRightScrollAmount()
    {
        return ViewportWidth;
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

    #endregion

    #region helper

    private Point FindItemOffset(int itemIndex)
    {
        if (!AllowDifferentSizedItems)
        {
            Size uniformItemSize = !itemSize.IsEmpty ? itemSize : sizeOfFirstItem;
            var itemsPerLine = Math.Max((int)Math.Floor(viewportSizeMainAxis / GetSizeOnMainAxis(uniformItemSize)), 1);
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

                if (offsetMainAxis != 0 && offsetMainAxis + GetSizeOnMainAxis(itemSize) > viewportSizeMainAxis)
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

    private ScrollBarVisibility GetCrossAxisScrollBarVisibility(ScrollViewer scrollViewer)
    {
        return orientation == Orientation.Horizontal
            ? scrollViewer.VerticalScrollBarVisibility
            : scrollViewer.HorizontalScrollBarVisibility;
    }

    private Visibility GetComputedCrossAxisScrollBarVisibility(ScrollViewer scrollViewer)
    {
        return orientation == Orientation.Horizontal
            ? scrollViewer.ComputedVerticalScrollBarVisibility
            : scrollViewer.ComputedHorizontalScrollBarVisibility;
    }

    private ScrollViewer? GetScrollOwner()
    {
        if (ItemsOwner is GroupItem groupItem
            && VisualTreeHelper.GetParent(groupItem) is IScrollInfo parentScrollInfo
            && parentScrollInfo.ScrollOwner is { } parentScrollOwner)
        {
            return parentScrollOwner;
        }

        return ScrollOwner;
    }

    private bool IsInsideViewport(FrameworkElement container)
    {
        return container.TransformToAncestor(this)
            .TransformBounds(new Rect(0, 0, container.ActualWidth, container.ActualHeight))
            .IntersectsWith(new Rect(0, 0, ViewportWidth, ViewportHeight));
    }

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

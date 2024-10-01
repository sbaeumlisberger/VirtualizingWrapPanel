using System;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace WpfToolkit.Controls
{
    /// <summary>
    /// A implementation of a wrap panel that supports virtualization and can be used in horizontal and vertical orientation.
    /// In addition the panel allows to expand one specific item.
    /// <p class="note">In order to work properly all items must have the same size.</p>
    /// </summary>
    public class VirtualizingWrapPanelWithItemExpansion : VirtualizingPanelBase
    {
        private struct ItemRangeStruct
        {
            public int StartIndex { get; }
            public int EndIndex { get; }

            public ItemRangeStruct(int startIndex, int endIndex) : this()
            {
                StartIndex = startIndex;
                EndIndex = endIndex;
            }

            public bool Contains(int itemIndex)
            {
                return itemIndex >= StartIndex && itemIndex <= EndIndex;
            }
        }

        public static readonly DependencyProperty SpacingModeProperty = DependencyProperty.Register(nameof(SpacingMode), typeof(SpacingMode), typeof(VirtualizingWrapPanelWithItemExpansion), new FrameworkPropertyMetadata(SpacingMode.Uniform, FrameworkPropertyMetadataOptions.AffectsMeasure));

        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(nameof(Orientation), typeof(Orientation), typeof(VirtualizingWrapPanelWithItemExpansion), new FrameworkPropertyMetadata(Orientation.Vertical, FrameworkPropertyMetadataOptions.AffectsMeasure, (obj, args) => ((VirtualizingWrapPanelWithItemExpansion)obj).Orientation_Changed()));

        public static readonly DependencyProperty ItemSizeProperty = DependencyProperty.Register(nameof(ItemSize), typeof(Size), typeof(VirtualizingWrapPanelWithItemExpansion), new FrameworkPropertyMetadata(Size.Empty, FrameworkPropertyMetadataOptions.AffectsMeasure));

        public static readonly DependencyProperty StretchItemsProperty = DependencyProperty.Register(nameof(StretchItems), typeof(bool), typeof(VirtualizingWrapPanelWithItemExpansion), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsArrange));

        public static readonly DependencyProperty ExpandedItemTemplateProperty = DependencyProperty.Register(nameof(ExpandedItemTemplate), typeof(DataTemplate), typeof(VirtualizingWrapPanelWithItemExpansion), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure));

        public static readonly DependencyProperty ExpandedItemProperty = DependencyProperty.Register(nameof(ExpandedItem), typeof(object), typeof(VirtualizingWrapPanelWithItemExpansion), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure, (o, a) => ((VirtualizingWrapPanelWithItemExpansion)o).ExpandedItemPropertyChanged(a)));

        /// <summary>
        /// Gets or sets the spacing mode used when arranging the items. The default value is <see cref="SpacingMode.Uniform"/>.
        /// </summary>
        public SpacingMode SpacingMode { get => (SpacingMode)GetValue(SpacingModeProperty); set => SetValue(SpacingModeProperty, value); }

        /// <summary>
        /// Gets or sets a value that specifies the orientation in which items are arranged. The default value is <see cref="Orientation.Vertical"/>.
        /// </summary>
        public Orientation Orientation { get => (Orientation)GetValue(OrientationProperty); set => SetValue(OrientationProperty, value); }

        /// <summary>
        /// Gets or sets a value that specifies the size of the items. The default value is <see cref="Size.Empty"/>. 
        /// If the value is <see cref="Size.Empty"/> the size of the items gots measured by the first realized item.
        /// </summary>
        public Size ItemSize { get => (Size)GetValue(ItemSizeProperty); set => SetValue(ItemSizeProperty, value); }

        /// <summary>
        /// Gets or sets a value that specifies if the items get stretched to fill up remaining space. The default value is false.
        /// </summary>
        /// <remarks>
        /// The MaxWidth and MaxHeight properties of the ItemContainerStyle can be used to limit the stretching. 
        /// In this case the use of the remaining space will be determined by the SpacingMode property. 
        /// </remarks>
        public bool StretchItems { get => (bool)GetValue(StretchItemsProperty); set => SetValue(StretchItemsProperty, value); }

        /// <summary>
        /// Gets or sets the data template used for the item expansion.
        /// </summary>
        public DataTemplate? ExpandedItemTemplate { get => (DataTemplate?)GetValue(ExpandedItemTemplateProperty); set => SetValue(ExpandedItemTemplateProperty, value); }

        /// <summary>
        /// Gets or set the expanded item. The default value is null.
        /// </summary>
        public object? ExpandedItem { get => GetValue(ExpandedItemProperty); set => SetValue(ExpandedItemProperty, value); }

        /// <summary>
        /// The cache length before and after the viewport. 
        /// </summary>
        private VirtualizationCacheLength cacheLength;

        /// <summary>
        /// The Unit of the cache length. Can be Pixel, Item or Page. 
        /// When the ItemsOwner is a group item it can only be pixel or item.
        /// </summary>
        private VirtualizationCacheLengthUnit cacheLengthUnit;

        private Size childSize;

        private int rowCount;

        private int itemsPerRowCount;

        /// <summary>
        /// The range of items that a realized in viewport or cache.
        /// </summary>
        private ItemRangeStruct ItemRange { get; set; }

        private int ExpandedItemIndex => ExpandedItem is null ? -1 : Items.IndexOf(ExpandedItem);

        private FrameworkElement? expandedItemChild = null;

        private int itemIndexFollwingExpansion;

        protected override void OnClearChildren()
        {
            base.OnClearChildren();
            expandedItemChild = null;
        }

        protected override void OnItemsChanged(object sender, ItemsChangedEventArgs args)
        {
            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Remove:
                case NotifyCollectionChangedAction.Replace:
                    RemoveInternalChildRange(args.Position.Index, args.ItemUICount);
                    break;
                case NotifyCollectionChangedAction.Move:
                    RemoveInternalChildRange(args.OldPosition.Index, args.ItemUICount);
                    break;
            }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            UpdateChildSize(availableSize);

            if (ShouldIgnoreMeasure())
            {
                return availableSize;
            }

            var groupItem = ItemsOwner as IHierarchicalVirtualizationAndScrollInfo;

            Size extent;
            Size desiredSize;

            if (groupItem != null)
            {
                /* If the ItemsOwner is a group item the availableSize is ifinity. 
                 * Therfore the vieport size provided by the group item is used. */
                var viewportSize = groupItem.Constraints.Viewport.Size;
                var headerSize = groupItem.HeaderDesiredSizes.PixelSize;
                double availableWidth = Math.Max(viewportSize.Width - 5, 0); // left margin of 5 dp
                double availableHeight = Math.Max(viewportSize.Height - headerSize.Height, 0);
                availableSize = new Size(availableWidth, availableHeight);

                extent = CalculateExtent(availableSize);

                desiredSize = new Size(extent.Width, extent.Height);

                Extent = extent;
                ScrollOffset = groupItem.Constraints.Viewport.Location;
                ViewportSize = groupItem.Constraints.Viewport.Size;
                cacheLength = groupItem.Constraints.CacheLength;
                cacheLengthUnit = groupItem.Constraints.CacheLengthUnit; // can be Item or Pixel
            }
            else
            {
                extent = CalculateExtent(availableSize);
                double desiredWidth = Math.Min(availableSize.Width, extent.Width);
                double desiredHeight = Math.Min(availableSize.Height, extent.Height);
                desiredSize = new Size(desiredWidth, desiredHeight);

                UpdateScrollInfo(desiredSize, extent);
                cacheLength = GetCacheLength(ItemsOwner);
                cacheLengthUnit = GetCacheLengthUnit(ItemsOwner); // can be Page, Item or Pixel
            }

            ItemRange = UpdateItemRange();

            RealizeItems();
            VirtualizeItems();

            return desiredSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            double expandedItemChildHeight = 0;

            Size childSize = CalculateChildArrangeSize(finalSize);

            CalculateSpacing(finalSize, out double innerSpacing, out double outerSpacing);

            for (int childIndex = 0; childIndex < InternalChildren.Count; childIndex++)
            {
                UIElement child = InternalChildren[childIndex];

                if (child == expandedItemChild)
                {
                    int rowIndex = ExpandedItemIndex / itemsPerRowCount + 1;
                    double x = outerSpacing;
                    double y = rowIndex * GetHeight(childSize);
                    double width = GetWidth(finalSize) - (2 * outerSpacing);
                    double height = GetHeight(expandedItemChild.DesiredSize);

                    if (SpacingMode == SpacingMode.None)
                    {
                        width = itemsPerRowCount * GetWidth(childSize);
                    }

                    if (Orientation == Orientation.Horizontal)
                    {
                        expandedItemChild.Arrange(CreateRect(x - GetX(ScrollOffset), y - GetY(ScrollOffset), width, height));
                    }
                    else
                    {
                        expandedItemChild.Arrange(CreateRect(x - GetX(ScrollOffset), y - GetY(ScrollOffset), height, width));
                    }
                    expandedItemChildHeight = height;
                }
                else
                {
                    int itemIndex = GetItemIndexFromChildIndex(childIndex);

                    int columnIndex = itemIndex % itemsPerRowCount;
                    int rowIndex = itemIndex / itemsPerRowCount;

                    double x = outerSpacing + columnIndex * (GetWidth(childSize) + innerSpacing);
                    double y = rowIndex * GetHeight(childSize) + expandedItemChildHeight;

                    child.Arrange(CreateRect(x - GetX(ScrollOffset), y - GetY(ScrollOffset), childSize.Width, childSize.Height));
                }
            }

            return finalSize;
        }

        protected override void BringIndexIntoView(int index)
        {
            var offset = (index / itemsPerRowCount) * GetHeight(childSize);

            if (expandedItemChild != null && index > itemIndexFollwingExpansion)
            {
                offset += GetHeight(expandedItemChild.DesiredSize);
            }

            if (Orientation == Orientation.Horizontal)
            {
                SetHorizontalOffset(offset);
            }
            else
            {
                SetVerticalOffset(offset);
            }
        }

        private void Orientation_Changed()
        {
            MouseWheelScrollDirection = Orientation == Orientation.Horizontal ? ScrollDirection.Vertical : ScrollDirection.Horizontal;
        }

        private void ExpandedItemPropertyChanged(DependencyPropertyChangedEventArgs args)
        {
            if (args.OldValue != null)
            {
                int index = InternalChildren.IndexOf(expandedItemChild);
                if (index != -1)
                {
                    expandedItemChild = null;
                    RemoveInternalChildRange(index, 1);
                }
            }
        }

        private int GetItemIndexFromChildIndex(int childIndex)
        {
            var generatorPosition = GetGeneratorPositionFromChildIndex(childIndex);
            return RecyclingItemContainerGenerator.IndexFromGeneratorPosition(generatorPosition);
        }

        private GeneratorPosition GetGeneratorPositionFromChildIndex(int childIndex)
        {
            int expandedItemChildIndex = InternalChildren.IndexOf(expandedItemChild);
            if (expandedItemChildIndex != -1 && childIndex > expandedItemChildIndex)
            {
                return new GeneratorPosition(childIndex - 1, 0);
            }
            else
            {
                return new GeneratorPosition(childIndex, 0);
            }
        }

        private void UpdateScrollInfo(Size availableSize, Size extent)
        {
            bool invalidateScrollInfo = false;

            if (extent != Extent)
            {
                Extent = extent;
                invalidateScrollInfo = true;

            }
            if (availableSize != ViewportSize)
            {
                ViewportSize = availableSize;
                invalidateScrollInfo = true;
            }

            if (ViewportHeight != 0 && VerticalOffset != 0 && VerticalOffset + ViewportHeight + 1 >= ExtentHeight)
            {
                ScrollOffset = new Point(ScrollOffset.X, extent.Height - availableSize.Height);
                invalidateScrollInfo = true;
            }
            if (ViewportWidth != 0 && HorizontalOffset != 0 && HorizontalOffset + ViewportWidth + 1 >= ExtentWidth)
            {
                ScrollOffset = new Point(extent.Width - availableSize.Width, ScrollOffset.Y);
                invalidateScrollInfo = true;
            }

            if (invalidateScrollInfo)
            {
                ScrollOwner?.InvalidateScrollInfo();
            }
        }

        /// <summary>
        /// Realizes visible and cached items.
        /// </summary>
        private void RealizeItems()
        {
            var startPosition = RecyclingItemContainerGenerator.GeneratorPositionFromIndex(ItemRange.StartIndex);

            int childIndex = startPosition.Offset == 0 ? startPosition.Index : startPosition.Index + 1;

            int itemIndexFollwingExpansion = ExpandedItemIndex != -1 ? (((ExpandedItemIndex / itemsPerRowCount) + 1) * itemsPerRowCount) - 1 : -1;
            itemIndexFollwingExpansion = Math.Min(itemIndexFollwingExpansion, Items.Count - 1);

            if (itemIndexFollwingExpansion != this.itemIndexFollwingExpansion && expandedItemChild != null)
            {
                RemoveInternalChildRange(InternalChildren.IndexOf(expandedItemChild), 1);
                expandedItemChild = null;
            }

            using (RecyclingItemContainerGenerator.StartAt(startPosition, GeneratorDirection.Forward, true))
            {
                for (int itemIndex = ItemRange.StartIndex; itemIndex <= ItemRange.EndIndex; itemIndex++, childIndex++)
                {
                    UIElement child = (UIElement)RecyclingItemContainerGenerator.GenerateNext(out bool isNewlyRealized);

                    if (isNewlyRealized || /*recycling*/!InternalChildren.Contains(child))
                    {
                        if (childIndex >= InternalChildren.Count)
                        {
                            AddInternalChild(child);
                        }
                        else
                        {
                            InsertInternalChild(childIndex, child);
                        }

                        RecyclingItemContainerGenerator.PrepareItemContainer(child);

                        if (ItemSize == Size.Empty)
                        {
                            child.Measure(CreateSize(GetWidth(ViewportSize), double.MaxValue));
                        }
                        else
                        {
                            child.Measure(ItemSize);
                        }
                    }

                    if (itemIndex == itemIndexFollwingExpansion && ExpandedItemTemplate != null)
                    {
                        if (expandedItemChild == null)
                        {
                            expandedItemChild = (FrameworkElement)ExpandedItemTemplate.LoadContent();
                            expandedItemChild.DataContext = Items[ExpandedItemIndex];
                            expandedItemChild.Measure(CreateSize(GetWidth(ViewportSize), double.MaxValue));
                        }
                        if (!InternalChildren.Contains(expandedItemChild))
                        {
                            childIndex++;
                            if (childIndex >= InternalChildren.Count)
                            {
                                AddInternalChild(expandedItemChild);
                            }
                            else
                            {
                                InsertInternalChild(childIndex, expandedItemChild);
                            }
                        }
                    }
                }

                this.itemIndexFollwingExpansion = itemIndexFollwingExpansion;
            }
        }

        /// <summary>
        /// Virtualizes (cleanups) no longer visible or cached items.
        /// </summary>
        private void VirtualizeItems()
        {
            for (int childIndex = InternalChildren.Count - 1; childIndex >= 0; childIndex--)
            {
                var child = (FrameworkElement)InternalChildren[childIndex];

                if (child == expandedItemChild)
                {
                    if (!ItemRange.Contains(ExpandedItemIndex))
                    {
                        expandedItemChild = null;
                        RemoveInternalChildRange(childIndex, 1);
                    }
                }
                else
                {
                    int itemIndex = Items.IndexOf(child.DataContext);

                    var position = RecyclingItemContainerGenerator.GeneratorPositionFromIndex(itemIndex);

                    if (!ItemRange.Contains(itemIndex))
                    {

                        if (IsRecycling)
                        {
                            RecyclingItemContainerGenerator.Recycle(position, 1);
                        }
                        else
                        {
                            RecyclingItemContainerGenerator.Remove(position, 1);
                        }

                        RemoveInternalChildRange(childIndex, 1);
                    }
                }
            }
        }

        private void UpdateChildSize(Size availableSize)
        {
            if (ItemsOwner is IHierarchicalVirtualizationAndScrollInfo groupItem && GetIsVirtualizingWhenGrouping(ItemsControl))
            {
                if (Orientation == Orientation.Vertical)
                {
                    availableSize.Width = groupItem.Constraints.Viewport.Size.Width;
                    availableSize.Width = Math.Max(availableSize.Width - (Margin.Left + Margin.Right), 0);
                }
                else
                {
                    availableSize.Height = groupItem.Constraints.Viewport.Size.Height;
                    availableSize.Height = Math.Max(availableSize.Height - (Margin.Top + Margin.Bottom), 0);
                }
            }

            if (ItemSize != Size.Empty)
            {
                childSize = ItemSize;
            }
            else if (InternalChildren.Count != 0)
            {
                childSize = InternalChildren[0].DesiredSize;
            }
            else
            {
                childSize = CalculateChildSize(availableSize);
            }

            if (double.IsInfinity(GetWidth(availableSize)))
            {
                itemsPerRowCount = Items.Count;
            }
            else
            {
                itemsPerRowCount = Math.Max(1, (int)Math.Floor(GetWidth(availableSize) / GetWidth(childSize)));
            }

            rowCount = (int)Math.Ceiling((double)Items.Count / itemsPerRowCount);
        }

        private Size CalculateChildSize(Size availableSize)
        {
            if (Items.Count == 0)
            {
                return new Size(0, 0);
            }
            var startPosition = RecyclingItemContainerGenerator.GeneratorPositionFromIndex(0);
            using (RecyclingItemContainerGenerator.StartAt(startPosition, GeneratorDirection.Forward, true))
            {
                var child = (UIElement)RecyclingItemContainerGenerator.GenerateNext();
                AddInternalChild(child);
                RecyclingItemContainerGenerator.PrepareItemContainer(child);
                child.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                return child.DesiredSize;
            }
        }

        private Size CalculateExtent(Size availableSize)
        {
            double extentWidth = SpacingMode != SpacingMode.None && !double.IsInfinity(GetWidth(availableSize))
                ? GetWidth(availableSize)
                : GetWidth(childSize) * itemsPerRowCount;

            if (ItemsOwner is IHierarchicalVirtualizationAndScrollInfo groupItem)
            {
                if (Orientation == Orientation.Vertical)
                {
                    extentWidth = Math.Max(extentWidth - (Margin.Left + Margin.Right), 0);
                }
                else
                {
                    extentWidth = Math.Max(extentWidth - (Margin.Top + Margin.Bottom), 0);
                }
            }

            double extentHeight = GetHeight(childSize) * rowCount;

            Size extent = CreateSize(extentWidth, extentHeight);

            if (expandedItemChild != null)
            {
                if (Orientation == Orientation.Horizontal)
                {
                    extent.Height += expandedItemChild.DesiredSize.Height;
                }
                else
                {
                    extent.Width += expandedItemChild.DesiredSize.Width;
                }
            }

            return extent;
        }

        private void CalculateSpacing(Size finalSize, out double innerSpacing, out double outerSpacing)
        {
            Size childSize = CalculateChildArrangeSize(finalSize);

            double finalWidth = GetWidth(finalSize);

            double totalItemsWidth = Math.Min(GetWidth(childSize) * itemsPerRowCount, finalWidth);
            double unusedWidth = finalWidth - totalItemsWidth;

            switch (SpacingMode)
            {
                case SpacingMode.Uniform:
                    innerSpacing = outerSpacing = unusedWidth / (itemsPerRowCount + 1);
                    break;

                case SpacingMode.BetweenItemsOnly:
                    innerSpacing = unusedWidth / Math.Max(itemsPerRowCount - 1, 1);
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

        private Size CalculateChildArrangeSize(Size finalSize)
        {
            if (StretchItems)
            {
                if (Orientation == Orientation.Horizontal)
                {
                    double childMaxWidth = ReadItemContainerStyle(MaxWidthProperty, double.PositiveInfinity);
                    double maxPossibleChildWith = finalSize.Width / itemsPerRowCount;
                    double childWidth = Math.Min(maxPossibleChildWith, childMaxWidth);
                    return new Size(childWidth, childSize.Height);
                }
                else
                {
                    double childMaxHeight = ReadItemContainerStyle(MaxHeightProperty, double.PositiveInfinity);
                    double maxPossibleChildHeight = finalSize.Height / itemsPerRowCount;
                    double childHeight = Math.Min(maxPossibleChildHeight, childMaxHeight);
                    return new Size(childSize.Width, childHeight);
                }
            }
            else
            {
                return childSize;
            }
        }

        private T ReadItemContainerStyle<T>(DependencyProperty property, T fallbackValue) where T : notnull
        {
            var value = ItemsControl.ItemContainerStyle?.Setters.OfType<Setter>()
                .FirstOrDefault(setter => setter.Property == property)?.Value;
            return (T)(value ?? fallbackValue);
        }

        private ItemRangeStruct UpdateItemRange()
        {
            if (!IsVirtualizing)
            {
                return new ItemRangeStruct(0, Items.Count - 1);
            }

            int startIndex;
            int endIndex;

            if (ItemsOwner is IHierarchicalVirtualizationAndScrollInfo groupItem)
            {
                if (!GetIsVirtualizingWhenGrouping(ItemsControl))
                {
                    return new ItemRangeStruct(0, Items.Count - 1);
                }

                var offset = new Point(ScrollOffset.X, groupItem.Constraints.Viewport.Location.Y);

                int offsetRowIndex;
                double offsetInPixel;

                int rowCountInViewport;

                if (ScrollUnit == ScrollUnit.Item)
                {
                    offsetRowIndex = GetY(offset) >= 1 ? (int)GetY(offset) - 1 : 0; // ignore header
                    offsetInPixel = offsetRowIndex * GetHeight(childSize);
                }
                else
                {
                    offsetInPixel = Math.Min(Math.Max(GetY(offset) - GetHeight(groupItem.HeaderDesiredSizes.PixelSize), 0), GetHeight(Extent));
                    offsetRowIndex = GetRowIndex(offsetInPixel);
                }

                double viewportHeight = Math.Min(GetHeight(ViewportSize), Math.Max(GetHeight(Extent) - offsetInPixel, 0));

                rowCountInViewport = (int)Math.Ceiling((offsetInPixel + viewportHeight) / GetHeight(childSize)) - (int)Math.Floor(offsetInPixel / GetHeight(childSize));

                startIndex = offsetRowIndex * itemsPerRowCount;
                endIndex = Math.Min(((offsetRowIndex + rowCountInViewport) * itemsPerRowCount) - 1, Items.Count - 1);

                if (cacheLengthUnit == VirtualizationCacheLengthUnit.Pixel)
                {
                    double cacheBeforeInPixel = Math.Min(cacheLength.CacheBeforeViewport, offsetInPixel);
                    double cacheAfterInPixel = Math.Min(cacheLength.CacheAfterViewport, GetHeight(Extent) - viewportHeight - offsetInPixel);
                    int rowCountInCacheBefore = (int)(cacheBeforeInPixel / GetHeight(childSize));
                    int rowCountInCacheAfter = ((int)Math.Ceiling((offsetInPixel + viewportHeight + cacheAfterInPixel) / GetHeight(childSize))) - (int)Math.Ceiling((offsetInPixel + viewportHeight) / GetHeight(childSize));
                    startIndex = Math.Max(startIndex - rowCountInCacheBefore * itemsPerRowCount, 0);
                    endIndex = Math.Min(endIndex + rowCountInCacheAfter * itemsPerRowCount, Items.Count - 1);
                }
                else if (cacheLengthUnit == VirtualizationCacheLengthUnit.Item)
                {
                    startIndex = Math.Max(startIndex - (int)cacheLength.CacheBeforeViewport, 0);
                    endIndex = Math.Min(endIndex + (int)cacheLength.CacheAfterViewport, Items.Count - 1);
                }
            }
            else
            {
                double viewportSartPos = GetY(ScrollOffset);
                double viewportEndPos = GetY(ScrollOffset) + GetHeight(ViewportSize);

                if (cacheLengthUnit == VirtualizationCacheLengthUnit.Pixel)
                {
                    viewportSartPos = Math.Max(viewportSartPos - cacheLength.CacheBeforeViewport, 0);
                    viewportEndPos = Math.Min(viewportEndPos + cacheLength.CacheAfterViewport, GetHeight(Extent));
                }

                int startRowIndex = GetRowIndex(viewportSartPos);
                startIndex = startRowIndex * itemsPerRowCount;

                int endRowIndex = GetRowIndex(viewportEndPos);
                endIndex = Math.Min(endRowIndex * itemsPerRowCount + (itemsPerRowCount - 1), Items.Count - 1);

                if (cacheLengthUnit == VirtualizationCacheLengthUnit.Page)
                {
                    int itemsPerPage = endIndex - startIndex + 1;
                    startIndex = Math.Max(startIndex - (int)cacheLength.CacheBeforeViewport * itemsPerPage, 0);
                    endIndex = Math.Min(endIndex + (int)cacheLength.CacheAfterViewport * itemsPerPage, Items.Count - 1);
                }
                else if (cacheLengthUnit == VirtualizationCacheLengthUnit.Item)
                {
                    startIndex = Math.Max(startIndex - (int)cacheLength.CacheBeforeViewport, 0);
                    endIndex = Math.Min(endIndex + (int)cacheLength.CacheAfterViewport, Items.Count - 1);
                }
            }

            return new ItemRangeStruct(startIndex, endIndex);
        }

        private int GetRowIndex(double location)
        {
            int calculatedRowIndex = (int)Math.Floor(location / GetHeight(childSize));
            int maxRowIndex = (int)Math.Ceiling((double)Items.Count / (double)itemsPerRowCount);
            return Math.Max(Math.Min(calculatedRowIndex, maxRowIndex), 0);
        }

        protected override double GetLineUpScrollAmount()
        {
            return -Math.Min(childSize.Height * ScrollLineDeltaItem, ViewportSize.Height);
        }

        protected override double GetLineDownScrollAmount()
        {
            return Math.Min(childSize.Height * ScrollLineDeltaItem, ViewportSize.Height);
        }

        protected override double GetLineLeftScrollAmount()
        {
            return -Math.Min(childSize.Width * ScrollLineDeltaItem, ViewportSize.Width);
        }

        protected override double GetLineRightScrollAmount()
        {
            return Math.Min(childSize.Width * ScrollLineDeltaItem, ViewportSize.Width);
        }

        protected override double GetMouseWheelUpScrollAmount()
        {
            return -Math.Min(childSize.Height * MouseWheelDeltaItem, ViewportSize.Height);
        }

        protected override double GetMouseWheelDownScrollAmount()
        {
            return Math.Min(childSize.Height * MouseWheelDeltaItem, ViewportSize.Height);
        }

        protected override double GetMouseWheelLeftScrollAmount()
        {
            return -Math.Min(childSize.Width * MouseWheelDeltaItem, ViewportSize.Width);
        }

        protected override double GetMouseWheelRightScrollAmount()
        {
            return Math.Min(childSize.Width * MouseWheelDeltaItem, ViewportSize.Width);
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

        /* orientation aware helper methods */

        private double GetX(Point point) => Orientation == Orientation.Horizontal ? point.X : point.Y;
        private double GetY(Point point) => Orientation == Orientation.Horizontal ? point.Y : point.X;

        private double GetWidth(Size size) => Orientation == Orientation.Horizontal ? size.Width : size.Height;
        private double GetHeight(Size size) => Orientation == Orientation.Horizontal ? size.Height : size.Width;

        private Size CreateSize(double width, double height) => Orientation == Orientation.Horizontal ? new Size(width, height) : new Size(height, width);
        private Rect CreateRect(double x, double y, double width, double height) => Orientation == Orientation.Horizontal ? new Rect(x, y, width, height) : new Rect(y, x, width, height);
    }
}

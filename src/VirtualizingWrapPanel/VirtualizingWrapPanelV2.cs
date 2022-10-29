using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace WpfToolkit.Controls
{
    /// <summary>
    /// A implementation of a wrap panel that supports virtualization and can be used in horizontal and vertical orientation.
    /// </summary>
    public class VirtualizingWrapPanelV2 : VirtualizingPanelBase
    {
        public static readonly DependencyProperty SpacingModeProperty = DependencyProperty.Register(nameof(SpacingMode), typeof(SpacingMode), typeof(VirtualizingWrapPanelV2), new FrameworkPropertyMetadata(SpacingMode.Uniform, FrameworkPropertyMetadataOptions.AffectsMeasure));

        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(nameof(Orientation), typeof(Orientation), typeof(VirtualizingWrapPanelV2), new FrameworkPropertyMetadata(Orientation.Vertical, FrameworkPropertyMetadataOptions.AffectsMeasure, (obj, args) => ((VirtualizingWrapPanelV2)obj).Orientation_Changed()));

        public static readonly DependencyProperty ItemSizeProperty = DependencyProperty.Register(nameof(ItemSize), typeof(Size), typeof(VirtualizingWrapPanelV2), new FrameworkPropertyMetadata(Size.Empty, FrameworkPropertyMetadataOptions.AffectsMeasure));

        public static readonly DependencyProperty StretchItemsProperty = DependencyProperty.Register(nameof(StretchItems), typeof(bool), typeof(VirtualizingWrapPanelV2), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsArrange));

        /// <summary>
        /// Gets or sets the spacing mode used when arranging the items. The default value is <see cref="SpacingMode.Uniform"/>.
        /// </summary>
        public SpacingMode SpacingMode { get => (SpacingMode)GetValue(SpacingModeProperty); set => SetValue(SpacingModeProperty, value); }

        /// <summary>
        /// Gets or sets a value that specifies the orientation in which items are arranged. The default value is <see cref="Orientation.Horizontal"/>.
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

        private List<Size> itemSizes = new List<Size>();

        private double cacheOffsetX = 0;
        private double cacheOffsetY = 0; // relative to scroll offset

        private Size availableSize = Size.Empty;
        private double knownExtend = 0;

        private void Orientation_Changed()
        {
            MouseWheelScrollDirection = Orientation == Orientation.Horizontal ? ScrollDirection.Vertical : ScrollDirection.Horizontal;
        }

        protected override void OnItemsChanged(object sender, ItemsChangedEventArgs args)
        {
            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Reset:
                    itemSizes.Clear();
                    break;
                case NotifyCollectionChangedAction.Remove:
                    itemSizes.RemoveRange(args.Position.Offset, args.ItemUICount);
                    break;
                case NotifyCollectionChangedAction.Add:
                case NotifyCollectionChangedAction.Replace:
                case NotifyCollectionChangedAction.Move:
                    // TODO
                    itemSizes.Clear();
                    break;
            }

            base.OnItemsChanged(sender, args);
        }

        protected override Size CalculateExtent(Size availableSize)
        {
            if (Items.Count == 0)
            {
                return new Size(0, 0);  // TODO ?
            }
            if (itemSizes.Count == 0)
            {
                return availableSize; // TODO: set extent after UpdateItemRange
            }

           // if (this.availableSize != availableSize)
           // {
                double x = 0;
                double y = 0;
                double currentRowHeight = 0;
                foreach (var itemSize in itemSizes)
                {
                    if (x + GetWidth(itemSize) > GetWidth(availableSize))
                    {
                        x = 0;
                        y += currentRowHeight;
                        currentRowHeight = 0;
                    }
                    x += GetWidth(itemSize);
                    currentRowHeight = Math.Max(currentRowHeight, GetHeight(itemSize));
                }
                knownExtend = y + currentRowHeight;
            //  }

            // TODO: hier passt was mit dem knownExtend nicht, ggf. erstmal umbauen
            double estimatedExtend = (double)Items.Count / itemSizes.Count * knownExtend;
            return CreateSize(GetWidth(availableSize), estimatedExtend);
        }

        protected override ItemRange UpdateItemRange()
        {
            if (!IsVirtualizing)
            {
                return new ItemRange(0, Items.Count - 1);
            }

            int startIndex = FindStartIndex();

            int endIndex = Items.Count - 1;

            // TODO
            base.ItemRange = new ItemRange(startIndex, endIndex);
            base.VirtualizeItems();

            double cacheLength = 0;

            if (CacheLengthUnit == VirtualizationCacheLengthUnit.Page)
            {
                cacheLength = CacheLength.CacheAfterViewport * GetHeight(ViewportSize);
            }
            else if (CacheLengthUnit == VirtualizationCacheLengthUnit.Pixel)
            {
                cacheLength = CacheLength.CacheAfterViewport;
            }

            double x = cacheOffsetX;
            double yRelativeToViewport = cacheOffsetY;
            double currentRowHeight = 0;

            var startPosition = ItemContainerGenerator.GeneratorPositionFromIndex(startIndex);
            using (ItemContainerGenerator.StartAt(startPosition, GeneratorDirection.Forward, true))
            {
                for (int i = startIndex, childIndex = 0; i < Items.Count; i++, childIndex++)
                {
                    var container = (UIElement)ItemContainerGenerator.GenerateNext(out bool isNewlyRealized);

                    if (isNewlyRealized || /* is recycled */ !InternalChildren.Contains(container))
                    {
                        if (childIndex >= InternalChildren.Count)
                        {
                            AddInternalChild(container);
                        }
                        else
                        {
                            InsertInternalChild(childIndex, container);
                        }
                    }

                    ItemContainerGenerator.PrepareItemContainer(container);

                    container.Measure(ItemSize != Size.Empty ? ItemSize : new Size(double.PositiveInfinity, double.PositiveInfinity));
                    Size itemSize = ItemSize != Size.Empty ? ItemSize : container.DesiredSize;

                    if (i >= itemSizes.Count)
                    {
                        itemSizes.Add(itemSize);
                    }

                    if (x + GetWidth(itemSize) > GetWidth(ViewportSize))
                    {
                        x = 0;
                        yRelativeToViewport += currentRowHeight;
                        currentRowHeight = 0;
                    }

                    x += GetWidth(itemSize);
                    currentRowHeight = Math.Max(currentRowHeight, GetHeight(itemSize));

                    if (endIndex == Items.Count - 1 && yRelativeToViewport >= GetHeight(ViewportSize) + cacheLength)
                    {
                        endIndex = i - 1;
                    }
                    if (endIndex != Items.Count - 1)
                    {
                        if (CacheLengthUnit == VirtualizationCacheLengthUnit.Item)
                        {
                            if (i >= endIndex + (int)CacheLength.CacheAfterViewport)
                            {
                                endIndex = i;
                                break;
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }

            if (GetY(Offset) + yRelativeToViewport + currentRowHeight > knownExtend)
            {
                //knownExtend = GetY(Offset) + yRelativeToViewport + currentRowHeight;
            }

            return new ItemRange(startIndex, endIndex);
        }

        private int FindStartIndex()
        {
            if (GetY(Offset) == 0)
            {
                cacheOffsetX = 0;
                cacheOffsetY = 0;
                return 0;
            }

            double cacheLength = 0;

            if (CacheLengthUnit == VirtualizationCacheLengthUnit.Page)
            {
                cacheLength = CacheLength.CacheBeforeViewport * GetHeight(ViewportSize);
            }
            else if (CacheLengthUnit == VirtualizationCacheLengthUnit.Pixel)
            {
                cacheLength = CacheLength.CacheBeforeViewport;
            }

            double startOffsetY = Math.Max(GetY(Offset) - cacheLength, 0);

            int currentRowFirstItemIndex = 0;

            double currentX = 0;
            double currentY = 0;
            double currentRowHeight = 0;

            for (int i = 0; i < Items.Count; i++)
            {
                Size itemSize;

                if (itemSizes.Count > i)
                {
                    itemSize = itemSizes[i];
                }
                else
                {
                    if (ItemSize != Size.Empty)
                    {
                        itemSize = ItemSize;
                        itemSizes.Add(ItemSize);
                    }
                    else
                    {
                        itemSize = MeasureItemSize(i);
                        itemSizes.Add(ItemSize);
                    }
                }

                if (currentX + GetWidth(itemSize) > GetWidth(ViewportSize))
                {
                    currentX = 0;
                    currentY += currentRowHeight;
                    currentRowHeight = 0;
                    currentRowFirstItemIndex = i;
                }

                currentX += GetWidth(itemSize);
                currentRowHeight = Math.Max(currentRowHeight, GetHeight(itemSize));

                if (currentY + currentRowHeight >= startOffsetY)
                {
                    cacheOffsetX = 0;
                    cacheOffsetY = currentY - GetY(Offset);

                    if (currentY > knownExtend)
                    {
                        //knownExtend = currentY;
                    }

                    if (CacheLengthUnit == VirtualizationCacheLengthUnit.Item)
                    {
                        int startIndex = Math.Max(currentRowFirstItemIndex - (int)CacheLength.CacheBeforeViewport, 0);
                        UpdateCacheOffset(startIndex);
                        return startIndex;
                    }

                    return currentRowFirstItemIndex;
                }
            }

            return 0;
        }

        private Size MeasureItemSize(int index)
        {
            var startPosition = ItemContainerGenerator.GeneratorPositionFromIndex(index);
            using (ItemContainerGenerator.StartAt(startPosition, GeneratorDirection.Forward, true))
            {
                var container = (UIElement)ItemContainerGenerator.GenerateNext(out bool isNewlyRealized);
                if (!InternalChildren.Contains(container))
                {
                    InsertInternalChild(0, container);
                }
                ItemContainerGenerator.PrepareItemContainer(container);
                container.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                Size itemSize = container.DesiredSize;

                var generatorPosition = GetGeneratorPositionFromChildIndex(0);
                if (VirtualizationMode == VirtualizationMode.Recycling)
                {
                    ItemContainerGenerator.Recycle(generatorPosition, 1);
                }
                else
                {
                    ItemContainerGenerator.Remove(generatorPosition, 1);
                }
                RemoveInternalChildRange(0, 1);

                return itemSize;
            }
        }

        private void UpdateCacheOffset(int startIndex)
        {
            double currentX = 0;
            double currentY = 0;
            double currentRowHeight = 0;

            for (int i = 0; i <= startIndex; i++)
            {
                Size itemSize = itemSizes[i];

                if (currentX + GetWidth(itemSize) > GetWidth(ViewportSize))
                {
                    currentX = 0;
                    currentY += currentRowHeight;
                    currentRowHeight = 0;
                }

                currentX += GetWidth(itemSize);
                currentRowHeight = Math.Max(currentRowHeight, GetHeight(itemSize));
            }

            cacheOffsetX = currentX;
            cacheOffsetY = currentY - GetY(Offset);
        }

        protected override void RealizeItems()
        {
            // done in UpdateItemRange
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            double x = cacheOffsetX;
            double y = cacheOffsetY;
            double currentRowHeight = 0;
            var currentRowChilds = new List<UIElement>();

            for (int childIndex = 0; childIndex < InternalChildren.Count; childIndex++)
            {
                UIElement child = InternalChildren[childIndex];

                Size childSize = ItemSize != Size.Empty ? ItemSize : child.DesiredSize;

                if (x + GetWidth(childSize) > GetWidth(finalSize))
                {
                    ArrangeRow(GetWidth(finalSize), currentRowChilds, y);
                    x = 0;
                    y += currentRowHeight;
                    currentRowHeight = 0;
                    currentRowChilds.Clear();
                }

                x += GetWidth(childSize);
                currentRowHeight = Math.Max(currentRowHeight, GetHeight(childSize));
                currentRowChilds.Add(child);
            }

            if (currentRowChilds.Any())
            {
                ArrangeRow(GetWidth(finalSize), currentRowChilds, y);
            }

            return finalSize;
        }

        private void ArrangeRow(double rowWidth, List<UIElement> children, double y)
        {
            double extraWidth = 0;
            double innerSpacing = 0;
            double outerSpacing = 0;

            if (StretchItems) // TODO: handle MaxWidth/MaxHeight and apply spacing
            {
                double summedUpChildWidth = children.Sum(child => GetWidth(child.DesiredSize));
                double unusedWidth = rowWidth - summedUpChildWidth;
                extraWidth = unusedWidth / children.Count;
            }
            else
            {
                CalculateRowSpacing(rowWidth, children, out innerSpacing, out outerSpacing);
            }

            double x = outerSpacing;

            foreach (var child in children)
            {
                Size childSize = ItemSize != Size.Empty ? ItemSize : child.DesiredSize;
                child.Arrange(CreateRect(x, y, GetWidth(childSize) + extraWidth, GetHeight(childSize)));
                x += GetWidth(childSize) + extraWidth + innerSpacing;
            }
        }

        private void CalculateRowSpacing(double rowWidth, List<UIElement> children, out double innerSpacing, out double outerSpacing)
        {            
          
            double summedUpChildWidth = children.Sum(child => GetWidth(ItemSize != Size.Empty ? ItemSize : child.DesiredSize));
            double unusedWidth = rowWidth - summedUpChildWidth;

            switch (SpacingMode)
            {
                case SpacingMode.Uniform:
                    innerSpacing = outerSpacing = unusedWidth / (children.Count + 1);
                    break;

                case SpacingMode.BetweenItemsOnly:
                    innerSpacing = unusedWidth / Math.Max(children.Count - 1, 1);
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

        protected override void BringIndexIntoView(int index)
        {
            if (index < 0 || index >= Items.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index), $"The argument {nameof(index)} must be >= 0 and < the number of items.");
            }

            // TODO
            //
            //if (itemsPerRowCount == 0)
            //{
            //    throw new InvalidOperationException();
            //}

            //var offset = (index / itemsPerRowCount) * GetHeight(childSize);

            //if (Orientation == Orientation.Horizontal)
            //{
            //    SetHorizontalOffset(offset);
            //}
            //else
            //{
            //    SetVerticalOffset(offset);
            //}
        }

        protected override double GetLineUpScrollAmount()
        {
            throw new NotSupportedException();
        }

        protected override double GetLineDownScrollAmount()
        {
            throw new NotSupportedException();
        }

        protected override double GetLineLeftScrollAmount()
        {
            throw new NotSupportedException();
        }

        protected override double GetLineRightScrollAmount()
        {
            throw new NotSupportedException();
        }

        protected override double GetMouseWheelUpScrollAmount()
        {
            throw new NotSupportedException();
        }

        protected override double GetMouseWheelDownScrollAmount()
        {
            throw new NotSupportedException();
        }

        protected override double GetMouseWheelLeftScrollAmount()
        {
            throw new NotSupportedException();
        }

        protected override double GetMouseWheelRightScrollAmount()
        {
            throw new NotSupportedException();
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

        protected double GetX(Point point) => Orientation == Orientation.Horizontal ? point.X : point.Y;
        protected double GetY(Point point) => Orientation == Orientation.Horizontal ? point.Y : point.X;

        protected double GetWidth(Size size) => Orientation == Orientation.Horizontal ? size.Width : size.Height;
        protected double GetHeight(Size size) => Orientation == Orientation.Horizontal ? size.Height : size.Width;

        protected Size CreateSize(double width, double height) => Orientation == Orientation.Horizontal ? new Size(width, height) : new Size(height, width);
        protected Rect CreateRect(double x, double y, double width, double height) => Orientation == Orientation.Horizontal ? new Rect(x, y, width, height) : new Rect(y, x, width, height);
    }
}

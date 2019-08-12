using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows;
using System.Windows.Media;
using System.Diagnostics;
using System.ComponentModel;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Media.Animation;
using System.Threading.Tasks;
using System.Reflection;

namespace WpfToolkit.Controls
{

    /// <summary>
    /// A implementation of a wrap panel that supports virtualization and can be used in horizontal and vertical orientation.
    /// <p class="note">In order to work properly all items must have the same size.</p>
    /// </summary>
    public class VirtualizingWrapPanel : VirtualizingPanelBase
    {

        #region depracted properties 

        [Obsolete("Use ItemSizeProperty")]
        public static readonly DependencyProperty ChildrenSizeProperty = ItemSizeProperty;

        [Obsolete("Use IsSpacingEnabledProperty")]
        public static readonly DependencyProperty SpacingEnabledProperty = IsSpacingEnabledProperty;

        [Obsolete("Use IsSpacingEnabled")]
        public bool SpacingEnabled { get => IsSpacingEnabled; set => IsSpacingEnabled = value; }

        [Obsolete("Use ItemSize")]
        public Size ChildrenSize { get => ItemSize; set => ItemSize = value; }

        #endregion

        public static readonly DependencyProperty IsSpacingEnabledProperty = DependencyProperty.Register(nameof(IsSpacingEnabled), typeof(bool), typeof(VirtualizingWrapPanel), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsMeasure));

        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(nameof(Orientation), typeof(Orientation), typeof(VirtualizingWrapPanel), new FrameworkPropertyMetadata(Orientation.Vertical, FrameworkPropertyMetadataOptions.AffectsMeasure, (obj, args) => ((VirtualizingWrapPanel)obj).Orientation_Changed()));

        public static readonly DependencyProperty ItemSizeProperty = DependencyProperty.Register(nameof(ItemSize), typeof(Size), typeof(VirtualizingWrapPanel), new FrameworkPropertyMetadata(Size.Empty, FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        ///  Gets or sets a value that specifies whether the items are distributed evenly across the width (horizontal orientation) 
        ///  or height (vertical orientation). The default value is true.
        /// </summary>
        public bool IsSpacingEnabled { get => (bool)GetValue(IsSpacingEnabledProperty); set => SetValue(IsSpacingEnabledProperty, value); }

        /// <summary>
        /// Gets or sets a value that specifies the orientation in which itmes are arranged. The default value is <see cref="Orientation.Vertical"/>.
        /// </summary>
        public Orientation Orientation { get => (Orientation)GetValue(OrientationProperty); set => SetValue(OrientationProperty, value); }

        /// <summary>
        /// Gets or sets a value that specifies the size of the items. The default value is <see cref="Size.Empty"/>. 
        /// If the value is <see cref="Size.Empty"/> the size of the items gots measured by the first realized item.
        /// </summary>
        public Size ItemSize { get => (Size)GetValue(ItemSizeProperty); set => SetValue(ItemSizeProperty, value); }

        protected Size childSize;

        protected int rowCount;

        protected int itemsPerRowCount;

        private static readonly DependencyProperty ItemsHostInsetProperty = (DependencyProperty)typeof(VirtualizingStackPanel).GetField("ItemsHostInsetProperty", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);

        private void Orientation_Changed()
        {
            MouseWheelScrollDirection = Orientation == Orientation.Vertical ? ScrollDirection.Vertical : ScrollDirection.Horizontal;
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            UpdateChildSize(availableSize);
            return base.MeasureOverride(availableSize);
        }

        private void UpdateChildSize(Size availableSize)
        {
            if (ItemsOwner is IHierarchicalVirtualizationAndScrollInfo groupItem)
            {
                if (Orientation == Orientation.Vertical)
                {
                    availableSize.Width = groupItem.Constraints.Viewport.Size.Width;
                }
                else
                {
                    availableSize.Height = groupItem.Constraints.Viewport.Size.Height;
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
            var startPosition = ItemContainerGenerator.GeneratorPositionFromIndex(0);
            using (ItemContainerGenerator.StartAt(startPosition, GeneratorDirection.Forward, true))
            {
                var child = (UIElement)ItemContainerGenerator.GenerateNext();
                AddInternalChild(child);
                ItemContainerGenerator.PrepareItemContainer(child);
                child.Measure(CreateSize(GetWidth(availableSize), double.PositiveInfinity));
                return child.DesiredSize;
            }
        }

        protected override Size CalculateExtent(Size availableSize)
        {
            double extentWidth = IsSpacingEnabled ? GetWidth(availableSize) : GetWidth(childSize) * itemsPerRowCount;
            double extentHeight = GetHeight(childSize) * rowCount;
            return CreateSize(extentWidth, extentHeight);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            double offsetX = GetX(Offset);
            double offsetY = GetY(Offset);

            /* When the items owner is a group item offset is handled by the parent panel. */
            if (ItemsOwner is IHierarchicalVirtualizationAndScrollInfo groupItem)
            {
                offsetY = 0;                
            }

            double unusedWidth = GetWidth(finalSize) - (GetWidth(childSize) * itemsPerRowCount);
            double spacing = unusedWidth > 0 ? unusedWidth / (itemsPerRowCount + 1) : 0;

            for (int childIndex = 0; childIndex < InternalChildren.Count; childIndex++)
            {
                UIElement child = InternalChildren[childIndex];

                int itemIndex = GetItemIndexFromChildIndex(childIndex);

                int columnIndex = itemIndex % itemsPerRowCount;
                int rowIndex = itemIndex / itemsPerRowCount;

                double x = columnIndex * GetWidth(childSize);

                if (IsSpacingEnabled)
                {
                    x += (columnIndex + 1) * spacing;
                }

                double y = rowIndex * GetHeight(childSize);

                if (GetHeight(finalSize) == 0)
                {
                    /* When the parent panel is grouping and a cached group item is not 
                     * in the viewport it has no valid arrangement. That means that the 
                     * height/width is 0. Therfore the items should not be visible so 
                     * that they are not falsely displayed. */
                    child.Arrange(new Rect(0, 0, 0, 0));
                }
                else
                {
                    child.Arrange(CreateRect(x - offsetX, y - offsetY, childSize.Width, childSize.Height));                  
                }
            }

            return finalSize;
        }

        protected override ItemRange UpdateItemRange()
        {
            if (!IsVirtualizing)
            {
                return new ItemRange(0, Items.Count - 1);
            }

            int startIndex;
            int endIndex;

            if (ItemsOwner is IHierarchicalVirtualizationAndScrollInfo groupItem)
            {
                var Offset = new Point(this.Offset.X, groupItem.Constraints.Viewport.Location.Y);

                int offsetRowIndex;
                double offsetInPixel;

                int rowCountInViewport;
                int rowCountInCacheBefore = 0;
                int rowCountInCacheAfter = 0;

                if (ScrollUnit == ScrollUnit.Item)
                {
                    offsetRowIndex = GetY(Offset) >= 1 ? (int)GetY(Offset) - 1 : 0; // ignore header
                    offsetInPixel = offsetRowIndex * GetHeight(childSize);
                }
                else
                {
                    offsetInPixel = Math.Min(Math.Max(GetY(Offset) - GetHeight(groupItem.HeaderDesiredSizes.PixelSize), 0), GetHeight(Extent));
                    offsetRowIndex = GetRowIndex(offsetInPixel);
                }

                double vh;
                if (GetY(Offset) < 1)
                {
                    vh = Math.Max(GetHeight(Viewport), 0);
                }
                else
                {
                    vh = GetHeight(Viewport);
                }
                double viewportHeight = Math.Min(vh, Math.Max(GetHeight(Extent)/*?*/ - offsetInPixel, 0));
               
                rowCountInViewport = (int)Math.Ceiling((offsetInPixel + viewportHeight) / GetHeight(childSize)) - (int)Math.Floor(offsetInPixel / GetHeight(childSize));
                
                startIndex = offsetRowIndex * itemsPerRowCount;
                endIndex = Math.Min(((offsetRowIndex + rowCountInViewport) * itemsPerRowCount) - 1, Items.Count - 1);

                if (CacheLengthUnit == VirtualizationCacheLengthUnit.Pixel)
                {
                    double cacheBeforeInPixel = Math.Min(CacheLength.CacheBeforeViewport, offsetInPixel);
                    double cacheAfterInPixel = Math.Min(CacheLength.CacheAfterViewport, GetHeight(Extent) - viewportHeight - offsetInPixel);
                    rowCountInCacheBefore = (int)(cacheBeforeInPixel / GetHeight(childSize));
                    rowCountInCacheAfter = ((int)Math.Ceiling((offsetInPixel + viewportHeight + cacheAfterInPixel) / GetHeight(childSize))) - (int)Math.Ceiling((offsetInPixel + viewportHeight) / GetHeight(childSize));
                    startIndex = Math.Max(startIndex - rowCountInCacheBefore * itemsPerRowCount, 0);
                    endIndex = Math.Min(endIndex + rowCountInCacheAfter * itemsPerRowCount, Items.Count - 1);
                }
                else if (CacheLengthUnit == VirtualizationCacheLengthUnit.Item)
                {
                    rowCountInCacheBefore = (int)Math.Ceiling(CacheLength.CacheBeforeViewport / itemsPerRowCount);
                    rowCountInCacheAfter = (int)Math.Ceiling(CacheLength.CacheAfterViewport / itemsPerRowCount);
                    startIndex = Math.Max(startIndex - (int)CacheLength.CacheBeforeViewport, 0);
                    endIndex = Math.Min(endIndex + (int)CacheLength.CacheAfterViewport, Items.Count - 1);
                }
            }
            else
            {
                double viewportSartPos = GetY(Offset);
                double viewportEndPos = GetY(Offset) + GetHeight(Viewport);

                if (CacheLengthUnit == VirtualizationCacheLengthUnit.Pixel)
                {
                    viewportSartPos = Math.Max(viewportSartPos - CacheLength.CacheBeforeViewport, 0);
                    viewportEndPos = Math.Min(viewportEndPos + CacheLength.CacheAfterViewport, GetHeight(Extent));
                }

                int startRowIndex = GetRowIndex(viewportSartPos);
                startIndex = startRowIndex * itemsPerRowCount;

                int endRowIndex = GetRowIndex(viewportEndPos);
                endIndex = Math.Min(endRowIndex * itemsPerRowCount + (itemsPerRowCount - 1), Items.Count - 1);

                if (CacheLengthUnit == VirtualizationCacheLengthUnit.Page)
                {
                    int itemsPerPage = endIndex - startIndex + 1;
                    startIndex = Math.Max(startIndex - (int)CacheLength.CacheBeforeViewport * itemsPerPage, 0);
                    endIndex = Math.Min(endIndex + (int)CacheLength.CacheAfterViewport * itemsPerPage, Items.Count - 1);
                }
                else if (CacheLengthUnit == VirtualizationCacheLengthUnit.Item)
                {
                    startIndex = Math.Max(startIndex - (int)CacheLength.CacheBeforeViewport, 0);
                    endIndex = Math.Min(endIndex + (int)CacheLength.CacheAfterViewport, Items.Count - 1);
                }
            }

            return new ItemRange(startIndex, endIndex);
        }

        private int GetRowIndex(double location)
        {
            int calculatedRowIndex = (int)Math.Floor(location / GetHeight(childSize));
            int maxRowIndex = (int)Math.Ceiling((double)Items.Count / (double)itemsPerRowCount);
            return Math.Max(Math.Min(calculatedRowIndex, maxRowIndex), 0);
        }

        protected override void BringIndexIntoView(int index)
        {
            var offset = (index / itemsPerRowCount) * GetHeight(childSize);
            if (Orientation == Orientation.Horizontal)
            {
                SetHorizontalOffset(offset);
            }
            else
            {
                SetVerticalOffset(offset);
            }
        }

        protected override double GetLineUpScrollAmount()
        {
            return -GetHeight(childSize);
        }

        protected override double GetLineDownScrollAmount()
        {
            return GetHeight(childSize);
        }

        protected override double GetLineLeftScrollAmount()
        {
            return -GetWidth(childSize);
        }

        protected override double GetLineRightScrollAmount()
        {
            return GetWidth(childSize);
        }

        protected override double GetMouseWheelUpScrollAmount()
        {
            return GetLineUpScrollAmount() * 3;
        }

        protected override double GetMouseWheelDownScrollAmount()
        {
            return GetLineDownScrollAmount() * 3;
        }

        protected override double GetMouseWheelLeftScrollAmount()
        {
            return GetLineLeftScrollAmount() * 3;
        }

        protected override double GetMouseWheelRightScrollAmount()
        {
            return GetLineRightScrollAmount() * 3;
        }

        protected override double GetPageUpScrollAmount()
        {
            return -Viewport.Height;
        }

        protected override double GetPageDownScrollAmount()
        {
            return Viewport.Height;
        }

        protected override double GetPageLeftScrollAmount()
        {
            return -Viewport.Width;
        }

        protected override double GetPageRightScrollAmount()
        {
            return Viewport.Width;
        }

        /* orientation aware helper methods */

        protected double GetX(Point point) => Orientation == Orientation.Vertical ? point.X : point.Y;
        protected double GetY(Point point) => Orientation == Orientation.Vertical ? point.Y : point.X;

        protected double GetWidth(Size size) => Orientation == Orientation.Vertical ? size.Width : size.Height;
        protected double GetHeight(Size size) => Orientation == Orientation.Vertical ? size.Height : size.Width;

        protected Size CreateSize(double width, double height) => Orientation == Orientation.Vertical ? new Size(width, height) : new Size(height, width);
        protected Rect CreateRect(double x, double y, double width, double height) => Orientation == Orientation.Vertical ? new Rect(x, y, width, height) : new Rect(y, x, width, height);

    }

}
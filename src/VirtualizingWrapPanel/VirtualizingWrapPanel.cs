using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Media3D;

namespace WpfToolkit.Controls
{
    /// <summary>
    /// A implementation of a wrap panel that supports virtualization and can be used in horizontal and vertical orientation.
    /// </summary>
    public class VirtualizingWrapPanel : VirtualizingPanelBase
    {
        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(nameof(Orientation), typeof(Orientation), typeof(VirtualizingWrapPanel), new FrameworkPropertyMetadata(Orientation.Horizontal, FrameworkPropertyMetadataOptions.AffectsMeasure, (obj, args) => ((VirtualizingWrapPanel)obj).Orientation_Changed()));

        public static readonly DependencyProperty ItemSizeProperty = DependencyProperty.Register(nameof(ItemSize), typeof(Size), typeof(VirtualizingWrapPanel), new FrameworkPropertyMetadata(Size.Empty, FrameworkPropertyMetadataOptions.AffectsMeasure));

        public static readonly DependencyProperty ItemSizeProviderProperty = DependencyProperty.Register(nameof(ItemSizeProvider), typeof(IItemSizeProvider), typeof(VirtualizingWrapPanel), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure));

        public static readonly DependencyProperty AllowDifferentSizedItemsProperty = DependencyProperty.Register(nameof(AllowDifferentSizedItems), typeof(bool), typeof(VirtualizingWrapPanel), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsMeasure));

        public static readonly DependencyProperty SpacingModeProperty = DependencyProperty.Register(nameof(SpacingMode), typeof(SpacingMode), typeof(VirtualizingWrapPanel), new FrameworkPropertyMetadata(SpacingMode.Uniform, FrameworkPropertyMetadataOptions.AffectsArrange));

        public static readonly DependencyProperty StretchItemsProperty = DependencyProperty.Register(nameof(StretchItems), typeof(bool), typeof(VirtualizingWrapPanel), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsArrange));

        public static readonly DependencyProperty HorizontalGroupOffsetProperty = DependencyProperty.Register(nameof(HorizontalGroupOffset), typeof(double), typeof(VirtualizingWrapPanel), new FrameworkPropertyMetadata(6d, FrameworkPropertyMetadataOptions.AffectsMeasure));

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
        /// it is strongly recommended to also set the <see cref="ItemSizeProvider"/> property. Otherwise, the performance 
        /// can be very poor and the position of the items is not always guaranteed to be correct.
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

        public double HorizontalGroupOffset { get => (double)GetValue(HorizontalGroupOffsetProperty); set => SetValue(HorizontalGroupOffsetProperty, value); }

        /// <summary>
        /// Gets value that indicates whether the <see cref="VirtualizingPanel"/> can virtualize items 
        /// that are grouped or organized in a hierarchy.
        /// </summary>
        /// <returns>always true for <see cref="VirtualizingWrapPanel"/></returns>
        protected override bool CanHierarchicallyScrollAndVirtualizeCore => true;

        protected override bool HasLogicalOrientation => true;

        protected override Orientation LogicalOrientation => Orientation == Orientation.Horizontal ? Orientation.Vertical : Orientation.Horizontal;

        private readonly VirtualizingPanelWrapper internalChildrenWrapper;
        private readonly IItemContainerManger itemContainerManager;
        private readonly VirtualizingWrapPanelModel model;

        public VirtualizingWrapPanel()
        {
            internalChildrenWrapper = new VirtualizingPanelWrapper(
                () => InternalChildren,
                container => AddInternalChild(container),
                (index, container) => InsertInternalChild(index, container),
                index => RemoveInternalChildRange(index, 1),
                () => ItemContainerGenerator
            );

            itemContainerManager = new ItemContainerManager(internalChildrenWrapper);

            model = new VirtualizingWrapPanelModel(itemContainerManager);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            if (ShouldIgnoreMeasure())
            {
                return availableSize;
            }

            itemContainerManager.IsRecycling = IsRecycling;
            model.Items = Items;
            model.Orientation = Orientation;
            model.FixedItemSize = ItemSize;
            model.ItemSizeProvider = ItemSizeProvider;
            model.AllowDifferentSizedItems = AllowDifferentSizedItems;

            Size desiredSize;

            if (ItemsOwner is IHierarchicalVirtualizationAndScrollInfo groupItem)
            {
                var constraints = groupItem.Constraints;

                var viewportSize = groupItem.Constraints.Viewport.Size;
                var headerSize = groupItem.HeaderDesiredSizes.PixelSize;
                double viewportWidth = Math.Max(viewportSize.Width - 2 * HorizontalGroupOffset, 0);
                double viewporteHeight = Math.Max(viewportSize.Height - headerSize.Height, 0);
                viewportSize = new Size(viewportWidth, viewporteHeight);

                model.CacheLength = groupItem.Constraints.CacheLength;
                model.CacheLengthUnit = groupItem.Constraints.CacheLengthUnit;

                // TODO
                Point offset = new Point(Math.Max(0, constraints.Viewport.Location.X), Math.Max(0, constraints.Viewport.Location.Y));

                desiredSize = model.OnMeasure(availableSize, viewportSize, offset);
                desiredSize.Width += 2 * HorizontalGroupOffset;
            }
            else
            {
                model.CacheLength = CacheLength;
                model.CacheLengthUnit = CacheLengthUnit;

                desiredSize = model.OnMeasure(availableSize, availableSize, Offset);
            }

            SetViewportAndExtend(model.ViewportSize, model.Extent);

            if (desiredSize.Height == double.PositiveInfinity)
            {
                Debug.WriteLine(desiredSize);
            }
            return desiredSize;
        }

        private void Orientation_Changed()
        {
            MouseWheelScrollDirection = Orientation == Orientation.Horizontal ? ScrollDirection.Vertical : ScrollDirection.Horizontal;
        }

        protected override void OnItemsChanged(object sender, ItemsChangedEventArgs args)
        {
            model.OnItemsChanged(args.Action);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            model.StretchItems = StretchItems;
            model.SpacingMode = SpacingMode;
            if (ItemsOwner is IHierarchicalVirtualizationAndScrollInfo groupItem)
            {
                var finalSize_ = finalSize;       
                finalSize_.Width -= 2 * HorizontalGroupOffset;
                model.OnArrange(finalSize_, 
                    Orientation == Orientation.Horizontal ? new Point(Offset.X, 0) : new Point(0, Offset.Y));
            }
            else
            {
                model.OnArrange(finalSize, Offset);
            }
            return finalSize;
        }

        protected override void BringIndexIntoView(int index)
        {
            if (index < 0 || index >= Items.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index), $"The argument {nameof(index)} must be >= 0 and < the number of items.");
            }

            var offset = model.FindOffsetOfItem(index);

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
            return -Math.Min(model.GetAverageItemSize().Height * ScrollLineDeltaItem, ViewportSize.Height);
        }

        protected override double GetLineDownScrollAmount()
        {
            return Math.Min(model.GetAverageItemSize().Height * ScrollLineDeltaItem, ViewportSize.Height);
        }

        protected override double GetLineLeftScrollAmount()
        {
            return -Math.Min(model.GetAverageItemSize().Width * ScrollLineDeltaItem, ViewportSize.Width);
        }

        protected override double GetLineRightScrollAmount()
        {
            return Math.Min(model.GetAverageItemSize().Width * ScrollLineDeltaItem, ViewportSize.Width);
        }

        protected override double GetMouseWheelUpScrollAmount()
        {
            return -Math.Min(model.GetAverageItemSize().Height * MouseWheelDeltaItem, ViewportSize.Height);
        }

        protected override double GetMouseWheelDownScrollAmount()
        {
            return Math.Min(model.GetAverageItemSize().Height * MouseWheelDeltaItem, ViewportSize.Height);
        }

        protected override double GetMouseWheelLeftScrollAmount()
        {
            return -Math.Min(model.GetAverageItemSize().Width * MouseWheelDeltaItem, ViewportSize.Width);
        }

        protected override double GetMouseWheelRightScrollAmount()
        {
            return Math.Min(model.GetAverageItemSize().Width * MouseWheelDeltaItem, ViewportSize.Width);
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
    }
}

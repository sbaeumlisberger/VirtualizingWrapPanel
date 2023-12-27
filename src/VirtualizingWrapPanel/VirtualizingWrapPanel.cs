using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Xml.Linq;

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

        public static readonly DependencyProperty HorizontalGroupOffsetProperty = DependencyProperty.Register(nameof(HorizontalGroupOffset), typeof(double), typeof(VirtualizingWrapPanel), new FrameworkPropertyMetadata(5d, FrameworkPropertyMetadataOptions.AffectsMeasure));

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

        public double HorizontalGroupOffset { get => (double)GetValue(HorizontalGroupOffsetProperty); set => SetValue(HorizontalGroupOffsetProperty, value); }

        /// <summary>
        /// Gets value that indicates whether the <see cref="VirtualizingPanel"/> can virtualize items 
        /// that are grouped or organized in a hierarchy.
        /// </summary>
        /// <returns>always true for <see cref="VirtualizingWrapPanel"/></returns>
        protected override bool CanHierarchicallyScrollAndVirtualizeCore => true;

        protected override bool HasLogicalOrientation => true;

        protected override Orientation LogicalOrientation => Orientation == Orientation.Horizontal ? Orientation.Vertical : Orientation.Horizontal;

        private IItemContainerManager ItemContainerManager
        {
            get
            {
                _itemContainerManager ??= new ItemContainerManager(ItemContainerGenerator);
                return _itemContainerManager;
            }
        }
        private IItemContainerManager? _itemContainerManager;

        private VirtualizingWrapPanelModel Model
        {
            get
            {
                if (_model is null)
                {
                    _model = new VirtualizingWrapPanelModel(ItemContainerManager, internalChildrenWrapper);
                    _model.ScrollInfoInvalidated += Model_ScrollInfoInvalidated;
                    _model.MeasureInvalidated += Model_MeasureInvalidated;
                }
                return _model;
            }
        }

        private VirtualizingWrapPanelModel? _model;

        internal override VirtualizingPanelModelBase BaseModel => Model;

        private readonly VirtualizingPanelWrapper internalChildrenWrapper;

        public VirtualizingWrapPanel()
        {
            internalChildrenWrapper = new VirtualizingPanelWrapper(
                child => AddInternalChild(child),
                child => RemoveInternalChildRange(InternalChildren.IndexOf(child), 1)
            );
        }

        private void Model_ScrollInfoInvalidated(object? sender, EventArgs e)
        {
            ScrollOwner?.InvalidateScrollInfo();
        }

        private void Model_MeasureInvalidated(object? sender, EventArgs e)
        {
            InvalidateMeasure();
        }

        private void Orientation_Changed()
        {
            MouseWheelScrollDirection = Orientation == Orientation.Horizontal
                                        ? ScrollDirection.Vertical
                                        : ScrollDirection.Horizontal;
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
            Model.Orientation = Orientation;
            Model.FixedItemSize = ItemSize;
            Model.ItemSizeProvider = ItemSizeProvider;
            Model.AllowDifferentSizedItems = AllowDifferentSizedItems;

            if (ItemsOwner is IHierarchicalVirtualizationAndScrollInfo groupItem)
            {
                var viewport = groupItem.Constraints.Viewport;
                var headerSize = groupItem.HeaderDesiredSizes.PixelSize;           

                double viewportWidth = Math.Max(viewport.Size.Width, 0);
                double viewporteHeight = Orientation == Orientation.Horizontal 
                    ? Math.Max(viewport.Size.Height, 0) 
                    : Math.Max(viewport.Size.Height - headerSize.Height, 0);

                var viewportSize = new Size(viewportWidth, viewporteHeight);

                Margin = new Thickness(-HorizontalGroupOffset, 0, 0, 0);

                Model.CacheLength = groupItem.Constraints.CacheLength;
                Model.CacheLengthUnit = groupItem.Constraints.CacheLengthUnit;

                return Model.OnMeasure(availableSize, viewportSize, viewport.Location);
            }
            else
            {
                Model.CacheLength = CacheLength;
                Model.CacheLengthUnit = CacheLengthUnit;
                return Model.OnMeasure(availableSize);
            }
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            Model.StretchItems = StretchItems;
            Model.SpacingMode = SpacingMode;
            Model.OnArrange(finalSize, ItemsOwner is IHierarchicalVirtualizationAndScrollInfo);     
            return finalSize;
        }

        protected override void BringIndexIntoView(int index)
        {
            Model.BringIndexIntoView(index);
        }
    }
}

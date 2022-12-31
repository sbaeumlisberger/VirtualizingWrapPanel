using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace WpfToolkit.Controls
{
    /// <summary>
    /// Base class for panels which are supporting virtualization.
    /// </summary>
    public abstract class VirtualizingPanelBase : VirtualizingPanel, IScrollInfo
    {
        public static readonly DependencyProperty ScrollLineDeltaProperty = DependencyProperty.Register(nameof(ScrollLineDelta), typeof(double), typeof(VirtualizingPanelBase), new FrameworkPropertyMetadata(16.0));
        public static readonly DependencyProperty MouseWheelDeltaProperty = DependencyProperty.Register(nameof(MouseWheelDelta), typeof(double), typeof(VirtualizingPanelBase), new FrameworkPropertyMetadata(48.0));
        public static readonly DependencyProperty ScrollLineDeltaItemProperty = DependencyProperty.Register(nameof(ScrollLineDeltaItem), typeof(int), typeof(VirtualizingPanelBase), new FrameworkPropertyMetadata(1));
        public static readonly DependencyProperty MouseWheelDeltaItemProperty = DependencyProperty.Register(nameof(MouseWheelDeltaItem), typeof(int), typeof(VirtualizingPanelBase), new FrameworkPropertyMetadata(3));

        public ScrollViewer? ScrollOwner { get; set; }

        public bool CanVerticallyScroll { get; set; }
        public bool CanHorizontallyScroll { get; set; }

        /// <summary>
        /// Scroll line delta for pixel based scrolling. The default value is 16 dp.
        /// </summary>
        public double ScrollLineDelta { get => (double)GetValue(ScrollLineDeltaProperty); set => SetValue(ScrollLineDeltaProperty, value); }

        /// <summary>
        /// Mouse wheel delta for pixel based scrolling. The default value is 48 dp.
        /// </summary>        
        public double MouseWheelDelta { get => (double)GetValue(MouseWheelDeltaProperty); set => SetValue(MouseWheelDeltaProperty, value); }

        /// <summary>
        /// Scroll line delta for item based scrolling. The default value is 1 item.
        /// </summary>
        public int ScrollLineDeltaItem { get => (int)GetValue(ScrollLineDeltaItemProperty); set => SetValue(ScrollLineDeltaItemProperty, value); }

        /// <summary>
        /// Mouse wheel delta for item based scrolling. The default value is 3 items.
        /// </summary> 
        public int MouseWheelDeltaItem { get => (int)GetValue(MouseWheelDeltaItemProperty); set => SetValue(MouseWheelDeltaItemProperty, value); }

        protected ScrollUnit ScrollUnit => GetScrollUnit(ItemsControl);

        /// <summary>
        /// The direction in which the panel scrolls when user turns the mouse wheel.
        /// </summary>
        protected ScrollDirection MouseWheelScrollDirection { get; set; } = ScrollDirection.Vertical;

        protected bool IsVirtualizing => GetIsVirtualizing(ItemsControl);

        protected VirtualizationMode VirtualizationMode => GetVirtualizationMode(ItemsControl);

        /// <summary>
        /// Returns true if the panel is in VirtualizationMode.Recycling, otherwise false.
        /// </summary>
        protected bool IsRecycling => VirtualizationMode == VirtualizationMode.Recycling;

        /// <summary>
        /// The cache length before and after the viewport. 
        /// </summary>
        protected VirtualizationCacheLength CacheLength => GetCacheLength(ItemsControl);

        /// <summary>
        /// The Unit of the cache length. Can be Pixel, Item or Page. 
        /// When the ItemsOwner is a group item it can only be pixel or item.
        /// </summary>
        protected VirtualizationCacheLengthUnit CacheLengthUnit => GetCacheLengthUnit(ItemsControl);

        /// <summary>
        /// The ItemsControl (e.g. ListView).
        /// </summary>
        protected ItemsControl ItemsControl => ItemsControl.GetItemsOwner(this);

        /// <summary>
        /// The ItemsControl (e.g. ListView) or if the ItemsControl is grouping a GroupItem.
        /// </summary>
        protected DependencyObject ItemsOwner
        {
            get
            {
                if (_itemsOwner is null)
                {
                    /* Use reflection to access internal method because the public 
                     * GetItemsOwner method does always return the itmes control instead 
                     * of the real items owner for example the group item when grouping */
                    MethodInfo getItemsOwnerInternalMethod = typeof(ItemsControl).GetMethod(
                        "GetItemsOwnerInternal",
                        BindingFlags.Static | BindingFlags.NonPublic,
                        null,
                        new Type[] { typeof(DependencyObject) },
                        null
                    )!;
                    _itemsOwner = (DependencyObject)getItemsOwnerInternalMethod.Invoke(null, new object[] { this })!;
                }
                return _itemsOwner;
            }
        }
        private DependencyObject? _itemsOwner;

        protected ReadOnlyCollection<object> Items => ItemContainerGenerator.Items;

        protected new ItemContainerGenerator ItemContainerGenerator
        {
            get
            {
                if (_itemContainerGenerator is null)
                {
                    // The ItemContainerGenerator is null until InternalChildren is accessed at least one time.
                    var children = InternalChildren;
                    _itemContainerGenerator = base.ItemContainerGenerator.GetItemContainerGeneratorForPanel(this);

                }
                return _itemContainerGenerator;
            }
        }
        private ItemContainerGenerator? _itemContainerGenerator;

        public double ExtentWidth => BaseModel.Extent.Width;
        public double ExtentHeight => BaseModel.Extent.Height;

        public double HorizontalOffset => BaseModel.ScrollOffset.X;
        public double VerticalOffset => BaseModel.ScrollOffset.Y;

        public double ViewportWidth => BaseModel.ViewportSize.Width;
        public double ViewportHeight => BaseModel.ViewportSize.Height;

        internal abstract VirtualizingPanelModelBase BaseModel { get; }

        private Visibility previousVerticalScrollBarVisibility = Visibility.Collapsed;
        private Visibility previousHorizontalScrollBarVisibility = Visibility.Collapsed;
        protected bool ShouldIgnoreMeasure()
        {
            /* Sometimes when scrolling the scrollbar gets hidden without any reason. In this case the "IsMeasureValid" 
            * property of the ScrollOwner is false. To prevent a infinite circle the mesasure call is ignored. */
            if (ScrollOwner != null)
            {
                bool verticalScrollBarGotHidden = ScrollOwner.VerticalScrollBarVisibility == ScrollBarVisibility.Auto
                    && ScrollOwner.ComputedVerticalScrollBarVisibility != Visibility.Visible
                    && ScrollOwner.ComputedVerticalScrollBarVisibility != previousVerticalScrollBarVisibility;

                bool horizontalScrollBarGotHidden = ScrollOwner.HorizontalScrollBarVisibility == ScrollBarVisibility.Auto
                   && ScrollOwner.ComputedHorizontalScrollBarVisibility != Visibility.Visible
                   && ScrollOwner.ComputedHorizontalScrollBarVisibility != previousHorizontalScrollBarVisibility;

                previousVerticalScrollBarVisibility = ScrollOwner.ComputedVerticalScrollBarVisibility;
                previousHorizontalScrollBarVisibility = ScrollOwner.ComputedHorizontalScrollBarVisibility;

                if (!ScrollOwner.IsMeasureValid && verticalScrollBarGotHidden || horizontalScrollBarGotHidden)
                {
                    return true;
                }
            }
            return false;
        }

        public virtual Rect MakeVisible(Visual visual, Rect rectangle)
        {
            Point pos = visual.TransformToAncestor(this).Transform(BaseModel.ScrollOffset);

            double scrollAmountX = 0;
            double scrollAmountY = 0;

            if (pos.X < HorizontalOffset)
            {
                scrollAmountX = -(HorizontalOffset - pos.X);
            }
            else if ((pos.X + rectangle.Width) > (HorizontalOffset + ViewportWidth))
            {
                double notVisibleX = (pos.X + rectangle.Width) - (HorizontalOffset + ViewportWidth);
                double maxScrollX = pos.X - HorizontalOffset; // keep left of the visual visible
                scrollAmountX = Math.Min(notVisibleX, maxScrollX);
            }

            if (pos.Y < VerticalOffset)
            {
                scrollAmountY = -(VerticalOffset - pos.Y);
            }
            else if ((pos.Y + rectangle.Height) > (VerticalOffset + ViewportHeight))
            {
                double notVisibleY = (pos.Y + rectangle.Height) - (VerticalOffset + ViewportHeight);
                double maxScrollY = pos.Y - VerticalOffset; // keep top of the visual visible
                scrollAmountY = Math.Min(notVisibleY, maxScrollY);
            }

            BaseModel.SetHorizontalOffset(HorizontalOffset + scrollAmountX);
            BaseModel.SetVerticalOffset(VerticalOffset + scrollAmountY);

            double visibleRectWidth = Math.Min(rectangle.Width, ViewportWidth);
            double visibleRectHeight = Math.Min(rectangle.Height, ViewportHeight);

            return new Rect(scrollAmountX, scrollAmountY, visibleRectWidth, visibleRectHeight);
        }

        public void LineUp() => BaseModel.LineUp();
        public void LineDown() => BaseModel.LineDown();
        public void LineLeft() => BaseModel.LineLeft();
        public void LineRight() => BaseModel.LineRight();

        public void MouseWheelUp() => BaseModel.MouseWheelUp();
        public void MouseWheelDown() => BaseModel.MouseWheelDown();
        public void MouseWheelLeft() => BaseModel.MouseWheelLeft();
        public void MouseWheelRight() => BaseModel.MouseWheelRight();

        public void PageUp() => BaseModel.PageUp();
        public void PageDown() => BaseModel.PageDown();
        public void PageLeft() => BaseModel.PageLeft();
        public void PageRight() => BaseModel.PageRight();

        public void SetHorizontalOffset(double offset) => BaseModel.SetHorizontalOffset(offset);
        public void SetVerticalOffset(double offset) => BaseModel?.SetVerticalOffset(offset);
    }
}

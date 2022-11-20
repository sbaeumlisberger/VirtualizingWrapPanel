using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

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
        public double ScrollLineDeltaItem { get => (int)GetValue(ScrollLineDeltaItemProperty); set => SetValue(ScrollLineDeltaItemProperty, value); }

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

        protected ReadOnlyCollection<object> Items => ((ItemContainerGenerator)ItemContainerGenerator).Items;

        protected new IRecyclingItemContainerGenerator ItemContainerGenerator
        {
            get
            {
                if (_itemContainerGenerator is null)
                {
                    /* Because of a bug in the framework the ItemContainerGenerator 
                     * is null until InternalChildren accessed at least one time. */
                    var children = InternalChildren;
                    _itemContainerGenerator = (IRecyclingItemContainerGenerator)base.ItemContainerGenerator;
                }
                return _itemContainerGenerator;
            }
        }
        private IRecyclingItemContainerGenerator? _itemContainerGenerator;

        public double ExtentWidth => Extent.Width;
        public double ExtentHeight => Extent.Height;
        protected Size Extent { get; private set; } = new Size(0, 0);

        public double HorizontalOffset => Offset.X;
        public double VerticalOffset => Offset.Y;
        protected Point Offset { get; private set; } = new Point(0, 0);

        public double ViewportWidth => ViewportSize.Width;
        public double ViewportHeight => ViewportSize.Height;
        protected Size ViewportSize { get; private set; } = new Size(0, 0);

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

        protected virtual void SetViewportAndExtend(Size viewportSize, Size extent)
        {
            bool invalidateScrollInfo = false;

            if (extent != Extent)
            {
                Extent = extent;
                invalidateScrollInfo = true;

            }
            if (viewportSize != ViewportSize)
            {
                ViewportSize = viewportSize;
                invalidateScrollInfo = true;
            }

            if (ViewportHeight != 0 && VerticalOffset != 0 && VerticalOffset + ViewportHeight + 1 >= ExtentHeight)
            {
                Offset = new Point(Offset.X, extent.Height - viewportSize.Height);
                invalidateScrollInfo = true;
            }
            if (ViewportWidth != 0 && HorizontalOffset != 0 && HorizontalOffset + ViewportWidth + 1 >= ExtentWidth)
            {
                Offset = new Point(extent.Width - viewportSize.Width, Offset.Y);
                invalidateScrollInfo = true;
            }

            if (invalidateScrollInfo)
            {
                ScrollOwner?.InvalidateScrollInfo();
            }
        }

        public virtual Rect MakeVisible(Visual visual, Rect rectangle)
        {
            Point pos = visual.TransformToAncestor(this).Transform(Offset);

            double scrollAmountX = 0;
            double scrollAmountY = 0;

            if (pos.X < Offset.X)
            {
                scrollAmountX = -(Offset.X - pos.X);
            }
            else if ((pos.X + rectangle.Width) > (Offset.X + ViewportSize.Width))
            {
                double notVisibleX = (pos.X + rectangle.Width) - (Offset.X + ViewportSize.Width);
                double maxScrollX = pos.X - Offset.X; // keep left of the visual visible
                scrollAmountX = Math.Min(notVisibleX, maxScrollX);
            }

            if (pos.Y < Offset.Y)
            {
                scrollAmountY = -(Offset.Y - pos.Y);
            }
            else if ((pos.Y + rectangle.Height) > (Offset.Y + ViewportSize.Height))
            {
                double notVisibleY = (pos.Y + rectangle.Height) - (Offset.Y + ViewportSize.Height);
                double maxScrollY = pos.Y - Offset.Y; // keep top of the visual visible
                scrollAmountY = Math.Min(notVisibleY, maxScrollY);
            }

            SetHorizontalOffset(Offset.X + scrollAmountX);
            SetVerticalOffset(Offset.Y + scrollAmountY);

            double visibleRectWidth = Math.Min(rectangle.Width, ViewportSize.Width);
            double visibleRectHeight = Math.Min(rectangle.Height, ViewportSize.Height);

            return new Rect(scrollAmountX, scrollAmountY, visibleRectWidth, visibleRectHeight);
        }

        protected virtual GeneratorPosition GetGeneratorPositionFromChildIndex(int childIndex)
        {
            return new GeneratorPosition(childIndex, 0);
        }

        public void SetVerticalOffset(double offset)
        {
            if (offset < 0 || ViewportSize.Height >= Extent.Height)
            {
                offset = 0;
            }
            else if (offset + ViewportSize.Height >= Extent.Height)
            {
                offset = Extent.Height - ViewportSize.Height;
            }
            this.Offset = new Point(this.Offset.X, offset);
            ScrollOwner?.InvalidateScrollInfo();
            InvalidateMeasure();
        }

        public void SetHorizontalOffset(double offset)
        {
            if (offset < 0 || ViewportSize.Width >= Extent.Width)
            {
                offset = 0;
            }
            else if (offset + ViewportSize.Width >= Extent.Width)
            {
                offset = Extent.Width - ViewportSize.Width;
            }
            this.Offset = new Point(offset, this.Offset.Y);
            ScrollOwner?.InvalidateScrollInfo();
            InvalidateMeasure();
        }

        protected void ScrollVertical(double amount)
        {
            SetVerticalOffset(VerticalOffset + amount);
        }

        protected void ScrollHorizontal(double amount)
        {
            SetHorizontalOffset(HorizontalOffset + amount);
        }

        public void LineUp() => ScrollVertical(ScrollUnit == ScrollUnit.Pixel ? -ScrollLineDelta : GetLineUpScrollAmount());
        public void LineDown() => ScrollVertical(ScrollUnit == ScrollUnit.Pixel ? ScrollLineDelta : GetLineDownScrollAmount());
        public void LineLeft() => ScrollHorizontal(ScrollUnit == ScrollUnit.Pixel ? -ScrollLineDelta : GetLineLeftScrollAmount());
        public void LineRight() => ScrollHorizontal(ScrollUnit == ScrollUnit.Pixel ? ScrollLineDelta : GetLineRightScrollAmount());

        public void MouseWheelUp()
        {
            if (MouseWheelScrollDirection == ScrollDirection.Vertical)
            {
                ScrollVertical(ScrollUnit == ScrollUnit.Pixel ? -MouseWheelDelta : GetMouseWheelUpScrollAmount());
            }
            else
            {
                MouseWheelLeft();
            }
        }

        public void MouseWheelDown()
        {
            if (MouseWheelScrollDirection == ScrollDirection.Vertical)
            {
                ScrollVertical(ScrollUnit == ScrollUnit.Pixel ? MouseWheelDelta : GetMouseWheelDownScrollAmount());
            }
            else
            {
                MouseWheelRight();
            }
        }

        public void MouseWheelLeft() => ScrollHorizontal(ScrollUnit == ScrollUnit.Pixel ? -MouseWheelDelta : GetMouseWheelLeftScrollAmount());
        public void MouseWheelRight() => ScrollHorizontal(ScrollUnit == ScrollUnit.Pixel ? MouseWheelDelta : GetMouseWheelRightScrollAmount());

        public void PageUp() => ScrollVertical(ScrollUnit == ScrollUnit.Pixel ? -ViewportHeight : GetPageUpScrollAmount());
        public void PageDown() => ScrollVertical(ScrollUnit == ScrollUnit.Pixel ? ViewportHeight : GetPageDownScrollAmount());
        public void PageLeft() => ScrollHorizontal(ScrollUnit == ScrollUnit.Pixel ? -ViewportHeight : GetPageLeftScrollAmount());
        public void PageRight() => ScrollHorizontal(ScrollUnit == ScrollUnit.Pixel ? ViewportHeight : GetPageRightScrollAmount());

        protected abstract double GetLineUpScrollAmount();
        protected abstract double GetLineDownScrollAmount();
        protected abstract double GetLineLeftScrollAmount();
        protected abstract double GetLineRightScrollAmount();

        protected abstract double GetMouseWheelUpScrollAmount();
        protected abstract double GetMouseWheelDownScrollAmount();
        protected abstract double GetMouseWheelLeftScrollAmount();
        protected abstract double GetMouseWheelRightScrollAmount();

        protected abstract double GetPageUpScrollAmount();
        protected abstract double GetPageDownScrollAmount();
        protected abstract double GetPageLeftScrollAmount();
        protected abstract double GetPageRightScrollAmount();
    }
}

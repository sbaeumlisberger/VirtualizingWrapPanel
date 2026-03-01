using System;
using System.Collections.ObjectModel;
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
        public const double DefaultScrollLineDeltaPixel = 16.0;
        public const double DefaultMouseWheelDeltaPixel = 48.0;
        public const int DefaultScrollLineDeltaItem = 1;
        public const int DefaultMouseWheelDeltaItem = 3;

        public static readonly DependencyProperty ScrollLineDeltaProperty = DependencyProperty.Register(nameof(ScrollLineDelta), typeof(double), typeof(VirtualizingPanelBase), new FrameworkPropertyMetadata(DefaultScrollLineDeltaPixel));
        public static readonly DependencyProperty MouseWheelDeltaProperty = DependencyProperty.Register(nameof(MouseWheelDelta), typeof(double), typeof(VirtualizingPanelBase), new FrameworkPropertyMetadata(DefaultMouseWheelDeltaPixel));
        public static readonly DependencyProperty ScrollLineDeltaItemProperty = DependencyProperty.Register(nameof(ScrollLineDeltaItem), typeof(int), typeof(VirtualizingPanelBase), new FrameworkPropertyMetadata(DefaultScrollLineDeltaItem));
        public static readonly DependencyProperty MouseWheelDeltaItemProperty = DependencyProperty.Register(nameof(MouseWheelDeltaItem), typeof(int), typeof(VirtualizingPanelBase), new FrameworkPropertyMetadata(DefaultMouseWheelDeltaItem));

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
        /// The ItemsControl (e.g. ListView).
        /// </summary>
        public ItemsControl ItemsControl => ItemsControl.GetItemsOwner(this);

        /// <summary>
        /// The ItemsControl (e.g. ListView) or if the ItemsControl is grouping a GroupItem.
        /// </summary>
        protected DependencyObject ItemsOwner => TemplatedParent is ItemsPresenter itemsPresenter ? itemsPresenter.TemplatedParent : TemplatedParent;

        protected ReadOnlyCollection<object> Items => ItemContainerGenerator.Items;

        protected IRecyclingItemContainerGenerator RecyclingItemContainerGenerator => ItemContainerGenerator;

        protected new ItemContainerGenerator ItemContainerGenerator
        {
            get
            {
                if (field is null)
                {
                    // The ItemContainerGenerator is null until InternalChildren is accessed at least one time.
                    _ = InternalChildren;

                    if (base.ItemContainerGenerator is null)
                    {
                        throw new InvalidOperationException($"ItemContainerGenerator is null. Make sure that"
                            + $" the {GetType().Name} is used as the ItemsPanel of an ItemsControl such as a ListView"
                            + $" or the GridView and VirtualizingItemsControl that are part of this package.");
                    }

                    field = base.ItemContainerGenerator.GetItemContainerGeneratorForPanel(this);
                }
                return field;
            }
        }

        public double ExtentWidth { get; protected set; }
        public double ExtentHeight { get; protected set; }

        public double HorizontalOffset { get; protected set; }
        public double VerticalOffset { get; protected set; }

        public double ViewportWidth { get; protected set; }
        public double ViewportHeight { get; protected set; }

        protected void InvalidateScrollInfo()
        {
            ScrollOwner?.InvalidateScrollInfo();
        }

        protected void VerifyItemsControl()
        {
            if (GetIsVirtualizing(ItemsControl) == false)
            {
                throw new InvalidOperationException("VirtualizingPanel.IsVirtualizing must be true." +
                    " Make sure VirtualizingPanel.IsVirtualizing=\"True\" is set on the ItemsControl.");
            }

            if (ItemsOwner is IHierarchicalVirtualizationAndScrollInfo && GetIsVirtualizingWhenGrouping(ItemsControl) == false)
            {
                throw new InvalidOperationException("VirtualizingPanel.IsVirtualizingWhenGrouping must be true." +
                    " Make sure VirtualizingPanel.IsVirtualizingWhenGrouping=\"True\" is set on the ItemsControl.");
            }

            if (ScrollViewer.GetCanContentScroll(ItemsControl) == false)
            {
                throw new InvalidOperationException("ScrollViewer.CanContentScroll must be true." +
                    " Make sure ScrollViewer.CanContentScroll=\"True\" is set on the ItemsControl.");
            }
        }

        public virtual Rect MakeVisible(Visual visual, Rect rectangle)
        {
            var transformedBounds = visual.TransformToAncestor(this).TransformBounds(rectangle);

            double offsetX = 0;
            double offsetY = 0;

            double visibleX = 0;
            double visibleY = 0;
            double visibleWidth = Math.Min(rectangle.Width, ViewportWidth);
            double visibleHeight = Math.Min(rectangle.Height, ViewportHeight);

            if (transformedBounds.Left < 0)
            {
                offsetX = transformedBounds.Left;
            }
            else if (transformedBounds.Right > ViewportWidth)
            {
                offsetX = Math.Min(transformedBounds.Right - ViewportWidth, transformedBounds.Left);

                if (rectangle.Width > ViewportWidth)
                {
                    visibleX = rectangle.Width - ViewportWidth;
                }
            }

            if (transformedBounds.Top < 0)
            {
                offsetY = transformedBounds.Top;
            }
            else if (transformedBounds.Bottom > ViewportHeight)
            {
                offsetY = Math.Min(transformedBounds.Bottom - ViewportHeight, transformedBounds.Top);

                if (rectangle.Height > ViewportHeight)
                {
                    visibleY = rectangle.Height - ViewportHeight;
                }
            }

            SetHorizontalOffset(HorizontalOffset + offsetX);
            SetVerticalOffset(VerticalOffset + offsetY);

            return new Rect(visibleX, visibleY, visibleWidth, visibleHeight);
        }

        public void SetVerticalOffset(double offset)
        {
            if (offset < 0 || ViewportHeight >= ExtentHeight)
            {
                offset = 0;
            }
            else if (offset + ViewportHeight >= ExtentHeight)
            {
                offset = ExtentHeight - ViewportHeight;
            }
            if (offset != VerticalOffset)
            {
                VerticalOffset = offset;
                InvalidateScrollInfo();
                InvalidateMeasure();
            }
        }

        public void SetHorizontalOffset(double offset)
        {
            if (offset < 0 || ViewportWidth >= ExtentWidth)
            {
                offset = 0;
            }
            else if (offset + ViewportWidth >= ExtentWidth)
            {
                offset = ExtentWidth - ViewportWidth;
            }
            if (offset != HorizontalOffset)
            {
                HorizontalOffset = offset;
                InvalidateScrollInfo();
                InvalidateMeasure();
            }
        }

        public virtual void LineUp()
        {
            ScrollVertical(ScrollUnit == ScrollUnit.Pixel ? -ScrollLineDelta : GetLineUpScrollAmount());
        }
        public virtual void LineDown()
        {
            ScrollVertical(ScrollUnit == ScrollUnit.Pixel ? ScrollLineDelta : GetLineDownScrollAmount());
        }
        public virtual void LineLeft()
        {
            ScrollHorizontal(ScrollUnit == ScrollUnit.Pixel ? -ScrollLineDelta : GetLineLeftScrollAmount());
        }
        public virtual void LineRight()
        {
            ScrollHorizontal(ScrollUnit == ScrollUnit.Pixel ? ScrollLineDelta : GetLineRightScrollAmount());
        }

        public virtual void MouseWheelUp()
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
        public virtual void MouseWheelDown()
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
        public virtual void MouseWheelLeft()
        {
            ScrollHorizontal(ScrollUnit == ScrollUnit.Pixel ? -MouseWheelDelta : GetMouseWheelLeftScrollAmount());
        }
        public virtual void MouseWheelRight()
        {
            ScrollHorizontal(ScrollUnit == ScrollUnit.Pixel ? MouseWheelDelta : GetMouseWheelRightScrollAmount());
        }

        public virtual void PageUp()
        {
            ScrollVertical(ScrollUnit == ScrollUnit.Pixel ? -ViewportHeight : GetPageUpScrollAmount());
        }
        public virtual void PageDown()
        {
            ScrollVertical(ScrollUnit == ScrollUnit.Pixel ? ViewportHeight : GetPageDownScrollAmount());
        }
        public virtual void PageLeft()
        {
            ScrollHorizontal(ScrollUnit == ScrollUnit.Pixel ? -ViewportWidth : GetPageLeftScrollAmount());
        }
        public virtual void PageRight()
        {
            ScrollHorizontal(ScrollUnit == ScrollUnit.Pixel ? ViewportWidth : GetPageRightScrollAmount());
        }

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

        private void ScrollVertical(double amount)
        {
            SetVerticalOffset(VerticalOffset + amount);
        }

        private void ScrollHorizontal(double amount)
        {
            SetHorizontalOffset(HorizontalOffset + amount);
        }
    }
}

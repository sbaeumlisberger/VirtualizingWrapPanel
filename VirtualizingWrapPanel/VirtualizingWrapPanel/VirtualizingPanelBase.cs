using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace WpfToolkit.Controls {

    /// <summary>
    /// Base class for panels which are supporting virtualization.
    /// </summary>
    public abstract class VirtualizingPanelBase : VirtualizingPanel, IScrollInfo {

        /*
         Scroll deltas for pixel based scrolling.
         Taken from https://referencesource.microsoft.com/#PresentationFramework/src/Framework/System/Windows/Controls/ScrollViewer.cs,278cafe26a902287.
        */
        public const double ScrollLineDelta = 16.0;
        public const double MouseWheelDelta = 48.0;

        public ScrollViewer ScrollOwner { get; set; }

        public bool CanVerticallyScroll { get; set; }
        public bool CanHorizontallyScroll { get; set; }

        public double ExtentWidth => Extent.Width;
        public double ExtentHeight => Extent.Height;

        public double ViewportWidth => Viewport.Width;
        public double ViewportHeight => Viewport.Height;

        public double HorizontalOffset => Offset.X;
        public double VerticalOffset => Offset.Y;

        protected bool IsVirtualizing => GetIsVirtualizing(ItemsControl);
        protected VirtualizationMode VirtualizationMode => GetVirtualizationMode(ItemsControl);
        protected ScrollUnit ScrollUnit => GetScrollUnit(ItemsControl);

        protected VirtualizationCacheLength CacheLength { get; private set; }
        protected VirtualizationCacheLengthUnit CacheLengthUnit { get; private set; }

        /// <summary>Returns true if the panel is in VirtualizationMode.Recycling, otherwise false.</summary>
        protected bool IsRecycling => VirtualizationMode == VirtualizationMode.Recycling;

        /// <summary>The direction in which the panel scrolls when user turns the mouse wheel.</summary>
        protected ScrollDirection MouseWheelScrollDirection { get; set; } = ScrollDirection.Vertical;

        protected override bool CanHierarchicallyScrollAndVirtualizeCore => true;

        /// <summary>The ItemsControl (e.g. ListView).</summary>
        protected ItemsControl ItemsControl => ItemsControl.GetItemsOwner(this);

        /// <summary>The ItemsControl (e.g. ListView) or if the ItemsControl is grouping a GroupItem.</summary>
        protected DependencyObject ItemsOwner {
            get {
                if (_itemsOwner == null)
                {
                    /* Use reflection to access internal method because the public 
                     * GetItemsOwner method does always return the itmes control instead 
                     * of the real items owner for example the group item when grouping */
                    _itemsOwner = (DependencyObject)typeof(ItemsControl).GetMethod(
                       "GetItemsOwnerInternal",
                       BindingFlags.Static | BindingFlags.NonPublic,
                       null,
                       new Type[] { typeof(DependencyObject) },
                       null
                    ).Invoke(null, new object[] { this });
                }
                return _itemsOwner;
            }
        }

        protected ReadOnlyCollection<object> Items => ((ItemContainerGenerator)ItemContainerGenerator).Items;

        protected new IRecyclingItemContainerGenerator ItemContainerGenerator {
            get {
                if (_itemContainerGenerator == null)
                {
                    /* Because of a bug in the framework the ItemContainerGenerator 
                     * is null until InternalChildren accessed at least one time. */
                    var children = InternalChildren;
                    _itemContainerGenerator = (IRecyclingItemContainerGenerator)base.ItemContainerGenerator;
                }
                return _itemContainerGenerator;
            }
        }

        private DependencyObject _itemsOwner;

        private IRecyclingItemContainerGenerator _itemContainerGenerator;

        protected Size Extent { get; private set; } = new Size(0, 0);
        protected Size Viewport { get; private set; } = new Size(0, 0);
        protected Point Offset { get; private set; } = new Point(0, 0);

        protected ItemRange ItemRange { get; set; }

        public void SetVerticalOffset(double offset)
        {
            if (offset < 0 || Viewport.Height >= Extent.Height)
            {
                offset = 0;
            }
            else if (offset + Viewport.Height >= Extent.Height)
            {
                offset = Extent.Height - Viewport.Height;
            }
            Offset = new Point(Offset.X, offset);
            ScrollOwner?.InvalidateScrollInfo();
            InvalidateMeasure();
        }

        public void SetHorizontalOffset(double offset)
        {
            if (offset < 0 || Viewport.Width >= Extent.Width)
            {
                offset = 0;
            }
            else if (offset + Viewport.Width >= Extent.Width)
            {
                offset = Extent.Width - Viewport.Width;
            }
            Offset = new Point(offset, Offset.Y);
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

        protected virtual void UpdateScrollInfo(Size availableSize, Size extent)
        {
            if (ViewportHeight != 0 && VerticalOffset != 0 && VerticalOffset + ViewportHeight + 1 >= ExtentHeight)
            {
                Offset = new Point(Offset.X, extent.Height - availableSize.Height);
                ScrollOwner?.InvalidateScrollInfo();
            }
            if (ViewportWidth != 0 && HorizontalOffset != 0 && HorizontalOffset + ViewportWidth + 1 >= ExtentWidth)
            {
                Offset = new Point(extent.Width - availableSize.Width, Offset.Y);
                ScrollOwner?.InvalidateScrollInfo();
            }
            if (availableSize != Viewport)
            {
                Viewport = availableSize;
                ScrollOwner?.InvalidateScrollInfo();
            }
            if (extent != Extent)
            {
                Extent = extent;
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
            else if ((pos.X + rectangle.Width) > (Offset.X + Viewport.Width))
            {
                scrollAmountX = (pos.X + rectangle.Width) - (Offset.X + Viewport.Width);
            }

            if (pos.Y < Offset.Y)
            {
                scrollAmountY = -(Offset.Y - pos.Y);
            }
            else if ((pos.Y + rectangle.Height) > (Offset.Y + Viewport.Height))
            {
                scrollAmountY = (pos.Y + rectangle.Height) - (Offset.Y + Viewport.Height);
            }

            SetHorizontalOffset(Offset.X + scrollAmountX);

            SetVerticalOffset(Offset.Y + scrollAmountY);

            double visibleRectWidth = Math.Min(rectangle.Width, Viewport.Width);
            double visibleRectHeight = Math.Min(rectangle.Height, Viewport.Height);

            return new Rect(scrollAmountX, scrollAmountY, visibleRectWidth, visibleRectHeight);
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

        protected int GetItemIndexFromChildIndex(int childIndex)
        {
            var generatorPosition = GetGeneratorPositionFromChildIndex(childIndex);
            return ItemContainerGenerator.IndexFromGeneratorPosition(generatorPosition);
        }

        protected virtual GeneratorPosition GetGeneratorPositionFromChildIndex(int childIndex)
        {
            return new GeneratorPosition(childIndex, 0);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            Size extent = CalculateExtent();

            double desiredWidth = Math.Min(availableSize.Width, extent.Width);
            double desiredHeight = Math.Min(availableSize.Height, extent.Height);

            Size desiredSize = new Size(desiredWidth, desiredHeight);

            if (ItemsOwner is IHierarchicalVirtualizationAndScrollInfo groupItem)
            {
                Extent = extent; // logical??
                Offset = groupItem.Constraints.Viewport.Location;
                Viewport = groupItem.Constraints.Viewport.Size;
                CacheLength = groupItem.Constraints.CacheLength;
                CacheLengthUnit = groupItem.Constraints.CacheLengthUnit; // Can be Item or Pixel
            }
            else
            {
                UpdateScrollInfo(desiredSize, extent);
                CacheLength = GetCacheLength(ItemsOwner);
                CacheLengthUnit = GetCacheLengthUnit(ItemsOwner); // Can be Page, Item or Pixel
            }

            ItemRange = UpdateItemRange();

            RealizeItems();

            // TODO: working as GroupStyle.ItemsPanel
            //foreach (UIElement child in InternalChildren) {
            //    if (child is IHierarchicalVirtualizationAndScrollInfo groupItem2) {
            //        double viewportWidth = Math.Min(Viewport.Width, child.DesiredSize.Width);
            //        double viewportHeight = Math.Min(Viewport.Height, child.DesiredSize.Height);
            //        var viewport = new Size(viewportWidth, viewportHeight);
            //        var c = new HierarchicalVirtualizationConstraints(new VirtualizationCacheLength(100), VirtualizationCacheLengthUnit.Pixel, new Rect(Offset, viewport));
            //        if (c != groupItem2.Constraints) {
            //            groupItem2.Constraints = c;
            //            child.InvalidateMeasure();
            //            child.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            //            Debug.WriteLine(Offset.X + ", " + Offset.Y + " :: " + viewport.Width + ", " + viewport.Height);
            //            InvalidateMeasure();
            //        }
            //    }               
            //}

            VirtualizeItems();

            return desiredSize;
        }

        protected virtual void RealizeItems()
        {
            var startPosition = ItemContainerGenerator.GeneratorPositionFromIndex(ItemRange.StartIndex);

            int childIndex = startPosition.Offset == 0 ? startPosition.Index : startPosition.Index + 1;

            using (ItemContainerGenerator.StartAt(startPosition, GeneratorDirection.Forward, true))
            {
                for (int i = ItemRange.StartIndex; i <= ItemRange.EndIndex; i++, childIndex++)
                {
                    UIElement child = (UIElement)ItemContainerGenerator.GenerateNext(out bool isNewlyRealized);
                    if (isNewlyRealized || /*recycled*/!InternalChildren.Contains(child))
                    {
                        if (childIndex >= InternalChildren.Count)
                        {
                            AddInternalChild(child);
                        }
                        else
                        {
                            InsertInternalChild(childIndex, child);
                        }
                        ItemContainerGenerator.PrepareItemContainer(child);

                        child.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                    }
                }
            }
        }

        protected virtual void VirtualizeItems()
        {
            for (int childIndex = InternalChildren.Count - 1; childIndex >= 0; childIndex--)
            {

                var generatorPosition = GetGeneratorPositionFromChildIndex(childIndex);

                int itemIndex = ItemContainerGenerator.IndexFromGeneratorPosition(generatorPosition);

                if (!ItemRange.Contains(itemIndex))
                {

                    if (VirtualizationMode == VirtualizationMode.Recycling)
                    {
                        ItemContainerGenerator.Recycle(generatorPosition, 1);
                    }
                    else
                    {
                        ItemContainerGenerator.Remove(generatorPosition, 1);
                    }
                    RemoveInternalChildRange(childIndex, 1);
                }
            }
        }

        protected abstract Size CalculateExtent();

        protected abstract ItemRange UpdateItemRange();

    }

}
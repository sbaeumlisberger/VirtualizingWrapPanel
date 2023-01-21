using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace WpfToolkit.Controls;
internal abstract class VirtualizingPanelModelBase
{
    public event EventHandler<EventArgs>? ScrollInfoInvalidated;
    public event EventHandler<EventArgs>? MeasureInvalidated;

    public Size Extent { get; protected set; } = new Size(0, 0);
    public Size ViewportSize { get; protected set; } = new Size(0, 0);
    public Point ScrollOffset { get; protected set; } = new Point(0, 0);
    public ScrollUnit ScrollUnit { get; set; } = ScrollUnit.Pixel;
    public VirtualizationCacheLength CacheLength { get; set; } = new VirtualizationCacheLength(1, 1);
    public VirtualizationCacheLengthUnit CacheLengthUnit { get; set; } = VirtualizationCacheLengthUnit.Page;
    public double ScrollLineDelta { get; set; } = 16;
    public double MouseWheelDelta { get; set; } = 48;
    public int ScrollLineDeltaItem { get; set; } = 1;
    public int MouseWheelDeltaItem { get; set; } = 3;
    protected ScrollDirection MouseWheelScrollDirection { get; set; } = ScrollDirection.Vertical;

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
        if (offset != ScrollOffset.Y)
        {
            ScrollOffset = new Point(ScrollOffset.X, offset);
            InvalidateScrollInfo();
            InvalidateMeasure();
        }
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
        if (offset != ScrollOffset.X)
        {
            ScrollOffset = new Point(offset, ScrollOffset.Y);
            InvalidateScrollInfo();
            InvalidateMeasure();
        }
    }

    public void LineUp()
    {
        ScrollVertical(ScrollUnit == ScrollUnit.Pixel ? -ScrollLineDelta : GetLineUpScrollAmount());
    }
    public void LineDown()
    {
        ScrollVertical(ScrollUnit == ScrollUnit.Pixel ? ScrollLineDelta : GetLineDownScrollAmount());
    }
    public void LineLeft()
    {
        ScrollHorizontal(ScrollUnit == ScrollUnit.Pixel ? -ScrollLineDelta : GetLineLeftScrollAmount());
    }
    public void LineRight()
    {
        ScrollHorizontal(ScrollUnit == ScrollUnit.Pixel ? ScrollLineDelta : GetLineRightScrollAmount());
    }

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
    public void MouseWheelLeft()
    {
        ScrollHorizontal(ScrollUnit == ScrollUnit.Pixel ? -MouseWheelDelta : GetMouseWheelLeftScrollAmount());
    }
    public void MouseWheelRight()
    {
        ScrollHorizontal(ScrollUnit == ScrollUnit.Pixel ? MouseWheelDelta : GetMouseWheelRightScrollAmount());
    }

    public void PageUp()
    {
        ScrollVertical(ScrollUnit == ScrollUnit.Pixel ? -ViewportSize.Height : GetPageUpScrollAmount());
    }
    public void PageDown()
    {
        ScrollVertical(ScrollUnit == ScrollUnit.Pixel ? ViewportSize.Height : GetPageDownScrollAmount());
    }
    public void PageLeft()
    {
        ScrollHorizontal(ScrollUnit == ScrollUnit.Pixel ? -ViewportSize.Width : GetPageLeftScrollAmount());
    }
    public void PageRight()
    {
        ScrollHorizontal(ScrollUnit == ScrollUnit.Pixel ? ViewportSize.Width : GetPageRightScrollAmount());
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

    protected void InvalidateScrollInfo()
    {
        ScrollInfoInvalidated?.Invoke(this, EventArgs.Empty);
    }

    protected void InvalidateMeasure()
    {
        MeasureInvalidated?.Invoke(this, EventArgs.Empty);
    }

    private void ScrollVertical(double amount)
    {
        SetVerticalOffset(ScrollOffset.Y + amount);
    }

    private void ScrollHorizontal(double amount)
    {
        SetHorizontalOffset(ScrollOffset.X + amount);
    }
}

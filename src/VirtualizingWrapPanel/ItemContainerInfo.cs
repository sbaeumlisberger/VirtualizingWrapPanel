using System.Windows;

namespace WpfToolkit.Controls;

internal interface IItemContainerInfo
{
    UIElement UIElement { get; }

    Size DesiredSize { get; }

    bool IsMeasureValid { get; }

    Size MaxSize { get; }

    object Item { get; }

    Size Measure(Size availableSize);

    void Arrange(Rect rect);
}

internal class ItemContainerInfo : IItemContainerInfo
{
    public UIElement UIElement { get; }

    public Size DesiredSize => UIElement.DesiredSize;

    public bool IsMeasureValid => UIElement.IsMeasureValid;

    public Size MaxSize { get; } = new Size(double.PositiveInfinity, double.PositiveInfinity);

    public object Item { get; }

    private ItemContainerInfo(UIElement uiElement, object item)
    {
        UIElement = uiElement;
        Item = item;

        if (uiElement is FrameworkElement fe)
        {
            MaxSize = new Size(fe.MaxWidth, fe.MaxHeight);
        }
        Item = item;
    }

    public static IItemContainerInfo For(UIElement uiElement, object item)
    {
        return new ItemContainerInfo(uiElement, item);
    }

    public Size Measure(Size availableSize)
    {
        UIElement.Measure(availableSize);
        return UIElement.DesiredSize;
    }

    public void Arrange(Rect rect)
    {
        UIElement.Arrange(rect);
    }

    public override bool Equals(object? obj)
    {
        return obj is ItemContainerInfo other && ReferenceEquals(UIElement, other.UIElement);
    }

    public override int GetHashCode()
    {
        return UIElement.GetHashCode();
    }

    public static bool operator ==(ItemContainerInfo obj1, ItemContainerInfo obj2)
    {
        return ReferenceEquals(obj1?.UIElement, obj2?.UIElement);
    }

    public static bool operator !=(ItemContainerInfo obj1, ItemContainerInfo obj2)
    {
        return !ReferenceEquals(obj1?.UIElement, obj2?.UIElement);
    }
}

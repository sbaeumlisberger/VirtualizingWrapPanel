using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WpfToolkit.Controls;

namespace VirtualizingWrapPanelTest.Mocks;

internal class ItemContainerInfoMock : IItemContainerInfo
{
    public UIElement UIElement => throw new NotImplementedException();

    public Size DesiredSize { get; private set; } = Size.Empty;

    public Size MaxSize { get; }

    public TestItem Item { get; }
    object IItemContainerInfo.Item => Item;

    public Rect ArrangeRect { get; private set; }

    public bool IsMeasureValid { get; private set; }

    public ItemContainerInfoMock(TestItem item)
    {
        Item = item;
    }

    public void Arrange(Rect rect)
    {
        ArrangeRect = rect;
    }

    public Size Measure(Size availableSize)
    {
        if (DesiredSize == Size.Empty && Item is LazyTestItem lazyTestItem)
        {
            DesiredSize = new Size(lazyTestItem.FirstTimeWidth, lazyTestItem.FirstTimeHeight);
        }
        else if(!IsMeasureValid)
        {
            DesiredSize = new Size(Item.Width, Item.Height);
            IsMeasureValid = true;
        }
        return DesiredSize;
    }

    public override bool Equals(object? obj)
    {
        return obj is ItemContainerInfoMock other && Item.Equals(other.Item);
    }

    public override int GetHashCode()
    {
        return Item.GetHashCode();
    }
}

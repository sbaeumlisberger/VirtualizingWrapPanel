using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WpfToolkit.Controls
{
    internal class ItemContainerInfo : IItemContainerInfo
    {
        public UIElement UIElement { get; }

        public Size DesiredSize => UIElement.DesiredSize;

        public Size MaxSize { get; } = new Size(double.PositiveInfinity, double.PositiveInfinity);

        private ItemContainerInfo(UIElement uiElement)
        {
            UIElement = uiElement;

            if (uiElement is FrameworkElement fe)
            {
                MaxSize = new Size(fe.MaxWidth, fe.MaxHeight);
            }
        }

        public static IItemContainerInfo For(UIElement uiElement)
        {
            return new ItemContainerInfo(uiElement);
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
}

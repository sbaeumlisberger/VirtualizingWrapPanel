using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WpfToolkit.Controls
{
    internal class Arrangeable : IArrangeable
    {
        private UIElement uiElement;

        public Size DesiredSize => uiElement.DesiredSize;

        private Arrangeable(UIElement uiElement)
        {
            this.uiElement = uiElement;
        }

        public static IArrangeable For(UIElement child)
        {
            return new Arrangeable(child);
        }

        public void Arrange(Rect rect)
        {
            uiElement.Arrange(rect);
        }
    }
}

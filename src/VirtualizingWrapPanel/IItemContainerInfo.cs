using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WpfToolkit.Controls
{
    internal interface IItemContainerInfo
    {
        Size DesiredSize { get; }

        Size MaxSize { get; }

        void Arrange(Rect rect);
    }
}

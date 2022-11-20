using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WpfToolkit.Controls
{
    internal interface IArrangeable
    {
        Size DesiredSize { get; }

        void Arrange(Rect rect);
    }
}

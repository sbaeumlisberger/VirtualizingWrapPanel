using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WpfToolkit.Controls;

namespace VirtualizingWrapPanelTest;

internal class ArrangableMock : IArrangeable
{
    public Size DesiredSize { get; }

    public Size MaxSize { get; }

    public TestItem Item { get; }

    public Rect ArrangeRect { get; private set; }
     
    public ArrangableMock(TestItem item)
    {
        DesiredSize = new Size(item.Width, item.Height);
        Item = item;
    }

    public void Arrange(Rect rect)
    {
        ArrangeRect = rect;
    }
}

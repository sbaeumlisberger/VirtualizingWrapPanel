using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualizingWrapPanelTest;

internal class TestItem
{

    public double Width { get; }

    public double Height { get; }

    public long ID { get; set; }

    public TestItem(double width, double height)
    {
        Width = width;
        Height = height;
    }
}



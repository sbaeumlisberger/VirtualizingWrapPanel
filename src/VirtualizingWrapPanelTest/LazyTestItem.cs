using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualizingWrapPanelTest;

internal class LazyTestItem : TestItem
{
    public double FirstTimeWidth { get; }

    public double FirstTimeHeight { get; }

    public LazyTestItem()
    {
        var random = new Random();
        FirstTimeWidth = Math.Round(Width * 0.5);
        FirstTimeHeight = Math.Round(Height * 0.5);
    }
}



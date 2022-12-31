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

    public string ID { get; set; } = Guid.NewGuid().ToString();

    public TestItem(double width, double height)
    {
        Width = width;
        Height = height;
    }

    public TestItem()
    {
        var random = new Random();
        Width = random.Next(100, 201);
        Height = random.Next(100, 201);
    }

    public override bool Equals(object? obj)
    {
        return obj is TestItem other && Equals(ID, other.ID);
    }

    public override int GetHashCode()
    {
        return ID.GetHashCode();
    }
}



using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using System.Xml;
using WpfToolkit.Controls;
using Xunit;
using Assert = Xunit.Assert;

namespace VirtualizingWrapPanelTest.Tests;

public class BasisTest
{
    private VirtualizingWrapPanel vwp = TestUtil.CreateVirtualizingWrapPanel(500, 400);

    [UIFact]
    public void Inital()
    {
        Assert.Equal(500, vwp.DesiredSize.Width);
        Assert.Equal(400, vwp.DesiredSize.Height);
       
        Assert.Equal(500, vwp.ExtentWidth);
        Assert.Equal(20_000, vwp.ExtentHeight);

        Assert.Equal(40, vwp.Children.Count);

        // in viewport
        TestUtil.AssertItem(vwp, "Item 1", 0, 0);
        TestUtil.AssertItem(vwp, "Item 2", 100, 0);
        TestUtil.AssertItem(vwp, "Item 5", 400, 0);
        TestUtil.AssertItem(vwp, "Item 6", 0, 100);
        TestUtil.AssertItem(vwp, "Item 11", 0, 200);
        TestUtil.AssertItem(vwp, "Item 16", 0, 300);

        // after viewport
        TestUtil.AssertItem(vwp, "Item 21", 0, 400);
        TestUtil.AssertItem(vwp, "Item 26", 0, 500);
    }

    [UIFact]
    public void OffsetOfOneRow()
    {
        vwp.SetVerticalOffset(100);
        vwp.UpdateLayout();

        Assert.Equal(500, vwp.DesiredSize.Width);
        Assert.Equal(400, vwp.DesiredSize.Height);

        Assert.Equal(45, vwp.Children.Count);

        // before viewport
        TestUtil.AssertItem(vwp, "Item 1", 0, -100);
        TestUtil.AssertItem(vwp, "Item 2", 100, -100);
        TestUtil.AssertItem(vwp, "Item 5", 400, -100);

        // in viewport
        TestUtil.AssertItem(vwp, "Item 6", 0, 0);
        TestUtil.AssertItem(vwp, "Item 7", 100, 0);
        TestUtil.AssertItem(vwp, "Item 10", 400, 0);
        TestUtil.AssertItem(vwp, "Item 11", 0, 100);
        TestUtil.AssertItem(vwp, "Item 16", 0, 200);
        TestUtil.AssertItem(vwp, "Item 21", 0, 300);

        // after viewport
        TestUtil.AssertItem(vwp, "Item 26", 0, 400);
        TestUtil.AssertItem(vwp, "Item 31", 0, 500);
    }

    [UIFact]
    public void OffsetOfMultiplePages()
    {
        vwp.SetVerticalOffset(800);
        vwp.UpdateLayout();

        Assert.Equal(500, vwp.DesiredSize.Width);
        Assert.Equal(400, vwp.DesiredSize.Height);

        Assert.Equal(60, vwp.Children.Count);

        // before viewport
        TestUtil.AssertItem(vwp, "Item 31", 0, -200);
        TestUtil.AssertItem(vwp, "Item 36", 0, -100);
        TestUtil.AssertItem(vwp, "Item 37", 100, -100);
        TestUtil.AssertItem(vwp, "Item 40", 400, -100);

        // in viewport
        TestUtil.AssertItem(vwp, "Item 41", 0, 0);
        TestUtil.AssertItem(vwp, "Item 42", 100, 0);
        TestUtil.AssertItem(vwp, "Item 45", 400, 0);
        TestUtil.AssertItem(vwp, "Item 46", 0, 100);
        TestUtil.AssertItem(vwp, "Item 51", 0, 200);
        TestUtil.AssertItem(vwp, "Item 56", 0, 300);

        // after viewport
        TestUtil.AssertItem(vwp, "Item 61", 0, 400);
        TestUtil.AssertItem(vwp, "Item 66", 0, 500);
    }

    [UIFact]
    public void RowPartiallyInViewport()
    {
        vwp.SetVerticalOffset(850);
        vwp.UpdateLayout();

        Assert.Equal(500, vwp.DesiredSize.Width);
        Assert.Equal(400, vwp.DesiredSize.Height);

        Assert.Equal(65, vwp.Children.Count);

        // before viewport
        TestUtil.AssertItem(vwp, "Item 21", 0, -450);
        TestUtil.AssertItem(vwp, "Item 26", 0, -350);
        TestUtil.AssertItem(vwp, "Item 31", 0, -250);
        TestUtil.AssertItem(vwp, "Item 36", 0, -150);
        TestUtil.AssertItem(vwp, "Item 37", 100, -150);
        TestUtil.AssertItem(vwp, "Item 40", 400, -150);

        // in viewport
        TestUtil.AssertItem(vwp, "Item 41", 0, -50);
        TestUtil.AssertItem(vwp, "Item 42", 100, -50);
        TestUtil.AssertItem(vwp, "Item 45", 400, -50);
        TestUtil.AssertItem(vwp, "Item 46", 0, 50);
        TestUtil.AssertItem(vwp, "Item 51", 0, 150);
        TestUtil.AssertItem(vwp, "Item 56", 0, 250);
        TestUtil.AssertItem(vwp, "Item 61", 0, 350);

        // after viewport
        TestUtil.AssertItem(vwp, "Item 66", 0, 450);
        TestUtil.AssertItem(vwp, "Item 71", 0, 550);
        TestUtil.AssertItem(vwp, "Item 76", 0, 650);
        TestUtil.AssertItem(vwp, "Item 81", 0, 750);
    }

}

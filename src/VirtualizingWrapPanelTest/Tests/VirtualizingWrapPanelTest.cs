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

public class VirtualizingWrapPanelTest
{
    private VirtualizingWrapPanel vwp = TestUtil.CreateVirtualizingWrapPanel(600 + SystemParameters.VerticalScrollBarWidth, 400);

    [UIFact]
    public void NoCache_0x0()
    {
        VirtualizingPanel.SetCacheLength(vwp.ItemsControl, new VirtualizationCacheLength(0));

        vwp.UpdateLayout();

        Assert.Equal(600, vwp.DesiredSize.Width);
        Assert.Equal(400, vwp.DesiredSize.Height);

        Assert.Equal(24, vwp.Children.Count);

        TestUtil.AssertItemPosition(vwp, "Item 1", 0, 0);
        TestUtil.AssertItemPosition(vwp, "Item 2", 100, 0);
        TestUtil.AssertItemPosition(vwp, "Item 7", 0, 100);
        TestUtil.AssertItemPosition(vwp, "Item 12", 500, 100);
        TestUtil.AssertItemPosition(vwp, "Item 19", 0, 300);
    }

    [UIFact]
    public void NoCache_0x250()
    {
        VirtualizingPanel.SetCacheLength(vwp.ItemsControl, new VirtualizationCacheLength(0));
        
        vwp.SetVerticalOffset(250);
        vwp.UpdateLayout();

        Assert.Equal(600, vwp.DesiredSize.Width);
        Assert.Equal(400, vwp.DesiredSize.Height);

        Assert.Equal(30, vwp.Children.Count);

        TestUtil.AssertItemPosition(vwp, "Item 13", 0, -50);
        TestUtil.AssertItemPosition(vwp, "Item 14", 100, -50);
        TestUtil.AssertItemPosition(vwp, "Item 19", 0, 50);
        TestUtil.AssertItemPosition(vwp, "Item 25", 0, 150);
        TestUtil.AssertItemPosition(vwp, "Item 31", 0, 250);
    }

    [UIFact]
    public void DefaultCache_0x0()
    {
        Assert.Equal(600, vwp.DesiredSize.Width);
        Assert.Equal(400, vwp.DesiredSize.Height);

        Assert.Equal(48, vwp.Children.Count);

        TestUtil.AssertItemPosition(vwp, "Item 1", 0, 0);
        TestUtil.AssertItemPosition(vwp, "Item 2", 100, 0);
        TestUtil.AssertItemPosition(vwp, "Item 7", 0, 100);
        TestUtil.AssertItemPosition(vwp, "Item 12", 500, 100);
        TestUtil.AssertItemPosition(vwp, "Item 19", 0, 300);
        TestUtil.AssertItemPosition(vwp, "Item 25", 0, 400);
        TestUtil.AssertItemPosition(vwp, "Item 31", 0, 500);
    }
  
}

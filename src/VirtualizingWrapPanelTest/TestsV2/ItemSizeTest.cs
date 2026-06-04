using System.Windows;
using Xunit;

namespace VirtualizingWrapPanelTest.TestsV2;

public class ItemSizeTest
{
    private readonly TestController tc = TestController.CreateListBoxWithVirtualizingWrapPanel();

    [WpfFact]
    public async Task ExtentIsUpdatedWhenItemSizeChanged()
    {
        await tc.SetItemSizeAsync(new Size(100, 200));
        Assert.Equal(400_000, tc.ExtentHeight);

        await tc.SetItemSizeAsync(new Size(200, 200));
        Assert.Equal(1_000_000, tc.ExtentHeight);

        await tc.SetItemSizeAsync(new Size(100, 100));
        Assert.Equal(200_000, tc.ExtentHeight);
    }

    [WpfFact]
    public async Task ChildrensAreMeasuredWhenItemSizeChanged() // Issue #50
    {
        await tc.SetItemTemplateAsync("""
            <DataTemplate xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"> 
                <Grid Width="100" Height="100">
                    <TextBlock Text="{Binding Name}"/>
                </Grid>
            </DataTemplate>
            """);
        await tc.SetItemSizeAsync(new Size(50, 50));

        foreach (var child in tc.Children)
        {
            Assert.Equal(50, child.DesiredSize.Width);
            Assert.Equal(50, child.DesiredSize.Height);
            Assert.Equal(50, child.ActualWidth);
            Assert.Equal(50, child.ActualHeight);
        }
    }
}

using System.Windows.Controls;
using System.Windows.Input;
using Xunit;

namespace VirtualizingWrapPanelTest.TestsV2;

public class KeyboardNavigationTest
{
    private readonly TestController testController = TestController.CreateListBoxWithVirtualizingWrapPanel();

    [WpfTheory]
    [InlineData(VirtualizationMode.Standard, ScrollUnit.Pixel)]
    [InlineData(VirtualizationMode.Standard, ScrollUnit.Item)]
    [InlineData(VirtualizationMode.Recycling, ScrollUnit.Pixel)]
    [InlineData(VirtualizationMode.Recycling, ScrollUnit.Item)]
    public async Task DownKey_FocsuedItemBeforeViewport(VirtualizationMode virtualizationMode, ScrollUnit scrollUnit)
    {
        await testController.SetVirtualizationModeAsync(virtualizationMode);
        await testController.SetScrollUnitAsync(scrollUnit);

        await testController.FocusItemAsync(testController.Items[0]);
        await testController.SetVerticalOffsetAsync(2000);

        await testController.SendKeyEventAsync(Key.Down);

        Assert.Equal(testController.Items[testController.ItemsPerRow], testController.FocusedItem);
    }

    [WpfTheory]
    [InlineData(VirtualizationMode.Standard, ScrollUnit.Pixel)]
    [InlineData(VirtualizationMode.Standard, ScrollUnit.Item)]
    [InlineData(VirtualizationMode.Recycling, ScrollUnit.Pixel)]
    [InlineData(VirtualizationMode.Recycling, ScrollUnit.Item)]
    public async Task UpKey_FocsuedItemBeforeViewport(VirtualizationMode virtualizationMode, ScrollUnit scrollUnit)
    {
        await testController.SetVirtualizationModeAsync(virtualizationMode);
        await testController.SetScrollUnitAsync(scrollUnit);

        await testController.FocusItemAsync(testController.Items[testController.ItemsPerRow]);
        await testController.SetVerticalOffsetAsync(2000);

        await testController.SendKeyEventAsync(Key.Up);

        Assert.Equal(testController.Items[0], testController.FocusedItem);
    }

    [WpfTheory]
    [InlineData(VirtualizationMode.Standard, ScrollUnit.Pixel)]
    [InlineData(VirtualizationMode.Standard, ScrollUnit.Item)]
    [InlineData(VirtualizationMode.Recycling, ScrollUnit.Pixel)]
    [InlineData(VirtualizationMode.Recycling, ScrollUnit.Item)]
    public async Task LeftKey_FocsuedItemBeforeViewport(VirtualizationMode virtualizationMode, ScrollUnit scrollUnit)
    {
        await testController.SetVirtualizationModeAsync(virtualizationMode);
        await testController.SetScrollUnitAsync(scrollUnit);

        await testController.FocusItemAsync(testController.Items[3]);
        await testController.SetVerticalOffsetAsync(2000);

        await testController.SendKeyEventAsync(Key.Left);

        Assert.Equal(testController.Items[2], testController.FocusedItem);
    }

    [WpfTheory]
    [InlineData(VirtualizationMode.Standard, ScrollUnit.Pixel)]
    [InlineData(VirtualizationMode.Standard, ScrollUnit.Item)]
    [InlineData(VirtualizationMode.Recycling, ScrollUnit.Pixel)]
    [InlineData(VirtualizationMode.Recycling, ScrollUnit.Item)]
    public async Task RightKey_FocsuedItemBeforeViewport(VirtualizationMode virtualizationMode, ScrollUnit scrollUnit)
    {
        await testController.SetVirtualizationModeAsync(virtualizationMode);
        await testController.SetScrollUnitAsync(scrollUnit);

        await testController.FocusItemAsync(testController.Items[3]);
        await testController.SetVerticalOffsetAsync(2000);

        await testController.SendKeyEventAsync(Key.Right);

        Assert.Equal(testController.Items[4], testController.FocusedItem);
    }

    [WpfTheory]
    [InlineData(VirtualizationMode.Standard, ScrollUnit.Pixel)]
    [InlineData(VirtualizationMode.Standard, ScrollUnit.Item)]
    [InlineData(VirtualizationMode.Recycling, ScrollUnit.Pixel)]
    [InlineData(VirtualizationMode.Recycling, ScrollUnit.Item)]
    public async Task KeyDown_FocsuedItemAfterViewport(VirtualizationMode virtualizationMode, ScrollUnit scrollUnit)
    {
        await testController.SetVirtualizationModeAsync(virtualizationMode);
        await testController.SetScrollUnitAsync(scrollUnit);

        await testController.SetVerticalOffsetAsync(2000);
        await testController.FocusItemAsync(testController.Items[100]);
        await testController.SetVerticalOffsetAsync(0);

        await testController.SendKeyEventAsync(Key.Down);

        Assert.Equal(testController.Items[100 + testController.ItemsPerRow], testController.FocusedItem);
    }

    [WpfTheory]
    [InlineData(VirtualizationMode.Standard, ScrollUnit.Pixel)]
    [InlineData(VirtualizationMode.Standard, ScrollUnit.Item)]
    [InlineData(VirtualizationMode.Recycling, ScrollUnit.Pixel)]
    [InlineData(VirtualizationMode.Recycling, ScrollUnit.Item)]
    public async Task KeyUp_FocsuedItemAfterViewport(VirtualizationMode virtualizationMode, ScrollUnit scrollUnit)
    {
        await testController.SetVirtualizationModeAsync(virtualizationMode);
        await testController.SetScrollUnitAsync(scrollUnit);

        await testController.SetVerticalOffsetAsync(2000);
        await testController.FocusItemAsync(testController.Items[100]);
        await testController.SetVerticalOffsetAsync(0);

        await testController.SendKeyEventAsync(Key.Up);

        Assert.Equal(testController.Items[100 - testController.ItemsPerRow], testController.FocusedItem);
    }

    [WpfTheory]
    [InlineData(VirtualizationMode.Standard, ScrollUnit.Pixel)]
    [InlineData(VirtualizationMode.Standard, ScrollUnit.Item)]
    [InlineData(VirtualizationMode.Recycling, ScrollUnit.Pixel)]
    [InlineData(VirtualizationMode.Recycling, ScrollUnit.Item)]
    public async Task KeyLeft_FocsuedItemAfterViewport(VirtualizationMode virtualizationMode, ScrollUnit scrollUnit)
    {
        await testController.SetVirtualizationModeAsync(virtualizationMode);
        await testController.SetScrollUnitAsync(scrollUnit);

        await testController.SetVerticalOffsetAsync(2000);
        await testController.FocusItemAsync(testController.Items[103]);
        await testController.SetVerticalOffsetAsync(0);

        await testController.SendKeyEventAsync(Key.Left);

        Assert.Equal(testController.Items[102], testController.FocusedItem);
    }

    [WpfTheory]
    [InlineData(VirtualizationMode.Standard, ScrollUnit.Pixel)]
    [InlineData(VirtualizationMode.Standard, ScrollUnit.Item)]
    [InlineData(VirtualizationMode.Recycling, ScrollUnit.Pixel)]
    [InlineData(VirtualizationMode.Recycling, ScrollUnit.Item)]
    public async Task KeyRight_FocsuedItemAfterViewport(VirtualizationMode virtualizationMode, ScrollUnit scrollUnit)
    {
        await testController.SetVirtualizationModeAsync(virtualizationMode); 
        await testController.SetScrollUnitAsync(scrollUnit);

        await testController.SetVerticalOffsetAsync(2000);
        await testController.FocusItemAsync(testController.Items[103]);
        await testController.SetVerticalOffsetAsync(0);

        await testController.SendKeyEventAsync(Key.Right);

        Assert.Equal(testController.Items[104], testController.FocusedItem);
    }

    [WpfTheory]
    [InlineData(VirtualizationMode.Standard, ScrollUnit.Pixel)]
    //[InlineData(VirtualizationMode.Standard, ScrollUnit.Item)] TODO: Requires offset to be in items instead of pixels?
    [InlineData(VirtualizationMode.Recycling, ScrollUnit.Pixel)]
    //[InlineData(VirtualizationMode.Recycling, ScrollUnit.Item)] TODO: Requires offset to be in items instead of pixels?
    public async Task EndKey(VirtualizationMode virtualizationMode, ScrollUnit scrollUnit)
    {
        await testController.SetVirtualizationModeAsync(virtualizationMode); 
        await testController.SetScrollUnitAsync(scrollUnit);

        await testController.SendKeyEventAsync(Key.End);

        Assert.Equal(testController.ExtentHeight - testController.ViewportHeight, testController.VerticalOffset);
        Assert.Equal(testController.Items.Last(), testController.FocusedItem);
    }

    [WpfTheory]
    [InlineData(VirtualizationMode.Standard, ScrollUnit.Pixel)]
    [InlineData(VirtualizationMode.Standard, ScrollUnit.Item)]
    [InlineData(VirtualizationMode.Recycling, ScrollUnit.Pixel)]
    [InlineData(VirtualizationMode.Recycling, ScrollUnit.Item)]
    public async Task HomeKey(VirtualizationMode virtualizationMode, ScrollUnit scrollUnit)
    {
        await testController.SetVirtualizationModeAsync(virtualizationMode); 
        await testController.SetScrollUnitAsync(scrollUnit);

        await testController.SetVerticalOffsetAsync(1000);

        await testController.SendKeyEventAsync(Key.Home);

        Assert.Equal(0, testController.VerticalOffset);
        Assert.Equal(testController.Items.First(), testController.FocusedItem);
    }

    [WpfTheory]
    [InlineData(VirtualizationMode.Standard, ScrollUnit.Pixel)]
    //[InlineData(VirtualizationMode.Standard, ScrollUnit.Item)] TODO: Requires offset to be in items instead of pixels
    [InlineData(VirtualizationMode.Recycling, ScrollUnit.Pixel)]
    //[InlineData(VirtualizationMode.Recycling, ScrollUnit.Item)] TODO: Requires offset to be in items instead of pixels
    public async Task PageDownKey(VirtualizationMode virtualizationMode, ScrollUnit scrollUnit)
    {
        await testController.SetVirtualizationModeAsync(virtualizationMode); 
        await testController.SetScrollUnitAsync(scrollUnit);

        await testController.FocusItemAsync(testController.GetItemsInViewport().Last());

        await testController.SendKeyEventAsync(Key.PageDown);

        Assert.Equal(testController.ViewportHeight, testController.VerticalOffset);
        Assert.Equal(testController.Items[35], testController.FocusedItem);
    }

    [WpfTheory]
    [InlineData(VirtualizationMode.Standard, ScrollUnit.Pixel)]
    //[InlineData(VirtualizationMode.Standard, ScrollUnit.Item)] TODO: Requires offset to be in items instead of pixels
    [InlineData(VirtualizationMode.Recycling, ScrollUnit.Pixel)]
    //[InlineData(VirtualizationMode.Recycling, ScrollUnit.Item)] TODO: Requires offset to be in items instead of pixels
    public async Task PageUpKey(VirtualizationMode virtualizationMode, ScrollUnit scrollUnit)
    {
        await testController.SetVirtualizationModeAsync(virtualizationMode); 
        await testController.SetScrollUnitAsync(scrollUnit);

        await testController.SetVerticalOffsetAsync(1000);
        await testController.FocusItemAsync(testController.GetItemsInViewport().First());

        await testController.SendKeyEventAsync(Key.PageUp);

        Assert.Equal(1000 - testController.ViewportHeight, testController.VerticalOffset);
        Assert.Equal(testController.Items[30], testController.FocusedItem);
    }

    // TODO: GridView.WrappingKeyboardNavigation
}

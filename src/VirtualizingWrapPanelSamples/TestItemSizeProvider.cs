using System.Windows;
using WpfToolkit.Controls;

namespace VirtualizingWrapPanelSamples
{
    internal class TestItemSizeProvider : IItemSizeProvider
    {
        public Size GetSizeForItem(object item)
        {
            var testItem = (TestItem)item;
            return testItem.ItemSize;
        }
    }
}

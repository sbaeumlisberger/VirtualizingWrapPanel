using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WpfToolkit.Controls;

namespace VirtualizingWrapPanelSamples
{
    internal class TestItemSizeProvider : IItemSizeProvider
    {
        public Size GetSizeForItem(object item)
        {
            var testItem = (TestItem)item;
            return new Size(testItem.Size.Width + 10, testItem.Size.Height + 4);
        }
    }
}

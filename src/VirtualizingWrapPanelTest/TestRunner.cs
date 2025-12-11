using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Threading;

namespace VirtualizingWrapPanelTest;

internal class TestRunner
{
    public Window Window { get; } = new Window();

    public async Task UpdateLayoutAsync(UIElement element)
    {
        element.UpdateLayout();
        await Window.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.Render);
    }
}

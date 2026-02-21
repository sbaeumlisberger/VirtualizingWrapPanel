using System;
using System.Windows.Input;

namespace VirtualizingWrapPanelSamples;

class SimpleCommand(Action<object?> execute) : ICommand
{
    public event EventHandler? CanExecuteChanged;
    public bool CanExecute(object? parameter) => true;
    public void Execute(object? parameter) => execute(parameter);
}

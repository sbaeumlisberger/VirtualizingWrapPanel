using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Controls;

namespace VirtualizingWrapPanelTest;

internal class TestViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public Orientation Orientation { get; set => SetProperty(ref field, value); } = Orientation.Horizontal;

    public Orientation OrientationParent { get; set => SetProperty(ref field, value); } = Orientation.Vertical;

    private void SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (!Equals(value, field))
        {
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

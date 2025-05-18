namespace WpfToolkit.Controls;

/// <summary>
/// Specifies how items are aligned on the cross axis of a <see cref="VirtualizingWrapPanel"/>.
/// </summary>
public enum ItemAlignment
{
    /// <summary>
    /// The items are placed at the start of the cross axis.
    /// </summary>
    Start,
    /// <summary>
    /// The items are centered om the cross-axis.
    /// </summary>
    Center,
    /// <summary>
    /// The items are placed at the end of the cross axis.
    /// </summary>
    End,
    /// <summary>
    /// The items are stretched to fill the cross axis.
    /// </summary>
    Stretch
}

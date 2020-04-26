namespace WpfToolkit.Controls
{
	public enum SpacingMode
	{
		/// <summary>
		/// Spacing is disabled and all items will be arranged as closely as possible.
		/// </summary>
		None,
		/// <summary>
		/// The remaining space is evenly distributed between the items on a layout row, as well as the start and end of each row.
		/// </summary>
		Uniform,
		/// <summary>
		/// The remaining space is evenly distributed between the items on a layout row, excluding the start and end of each row.
		/// </summary>
		Spread
	}
}

using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace WpfToolkit.Controls
{
    /// <summary>
    /// Simple control that displays a gird of items. Depending on the orientation, the items are either stacked horizontally or vertically 
    /// until the items are wrapped to the next row or column. The control is using virtualization to support large amount of items.
    /// <p class="note">In order to work properly all items must have the same size.</p>
    /// </summary>
    public class GridView : ListView
    {
        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(nameof(Orientation), typeof(Orientation), typeof(GridView), new FrameworkPropertyMetadata(Orientation.Vertical));

        public static readonly DependencyProperty SpacingModeProperty = DependencyProperty.Register(nameof(SpacingMode), typeof(SpacingMode), typeof(GridView), new FrameworkPropertyMetadata(SpacingMode.Uniform, FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        /// Gets or sets a value that specifies the orientation in which items are arranged. The default value is <see cref="Orientation.Horizontal"/>.
        /// </summary>
        public Orientation Orientation { get => (Orientation)GetValue(OrientationProperty); set => SetValue(OrientationProperty, value); }

        /// <summary>
        /// Gets or sets the spacing mode used when arranging the items. The default value is <see cref="SpacingMode.Uniform"/>.
        /// </summary>
        public SpacingMode SpacingMode { get => (SpacingMode)GetValue(SpacingModeProperty); set => SetValue(SpacingModeProperty, value); }

        public GridView()
        {
            var factory = new FrameworkElementFactory(typeof(VirtualizingWrapPanel));
            factory.SetBinding(VirtualizingWrapPanel.OrientationProperty, new Binding
            {
                Source = this,
                Path = new PropertyPath(nameof(Orientation)),
                Mode = BindingMode.OneWay
            });
            factory.SetBinding(VirtualizingWrapPanel.SpacingModeProperty, new Binding
            {
                Source = this,
                Path = new PropertyPath(nameof(SpacingMode)),
                Mode = BindingMode.OneWay
            });
            ItemsPanel = new ItemsPanelTemplate(factory);

            VirtualizingPanel.SetCacheLengthUnit(this, VirtualizationCacheLengthUnit.Page);
            VirtualizingPanel.SetCacheLength(this, new VirtualizationCacheLength(1));

            VirtualizingPanel.SetIsVirtualizingWhenGrouping(this, true);

            ItemContainerStyle = new Style
            {
                Setters = {
                    new Setter {
                        Property = MarginProperty,
                        Value = new Thickness(4)
                    },
                    new Setter {
                        Property = PaddingProperty,
                        Value = new Thickness(4)
                    }
                }
            };
        }
    }
}

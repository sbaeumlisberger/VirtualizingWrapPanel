using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace WpfToolkit.Controls
{
    /// <summary>
    /// A ItemsControl supporting virtualization.
    /// </summary>
    public class VirtualizingItemsControl : ItemsControl
    {
        public VirtualizingItemsControl()
        {
            ItemsPanel = new ItemsPanelTemplate(new FrameworkElementFactory(typeof(VirtualizingStackPanel)));

            string template = @"
            <ControlTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'>
                <Border
                    BorderThickness='{TemplateBinding Border.BorderThickness}'
                    Padding='{TemplateBinding Control.Padding}'
                    BorderBrush='{TemplateBinding Border.BorderBrush}'
                    Background='{TemplateBinding Panel.Background}'
                    SnapsToDevicePixels='True'>
                    <ScrollViewer
                        Padding='{TemplateBinding Control.Padding}'
                        Focusable='False'>
                        <ItemsPresenter
                            SnapsToDevicePixels='{TemplateBinding UIElement.SnapsToDevicePixels}'/>
                    </ScrollViewer>
                </Border>
            </ControlTemplate>";
            Template = (ControlTemplate)XamlReader.Parse(template);

            ScrollViewer.SetCanContentScroll(this, true);

            ScrollViewer.SetVerticalScrollBarVisibility(this, ScrollBarVisibility.Auto);
            ScrollViewer.SetHorizontalScrollBarVisibility(this, ScrollBarVisibility.Auto);

            VirtualizingPanel.SetCacheLengthUnit(this, VirtualizationCacheLengthUnit.Page);
            VirtualizingPanel.SetCacheLength(this, new VirtualizationCacheLength(1));

            VirtualizingPanel.SetIsVirtualizingWhenGrouping(this, true);
        }
    }
}

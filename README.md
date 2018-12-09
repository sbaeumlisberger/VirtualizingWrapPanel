## VirtualizingWrapPanel for WPF

### Dependencies

.NET Framework 4.5+

### Whats's included?
* VirtualizingWrapPanel (implementation of a wrap panel that supports virtualization)
* GridView (control that display a grid of items by using the VirtualizingWrapPanel)

### Features
* horizontal & vertical orientation
* caching per item, page or pixels
* container recycling
* scrolling by item or pixels

### Installation
Get the [Nuget Package](https://www.nuget.org/packages/VirtualizingWrapPanel/).

### Resources
* [Sample Application](https://gitlab.com/sbaeumlisberger/virtualizing-wrap-panel/tree/master/VirtualizingWrapPanelSamples).
* [API-Documentation](http://sbaeumlisberger.gitlab.io/virtualizing-wrap-panel/api/WpfToolkit.Controls.html).

### Quick Start
```xml
<ListView xmlns:controls="clr-namespace:WpfToolkit.Controls;assembly=VirtualizingWrapPanel">
    <ListView.ItemsPanel>
         <ItemsPanelTemplate>
              <controls:VirtualizingWrapPanel/>
        </ItemsPanelTemplate>
    </ListView.ItemsPanel>
</ListView>
```
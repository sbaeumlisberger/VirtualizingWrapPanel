## VirtualizingWrapPanel for WPF

### Dependencies

.NET Framework 4.5+

### Whats's included?
* VirtualizingWrapPanel
* GridView (a simple control using the VirtualizingWrapPanel)

### Features
* horizontal & vertical orientation
* caching per item, page or pixels
* recycling containers mode
* scrolling by unit or pixels

### Installation
Get the [Nuget Package](https://www.nuget.org/packages/VirtualizingWrapPanel/).

### Samples & Documentation 
* A sample application can be found [here](https://gitlab.com/s.baeumlisberger/virtualizing-wrap-panel/tree/master/VirtualizingWrapPanelSamples).
* The API-Documentation can be found [here](http://s.baeumlisberger.gitlab.io/virtualizing-wrap-panel/api/WpfToolkit.Controls.html).

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
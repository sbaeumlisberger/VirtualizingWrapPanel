## Getting started

### Installation

First, add the package to your project. For example via the .NET CLI:  
`dotnet add package VirtualizingWrapPanel`  

More installation options can be found [here](https://www.nuget.org/packages/VirtualizingWrapPanel/).

Then add the follwing namespace to the xaml files, where you want to use package:  
`xmlns:vwp="clr-namespace:WpfToolkit.Controls;assembly=VirtualizingWrapPanel"`

### Basic usage

You can use the VirtualizingWrapPanel with an existing ItemsControl like a ListView as follows:

```
<Window ... xmlns:vwp="clr-namespace:WpfToolkit.Controls;assembly=VirtualizingWrapPanel">
    <ListView
        ItemsSource="{Binding YourItemsSource, Mode=OneWay}"
        ItemTemplate="{StaticResource YourItemTemplate}">
        <ListView.ItemsPanel>
            <ItemsPanelTemplate>
                <vwp:VirtualizingWrapPanel/>
            </ItemsPanelTemplate>
        </ListView.ItemsPanel>
    </ListView>
</Window>
```

Alternatively you can use the included GridView control, which is using a VirtualizingWrapPanel by default:

```
<Window ... xmlns:vwp="clr-namespace:WpfToolkit.Controls;assembly=VirtualizingWrapPanel">
    <GridView
        ItemsSource="{Binding YourItemsSource, Mode=OneWay}"
        ItemTemplate="{StaticResource YourItemTemplate}">
</Window>
```

### Spacing Behaviour

You can control the behaviour how the remaining space of a layout row is used. By default the remaining space is evenly distributed between the items, as well as the start and end of each row.

```
<vwp:VirtualizingWrapPanel SpacingMode="Uniform" StretchItems="false"/>
```

| SpacingMode      | Description |
| ---------------- | ----------- |
| None             | Spacing is disabled and all items will be arranged as closely as possible. |
| Uniform          | The remaining space is evenly distributed between the items on a layout row, as well as the start and end of each row. |
| BetweenItemsOnly | The remaining space is evenly distributed between the items on a layout row, excluding the start and end of each row. |
| StartAndEndOnly  | The remaining space is evenly distributed between start and end of each row. |

When the `StretchItems` property is set to true the items get stretched up to their max size to fill up the remaining space.

### Caching

The attached properties `VirtualizingPanel.CacheLength` and `VirtualizingPanel.CacheLengthUnit` can be used to control the caching behaviour.
For more information visit the [.NET API documentation](https://learn.microsoft.com/dotnet/api/system.windows.controls.virtualizingpanel.cachelength).

```
<ListView
    VirtualizingPanel.CacheLength="200"
    VirtualizingPanel.CacheLengthUnit="Pixel">
    <ListView.ItemsPanel>
         <ItemsPanelTemplate>
            <vwp:VirtualizingWrapPanel/>
        </ItemsPanelTemplate>
    </ListView.ItemsPanel>
</ListView>
```

### Vertical Orientation

```
<vwp:VirtualizingWrapPanel Orientation="Vertical"/>
```

### Grouping
TODO

### Different Sized Items
TODO

### Item Expansion
TODO

### VirtualizingItemsControl
TODO

## Getting started

### Installation

First, add the package to your project. For example via the .NET CLI:  
`dotnet add package VirtualizingWrapPanel`  

More installation options can be found [here](https://www.nuget.org/packages/VirtualizingWrapPanel/).

Then add the following namespace to the xaml files, where you want to use package:  
`xmlns:vwp="clr-namespace:WpfToolkit.Controls;assembly=VirtualizingWrapPanel"`

### Basic usage

You can use the VirtualizingWrapPanel with an existing ItemsControl like a ListView as follows:

```
<Window xmlns:vwp="clr-namespace:WpfToolkit.Controls;assembly=VirtualizingWrapPanel">
    <ListView
        ItemsSource="{Binding YourItemsSource, Mode=OneWay}"
        ItemTemplate="{StaticResource YourItemTemplate}">       
        <ListView.ItemsPanel>
            <ItemsPanelTemplate>
                <vwp:VirtualizingWrapPanel/>
            </ItemsPanelTemplate>
        </ListView.ItemsPanel>
         <ListView.ItemContainerStyle>
            <Style x:Key="ItemContainerStyle" TargetType="{x:Type ListViewItem}">
                <Setter Property="HorizontalContentAlignment" Value="Stretch"/>
                <Setter Property="VerticalContentAlignment" Value="Stretch"/>
            </Style>
        </ListView.ItemContainerStyle>
    </ListView>
</Window>
```

Alternatively, you can use the included GridView control, which is using a VirtualizingWrapPanel by default:

```
<Window xmlns:vwp="clr-namespace:WpfToolkit.Controls;assembly=VirtualizingWrapPanel">
    <vwp:GridView
        ItemsSource="{Binding YourItemsSource, Mode=OneWay}"
        ItemTemplate="{StaticResource YourItemTemplate}">
</Window>
```

### Spacing Behaviour

The `SpacingMode` property controls how the remaining space of a layout row is used. The default value is `Uniform` which means that the remaining space is evenly distributed between the items, as well as the start and end of each row.

```
<vwp:VirtualizingWrapPanel SpacingMode="Uniform" StretchItems="false"/>
```
```
<vwp:GridView SpacingMode="Uniform" StretchItems="false"/>
```

| SpacingMode      | Description |
| ---------------- | ----------- |
| None             | The items are placed next to each other without spacing.  |
| Uniform          | The remaining space is evenly distributed between the items on a layout row, as well as the start and end of each row. |
| BetweenItemsOnly | The remaining space is evenly distributed between the items on a layout row, but not the start and end of each row. |
| StartAndEndOnly  | The remaining space is evenly distributed between start and end of each row. |

When the `StretchItems` property is set to `true` the items get stretched up to their maximum size to fill as much remaining space as possible. The still remaining space is distributed according to the `SpacingMode` property.

### Caching

The attached properties `VirtualizingPanel.CacheLength` and `VirtualizingPanel.CacheLengthUnit` can be used to control the caching behaviour.
For more information visit the official [.NET API documentation](https://learn.microsoft.com/dotnet/api/system.windows.controls.virtualizingpanel.cachelength).

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
```
<GridView
    VirtualizingPanel.CacheLength="200"
    VirtualizingPanel.CacheLengthUnit="Pixel"/>
```
### Orientation

⚠️ This information is only valid for version 2.x

The `Orientation` property controls how the items are arranged. The default value is `Horizontal`, which means that the elements are arranged horizontally and wrap to the next row when the edge of the panel is reached. The scroll direction is vertical. When the property is set to `Vertical`, the items are arranged vertically and warp to a new column when the bottom of the panel is reached. The scroll direction is horizontal.
```
<vwp:VirtualizingWrapPanel Orientation="Vertical"/>
```
```
<vwp:GridView Orientation="Vertical"/>
```

### Grouping

Grouping is fully supported and can be used as shown in the following example:
```
<YouControl.Resources>
    <CollectionViewSource
        x:Key="GroupingItemsSource"
        Source="{Binding YourItemsSource, Mode=OneWay}">
        <CollectionViewSource.GroupDescriptions>
            <PropertyGroupDescription PropertyName="PropertyUsedForGrouping"/>
        </CollectionViewSource.GroupDescriptions>
    </CollectionViewSource>
<YouControl.Resources/>  

<ListView 
    VirtualizingPanel.IsVirtualizingWhenGrouping="True"  
    ItemsSource='{Binding Source={StaticResource GroupingItemsSource}}'>
    <ItemsControl.ItemsPanel>
        <ItemsPanelTemplate>
            <vwp:VirtualizingWrapPanel/>
        </ItemsPanelTemplate>
    </ItemsControl.ItemsPanel>
    <ItemsControl.GroupStyle>
        <GroupStyle>
            <GroupStyle.Panel>
                <ItemsPanelTemplate>
                    <VirtualizingStackPanel Orientation="Vertical"/>
                </ItemsPanelTemplate>
            </GroupStyle.Panel>
            <GroupStyle.ContainerStyle>
                <!-- orginal WPF style but with zero Margin on the ItemsPresenter -->
                <Style TargetType="{x:Type GroupItem}">
                    <Setter Property="Control.Template">
                        <Setter.Value>
                            <ControlTemplate TargetType="{x:Type GroupItem}">
                                <StackPanel>
                                    <ContentPresenter
                                        Content="{TemplateBinding ContentControl.Content}"
                                        ContentTemplate="{TemplateBinding ContentControl.ContentTemplate}"
                                        ContentStringFormat="{TemplateBinding ContentControl.ContentStringFormat}"
                                        Name="PART_Header" />
                                    <ItemsPresenter Name="ItemsPresenter" Margin="0" />
                                </StackPanel>
                            </ControlTemplate>
                        </Setter.Value>
                    </Setter>
                </Style>
            </GroupStyle.ContainerStyle>
        </GroupStyle>
     </ItemsControl.GroupStyle>
</ListView>
```
If `VirtualizingStackPanel.Orientation` is `Vertical` and `VirtualizingWrapPanel.Orientation` is `Horizontal`, you can scroll vertically through the groups and the VirtualizingWrapPanel wraps the items at the right end of the viewport.

If `VirtualizingStackPanel.Orientation` is `Horizontal` and `VirtualizingWrapPanel.Orientation` is `Vertical`, you can scroll horizontally through the groups and the VirtualizingWrapPanel wraps the items at the bottom of the viewport.

⚠️ Any other combination of the Orientation properties is not supported.

### Different Sized Items

⚠️ This information is only valid for version 2.x

In order to use different sized items the property `AllowDifferentSizedItems` must be set to `true`. If this property is enabled, it is strongly recommended to also set the `ItemSizeProvider` property to an instance of the `IItemSizeProvider` interface. Otherwise, the position of the items is not guaranteed to be correct when the user is scrolling fast.

The purpose of the `IItemSizeProvider` is to provide the size of the items based on the data. For example, when displaying images or other data where the resulting size is known without creating the UI elements. When the user scrolls quickly, it is not possible to realize all the items and get the desired size of the UI element. If this would be done, the performance would be really bad. Therefore, if no `IItemSizeProvider` is provided, the size of the items is assumed based on the size of the already realized items. Since in this case the size is assumed, it is not possible to guarantee that the items will always be shown at the right position.

### Item Expansion / Details View

The `GridDetailsView` provides the feature to show an inline details view. The `ExpandedItemTemplate` property can be used to specify a template for this view.
```
<vwp:GridDetailsView
    ItemsSource="{Binding YourItemsSource, Mode=OneWay}"
    ItemTemplate="{StaticResource ItemTemplate}"
    ExpandedItemTemplate="{StaticResource ExpandedItemTemplate}">
```

### VirtualizingItemsControl

The `VirtualizingItemsControl` is an extension of the standard `ItemsControl` to support virtualization.
```
<vwp:VirtualizingItemsControl
    ItemsSource="{Binding YourItemsSource, Mode=OneWay}"
    ItemTemplate="{StaticResource ItemTemplate}">
```

### More Information / Help

To get more information checkout the [Issues page](https://github.com/sbaeumlisberger/VirtualizingWrapPanel/issues) and feel free to open a new issue.

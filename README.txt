VirtualizingWrapPanel for WPF

.NET Framework 4.5+

Features:
- horizontal & vertical orientation
- caching per item, page or pixels
- recycling containers mode
- scrolling by unit or pixels

Example of use:
<ListView xmlns:wpftk="clr-namespace:WpfToolkit.Controls;assembly=VirtualizingWrapPanel">
    <ListView.ItemsPanel>
         <ItemsPanelTemplate>
              <wpftk:VirtualizingWrapPanel/>
        </ItemsPanelTemplate>
    </ListView.ItemsPanel>
</ListView>
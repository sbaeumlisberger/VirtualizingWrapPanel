VirtualizingWrapPanel for WPF

.NET Framework 4.5+

Example:
<ListView xmlns:wpftk="clr-namespace:WpfToolkit.Controls;assembly=VirtualizingWrapPanel">
    <ListView.ItemsPanel>
         <ItemsPanelTemplate>
              <wpftk:VirtualizingWrapPanel/>
        </ItemsPanelTemplate>
    </ListView.ItemsPanel>
</ListView>

Remarks:
- all children in the panel must be the same size
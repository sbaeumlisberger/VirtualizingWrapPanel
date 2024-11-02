using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using WpfToolkit.Controls;

namespace VirtualizingWrapPanelSamples
{
    public partial class MainWindow : Window
    {
        internal readonly MainWindowModel model = new MainWindowModel();

        private ItemsControl? previousItemsControl;

        public MainWindow()
        {
            DataContext = model;

            model.CollectionView.GroupDescriptions.Add(new PropertyGroupDescription(nameof(TestItem.Group)));

            InitializeComponent();

            model.PropertyChanged += Model_PropertyChanged;
        }

        private void Model_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(model.UseLazyLoadingItems):
                    string itemTemplateKey = model.UseLazyLoadingItems ? "RandomSizedItemTemplateLazy" : "RandomSizedItemTemplate";
                    listViewAllowDifferentSizedItems.ItemTemplate = (DataTemplate)Resources[itemTemplateKey];
                    Reload(listViewAllowDifferentSizedItems);
                    break;
                case nameof(model.UseItemSizeProvider):
                    var vwp = GetChildOfType<VirtualizingWrapPanel>(listViewAllowDifferentSizedItems)!;
                    vwp.ItemSizeProvider = model.UseItemSizeProvider ? model.ItemSizeProvider : null;
                    Reload(listViewAllowDifferentSizedItems);
                    break;
            }
        }

        private void Reload(ItemsControl itemsControl)
        {
            itemsControl.ItemsSource = null;
            itemsControl.UpdateLayout();

            foreach (var item in model.Items)
            {
                item.Reset();
            }

            itemsControl.ItemsSource = model.CollectionView;
        }

        private void InsertButton_Click(object sender, RoutedEventArgs args)
        {
            model.InsertItemAtRandomPosition();
        }

        private void FillButton_Click(object sender, RoutedEventArgs args)
        {
            model.AddItems();
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs args)
        {
            model.RemoveRandomItem();
        }

        private void ClearButton_Click(object sender, RoutedEventArgs args)
        {
            model.RemoveAllItems();
        }

        private void Random_Click(object sender, RoutedEventArgs args)
        {
            model.RandomizeItems();
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            var content = (DependencyObject)tabControl.SelectedContent;
            var itemsControl = content as ItemsControl ?? GetChildOfType<ItemsControl>(content)!;

            if (itemsControl != previousItemsControl)
            {
                if (previousItemsControl != null)
                {
                    previousItemsControl.ItemsSource = null;
                }
                itemsControl.ItemsSource = model.CollectionView;
                previousItemsControl = itemsControl;

                if (model.IsGrouping)
                {
                    itemsControl.GroupStyle.Add((GroupStyle)Resources["GroupStyle"]);
                }
                else
                {
                    itemsControl.GroupStyle.Clear();
                }
            }
        }

        private void Item_Loaded(object sender, RoutedEventArgs args)
        {
            model.RenderedItemsCount++;
        }

        private void Item_Unloaded(object sender, RoutedEventArgs args)
        {
            model.RenderedItemsCount--;
        }

        private void RefreshMemoryUsageButton_Click(object sender, RoutedEventArgs args)
        {
            model.RefreshMemoryUsage();
        }

        private static T? GetChildOfType<T>(DependencyObject element) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
            {
                var child = VisualTreeHelper.GetChild(element, i);
                var result = (child as T) ?? GetChildOfType<T>(child);
                if (result != null) return result;
            }
            return null;
        }

        private void FilterTextBox_TextChanged(object sender, TextChangedEventArgs args)
        {
            model.CollectionView.Filter = new Predicate<object>((item) =>
            {
                if (int.TryParse(filterTextBox.Text, out int filterValue))
                {
                    return ((TestItem)item).Number > filterValue;
                }
                return true;
            });
        }

        private void RemoveMenuItem_Click(object sender, RoutedEventArgs e)
        {
            model.RemoveItem((TestItem)((FrameworkElement)sender).DataContext);
        }

        private void ScrollIntoViewTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            scrollIntoViewTextBoxPlaceholder.Visibility = string.IsNullOrEmpty(scrollIntoViewTextBox.Text) ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ScrollIntoViewTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ScrollIntoView();
            }
        }

        private void ScrollIntoViewTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            ScrollIntoView();
        }

        private void ScrollIntoView()
        {
            if (int.TryParse(scrollIntoViewTextBox.Text, out int itemNumber)
                && model.Items.Where(item => item.Number == itemNumber).FirstOrDefault() is { } item)
            {
                var itemsControl = FindItemsControl();

                if (itemsControl is ListView listView)
                {
                    listView?.ScrollIntoView(item);
                }
                else if (itemsControl is ListBox listBox)
                {
                    listBox?.ScrollIntoView(item);
                }
            }
            scrollIntoViewTextBox.Clear();
        }

        private ItemsControl FindItemsControl()
        {
            var content = (DependencyObject)tabControl.SelectedContent;
            return content as ItemsControl ?? GetChildOfType<ItemsControl>(content)!;
        }

        private void GroupingCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            model.IsGrouping = true;
            FindItemsControl().GroupStyle.Add((GroupStyle)Resources["GroupStyle"]);
        }

        private void GroupingCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            model.IsGrouping = false;
            FindItemsControl().GroupStyle.Clear();
        }
    }
}

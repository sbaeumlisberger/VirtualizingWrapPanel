using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

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
    }
}

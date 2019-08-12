using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace VirtualizingWrapPanelSamples
{

    public partial class MainWindow : Window
    {

        private readonly MainWindowModel model = new MainWindowModel();

        private ItemsControl previousItemsControl;

        private readonly ICollectionView view;

        public MainWindow()
        {
            DataContext = model;

            view = CollectionViewSource.GetDefaultView(model.Items);
            view.GroupDescriptions.Add(new PropertyGroupDescription(nameof(TestItem.Group)));

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

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            if (tabControl.SelectedContent != previousItemsControl)
            {
                if (previousItemsControl != null)
                {
                    previousItemsControl.ItemsSource = null;
                }

                var content = (DependencyObject)tabControl.SelectedContent;
                var itemsControl = content as ItemsControl ?? GetChildOfType<ItemsControl>(content);
                itemsControl.ItemsSource = model.Items;
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

        private static T GetChildOfType<T>(DependencyObject element) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
            {
                var child = VisualTreeHelper.GetChild(element, i);
                var result = (child as T) ?? GetChildOfType<T>(child);
                if (result != null) return result;
            }
            return null;
        }

    }

}
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WpfToolkit.Controls
{
    /// <summary>
    /// Simple control that displays a gird of items. Depending on the orientation, the items are either stacked horizontally or vertically 
    /// until the items are wrapped to the next row or column. The control is using virtualization to support large amount of items.
    /// If an item is clicked the item gots expanded until it is clicked again or an other item is clicked and gots expanded.
    /// <p class="note">In order to work properly all items must have the same size.</p>
    /// </summary>
    public partial class GridDetailsView : GridView
    {
        public static readonly DependencyProperty ExpandedItemTemplateProperty = DependencyProperty.Register(nameof(ExpandedItemTemplate), typeof(DataTemplate), typeof(GridDetailsView), new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty ExpandedItemProperty = DependencyProperty.Register(nameof(ExpandedItem), typeof(object), typeof(GridDetailsView), new FrameworkPropertyMetadata(null));

        /// <summary>Gets or sets the data template used for the item expansion.</summary>
        public DataTemplate? ExpandedItemTemplate { get => (DataTemplate?)GetValue(ExpandedItemTemplateProperty); set => SetValue(ExpandedItemTemplateProperty, value); }

        /// <summary>Gets the currently expanded item. If no item is expanded null is returned.</summary>
        public object? ExpandedItem { get => GetValue(ExpandedItemProperty); private set => SetValue(ExpandedItemProperty, value); }

        private FrameworkElement? expandedItemContainerRoot;

        private bool animateExpansion = false;
        private bool animateCloseExpansion = false;

        public GridDetailsView()
        {
            InitializeComponent();
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            var container = (FrameworkElement)base.GetContainerForItemOverride();
            container.PreviewMouseDown += Container_PreviewMouseDown;
            return container;
        }

        private async void Container_PreviewMouseDown(object sender, MouseButtonEventArgs args)
        {
            if (args.LeftButton == MouseButtonState.Pressed)
            {
                var item = ((FrameworkElement)sender).DataContext;
                if (item != ExpandedItem)
                {
                    ExpandedItem = item;
                }
                else
                {
                    animateExpansion = false;
                    animateCloseExpansion = true;

                    if (MaxContainerSize == double.PositiveInfinity)
                    {
                        MaxContainerSize = DesiredContainerSize;
                    }

                    double sourceHeight = MaxContainerSize;

                    for (int i = 20; i >= 0; i--)
                    {
                        if (!animateCloseExpansion)
                        {
                            return;
                        }
                        MaxContainerSize = (sourceHeight / 20) * i;
                        if (i != 0)
                        {
                            await Task.Delay(15);
                        }
                    }

                    expandedItemContainerRoot = null;
                    ExpandedItem = null;
                    animateCloseExpansion = false;
                }
            }
        }

        private async void ExpandedItemContainerRoot_Loaded(object sender, RoutedEventArgs args)
        {
            animateCloseExpansion = false;

            if (expandedItemContainerRoot == null)
            {
                expandedItemContainerRoot = (FrameworkElement)sender;
                MaxContainerSize = 0;

                double targetHeight = DesiredContainerSize;

                animateExpansion = true;
                for (int i = 0; i <= 20; i++)
                {
                    if (!animateExpansion)
                    {
                        return;
                    }
                    MaxContainerSize = (targetHeight / 20) * i;
                    if (i != 20)
                    {
                        await Task.Delay(15);
                    }
                }
                MaxContainerSize = double.PositiveInfinity;
                animateExpansion = false;
            }
            else
            {
                expandedItemContainerRoot = (FrameworkElement)sender;
                MaxContainerSize = double.PositiveInfinity;
            }
        }

        private double DesiredContainerSize
        {
            get
            {
                if (expandedItemContainerRoot is null)
                {
                    throw new NullReferenceException($"{nameof(expandedItemContainerRoot)} is null");
                }
                if (Orientation == Orientation.Horizontal)
                {
                    return expandedItemContainerRoot.DesiredSize.Height;
                }
                else
                {
                    return expandedItemContainerRoot.DesiredSize.Width;
                }
            }
        }

        private double MaxContainerSize
        {
            get
            {
                if (expandedItemContainerRoot is null)
                {
                    throw new NullReferenceException($"{nameof(expandedItemContainerRoot)} is null");
                }
                if (Orientation == Orientation.Horizontal)
                {
                    return expandedItemContainerRoot.MaxHeight;
                }
                else
                {
                    return expandedItemContainerRoot.MaxWidth;
                }
            }
            set
            {
                if (expandedItemContainerRoot is null)
                {
                    throw new NullReferenceException($"{nameof(expandedItemContainerRoot)} is null");
                }
                if (Orientation == Orientation.Horizontal)
                {
                    expandedItemContainerRoot.MaxHeight = value;
                }
                else
                {
                    expandedItemContainerRoot.MaxWidth = value;
                }
            }
        }
    }
}

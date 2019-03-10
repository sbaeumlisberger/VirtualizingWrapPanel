using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Controls;

namespace WpfToolkit.Controls {

    // TODO: BringIndexIntoView

    /// <summary>
    /// A implementation of a wrap panel that supports virtualization and can be used in horizontal and vertical orientation.
    /// In addition the panel allows to expand one specific item.
    /// <p class="note">In order to work properly all items must have the same size.</p>
    /// </summary>
    public class VirtualizingWrapPanelWithItemExpansion : VirtualizingWrapPanel {

        public static readonly DependencyProperty ExpandedItemTemplateProperty = DependencyProperty.Register(nameof(ExpandedItemTemplate), typeof(DataTemplate), typeof(VirtualizingWrapPanelWithItemExpansion), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure));

        public static readonly DependencyProperty ExpandedItemProperty = DependencyProperty.Register(nameof(ExpandedItem), typeof(object), typeof(VirtualizingWrapPanelWithItemExpansion), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure, (o, a) => ((VirtualizingWrapPanelWithItemExpansion)o).ExpandedItemPropertyChanged(a)));

        /// <summary>Gets or sets the data template used for the item expansion.</summary>
        public DataTemplate ExpandedItemTemplate { get => (DataTemplate)GetValue(ExpandedItemTemplateProperty); set => SetValue(ExpandedItemTemplateProperty, value); }

        /// <summary>Gets or set the expanded item. The default value is null.</summary>
        public object ExpandedItem { get => GetValue(ExpandedItemProperty); set => SetValue(ExpandedItemProperty, value); }

        private int ExpandedItemIndex => Items.IndexOf(ExpandedItem);

        private FrameworkElement expandedItemChild = null;

        private int _itemIndexFollwingExpansion;

        private void ExpandedItemPropertyChanged(DependencyPropertyChangedEventArgs args) {
            if (args.OldValue != null) {
                int index = InternalChildren.IndexOf(expandedItemChild);
                if (index != -1) {
                    expandedItemChild = null;
                    RemoveInternalChildRange(index, 1);
                }
            }
        }

        protected override Size CalculateExtent() {
            Size extent = base.CalculateExtent();

            if (expandedItemChild != null) {
                if (Orientation == Orientation.Vertical) {
                    extent.Height += expandedItemChild.DesiredSize.Height;
                }
                else {
                    extent.Width += expandedItemChild.DesiredSize.Width;
                }
            }

            return extent;
        }

        protected override Size ArrangeOverride(Size finalSize) {
            double expandedItemChildHeight = 0;

            double unusedWidth = GetWidth(finalSize) - (GetWidth(childSize) * itemsPerRowCount);
            double spacing = unusedWidth > 0 ? unusedWidth / (itemsPerRowCount + 1) : 0;

            for (int childIndex = 0; childIndex < InternalChildren.Count; childIndex++) {
                UIElement child = InternalChildren[childIndex];

                if (child == expandedItemChild) {
                    double x = IsSpacingEnabled ? spacing : 0;
                    double y = (ExpandedItemIndex / itemsPerRowCount) * GetHeight(childSize) + GetHeight(childSize);
                    double width = IsSpacingEnabled ? GetWidth(finalSize) - 2 * spacing : GetWidth(finalSize);
                    double height = GetHeight(expandedItemChild.DesiredSize);
                    if (Orientation == Orientation.Vertical) {
                        expandedItemChild.Arrange(CreateRect(x - GetX(Offset), y - GetY(Offset), width, height));
                    }
                    else {
                        expandedItemChild.Arrange(CreateRect(x - GetX(Offset), y - GetY(Offset), height, width));
                    }
                    expandedItemChildHeight = height;
                }
                else {
                    int itemIndex = GetItemIndexFromChildIndex(childIndex);

                    int columnIndex = itemIndex % itemsPerRowCount;
                    int rowIndex = itemIndex / itemsPerRowCount;

                    double x = columnIndex * GetWidth(childSize);

                    if (IsSpacingEnabled) {
                        x += (columnIndex + 1) * spacing;
                    }

                    double y = rowIndex * GetHeight(childSize) + expandedItemChildHeight;

                    child.Arrange(CreateRect(x - GetX(Offset), y - GetY(Offset), childSize.Width, childSize.Height));
                }
            }

            return finalSize;
        }

        protected override void RealizeItems() {
            var startPos = ItemContainerGenerator.GeneratorPositionFromIndex(ItemRange.StartIndex);

            int childIndex = startPos.Offset == 0 ? startPos.Index : startPos.Index + 1;

            int expandedItemIndex = Items.IndexOf(ExpandedItem);
            int itemIndexFollwingExpansion = expandedItemIndex != -1 ? (((expandedItemIndex / itemsPerRowCount) + 1) * itemsPerRowCount) - 1 : -1;
            itemIndexFollwingExpansion = Math.Min(itemIndexFollwingExpansion, Items.Count - 1);

            if (itemIndexFollwingExpansion != _itemIndexFollwingExpansion && expandedItemChild != null) {
                RemoveInternalChildRange(InternalChildren.IndexOf(expandedItemChild), 1);
            }

            using (ItemContainerGenerator.StartAt(startPos, GeneratorDirection.Forward, true)) {
                for (int itemIndex = ItemRange.StartIndex; itemIndex <= ItemRange.EndIndex; itemIndex++, childIndex++) {

                    FrameworkElement child = (FrameworkElement)ItemContainerGenerator.GenerateNext(out bool isNewlyRealized);

                    if (isNewlyRealized || /*recycling*/!InternalChildren.Contains(child)) {

                        if (childIndex >= InternalChildren.Count) {
                            AddInternalChild(child);
                        }
                        else {
                            InsertInternalChild(childIndex, child);
                        }
                        ItemContainerGenerator.PrepareItemContainer(child);
                        if (ItemSize == Size.Empty) {
                            child.Measure(CreateSize(GetWidth(Viewport), double.MaxValue));
                        }
                        else {
                            child.Measure(ItemSize);
                        }
                    }

                    if (itemIndex == itemIndexFollwingExpansion) {
                        if (expandedItemChild == null) {
                            expandedItemChild = (FrameworkElement)ExpandedItemTemplate.LoadContent();
                            expandedItemChild.DataContext = Items[expandedItemIndex];                           
                            expandedItemChild.Measure(Viewport);
                        }
                        if (!InternalChildren.Contains(expandedItemChild)) {
                            childIndex++;
                            if (childIndex >= InternalChildren.Count) {
                                AddInternalChild(expandedItemChild);
                            }
                            else {
                                InsertInternalChild(childIndex, expandedItemChild);
                            }
                        }
                    }
                }

                _itemIndexFollwingExpansion = itemIndexFollwingExpansion;
            }
        }

        protected override void OnClearChildren() {
            base.OnClearChildren();
            expandedItemChild = null;
        }

        protected override GeneratorPosition GetGeneratorPositionFromChildIndex(int childIndex) {
            int expandedItemChildIndex = InternalChildren.IndexOf(expandedItemChild);
            if (expandedItemChildIndex != -1 && childIndex > expandedItemChildIndex) {
                return new GeneratorPosition(childIndex - 1, 0);
            }
            else {
                return new GeneratorPosition(childIndex, 0);
            }
        }

        protected override void VirtualizeItems() {
            for (int childIndex = InternalChildren.Count - 1; childIndex >= 0; childIndex--) {

                var child = (FrameworkElement)InternalChildren[childIndex];

                if (child == expandedItemChild) {
                    if (!ItemRange.Contains(ExpandedItemIndex)) {
                        expandedItemChild = null;
                        RemoveInternalChildRange(childIndex, 1);
                    }
                }
                else {
                    int itemIndex = Items.IndexOf(child.DataContext);

                    var position = ItemContainerGenerator.GeneratorPositionFromIndex(itemIndex);

                    if (!ItemRange.Contains(itemIndex)) {

                        if (IsRecycling) {
                            ItemContainerGenerator.Recycle(position, 1);
                        }
                        else {
                            ItemContainerGenerator.Remove(position, 1);
                        }

                        RemoveInternalChildRange(childIndex, 1);
                    }
                }
            }
        }
    }

}

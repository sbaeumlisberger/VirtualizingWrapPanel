using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace WpfToolkit.Controls
{
    internal interface IVirtualizingPanelWrapper
    {
        IEnumerable<UIElement> InternalChildren { get; }
        int InternalChildrenCount { get; }
        bool ContainsInternalChild(UIElement uiElement);
        void AddInternalChild(UIElement uiElement);
        void InsertInternalChild(int index, UIElement uiElement);
        void RemoveInternalChildAt(int index);
        IRecyclingItemContainerGenerator ItemContainerGenerator { get; }
    }

    internal class VirtualizingPanelWrapper : IVirtualizingPanelWrapper
    {
        public IEnumerable<UIElement> InternalChildren => internalChildrenSupplier.Invoke().Cast<UIElement>();

        public int InternalChildrenCount => internalChildrenSupplier.Invoke().Count;

        public IRecyclingItemContainerGenerator ItemContainerGenerator => itemContainerGeneratorSupplier.Invoke();

        private readonly Func<UIElementCollection> internalChildrenSupplier;
        private readonly Action<UIElement> addInternalChild;
        private readonly Action<int, UIElement> insertInternalChild;
        private readonly Action<int> removetInternalChildAt;
        private readonly Func<IRecyclingItemContainerGenerator> itemContainerGeneratorSupplier;

        public VirtualizingPanelWrapper(
            Func<UIElementCollection> internalChildrenSupplier, 
            Action<UIElement> addInternalChild, 
            Action<int, UIElement> insertInternalChild, 
            Action<int> removetInternalChildAt,
            Func<IRecyclingItemContainerGenerator> itemContainerGeneratorSupplier)
        {
            this.internalChildrenSupplier = internalChildrenSupplier;
            this.addInternalChild = addInternalChild;     
            this.insertInternalChild = insertInternalChild;
            this.removetInternalChildAt = removetInternalChildAt;
            this.itemContainerGeneratorSupplier = itemContainerGeneratorSupplier;
        }

        public bool ContainsInternalChild(UIElement uiElement) 
        {
            return internalChildrenSupplier.Invoke().Contains(uiElement);
        }

        public void AddInternalChild(UIElement uiElement)
        {
            addInternalChild.Invoke(uiElement);
        }

        public void InsertInternalChild(int index, UIElement uiElement)
        {
            insertInternalChild.Invoke(index, uiElement);
        }

        public void RemoveInternalChildAt(int index)
        {
            removetInternalChildAt.Invoke(index);
        }
    }
}

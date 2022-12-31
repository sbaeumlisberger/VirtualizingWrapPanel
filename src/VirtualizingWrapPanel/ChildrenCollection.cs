using System;
using System.Windows;

namespace WpfToolkit.Controls
{
    internal interface IChildrenCollection
    {
        void AddChild(IItemContainerInfo child);
        void RemoveChild(IItemContainerInfo child);
    }

    internal class VirtualizingPanelWrapper : IChildrenCollection
    {

        private readonly Action<UIElement> addInternalChild;
        private readonly Action<UIElement> removetInternalChild;

        public VirtualizingPanelWrapper(
            Action<UIElement> addInternalChild,
            Action<UIElement> removetInternalChild)
        {
            this.addInternalChild = addInternalChild;
            this.removetInternalChild = removetInternalChild;
        }

        public void AddChild(IItemContainerInfo containerInfo)
        {
            addInternalChild.Invoke(containerInfo.UIElement);
        }

        public void RemoveChild(IItemContainerInfo containerInfo)
        {
            removetInternalChild.Invoke(containerInfo.UIElement);
        }
    }
}



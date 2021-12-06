using System;

namespace WpfToolkit.Controls
{
    internal interface IVirtualizingContinueWith
    {
        public Action AddContinueWith { get; }
        public Action RemoveContinueWith { get; }
    }
}

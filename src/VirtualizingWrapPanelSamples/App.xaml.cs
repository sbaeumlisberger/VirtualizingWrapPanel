using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace VirtualizingWrapPanelSamples
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            DispatcherUnhandledException += App_DispatcherUnhandledException;
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            if (e.Exception is InvalidOperationException)
            {
                e.Handled = true;
                var mainWindowModel = ((MainWindow)MainWindow).model;
                mainWindowModel.VirtualizationMode =
                    mainWindowModel.VirtualizationMode == VirtualizationMode.Standard
                    ? VirtualizationMode.Recycling
                    : VirtualizationMode.Standard;
                MessageBox.Show(e.Exception.Message);

            }
        }
    }
}

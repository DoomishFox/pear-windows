// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using H.NotifyIcon;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;

using LaunchActivatedEventArgs = Microsoft.UI.Xaml.LaunchActivatedEventArgs;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Pear
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public sealed partial class App : Application
    {
        #region Properties

        public TaskbarIcon? TrayIcon { get; private set; }
        public Window? Window { get; set; }
        public bool HandleClosedEvents { get; set; } = true;

        #endregion

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            InitializeTrayIcon();

            Peard.Start("APRIL"); // [TODO]: get computer name and put it in here.
                                  //         the other option is to use a saved identity
        }

        private void InitializeTrayIcon()
        {
            var showHideWindowCommand = (XamlUICommand)Resources["ShowHideWindowCommand"];
            showHideWindowCommand.ExecuteRequested += ShowHideWindowCommand_ExecuteRequested;

            var exitApplicationCommand = (XamlUICommand)Resources["ExitApplicationCommand"];
            exitApplicationCommand.ExecuteRequested += ExitApplicationCommand_ExecuteRequested;

            TrayIcon = (TaskbarIcon)Resources["TrayIcon"];
            TrayIcon.ForceCreate();
        }

        private void ShowHideWindowCommand_ExecuteRequested(object? _, ExecuteRequestedEventArgs args)
        {
            if (Window == null)
            {
                Window = new MainWindow();
                Window.Activate();
                Window.Closed += (sender, args) =>
                {
                    if (HandleClosedEvents)
                    {
                        args.Handled = true;
                        Window.Hide();
                    }
                };
                Window.Show();
                return;
            }

            if (Window.Visible)
            {
                Window.Hide();
            }
            else
            {
                Window.Show();
            }
        }

        private void ExitApplicationCommand_ExecuteRequested(object? _, ExecuteRequestedEventArgs args)
        {
            HandleClosedEvents = false;
            TrayIcon?.Dispose();
            Window?.Close();

            // https://github.com/HavenDV/H.NotifyIcon/issues/66
            if (Window == null)
            {
                Environment.Exit(0);
            }
        }
    }
}

using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using CefSharp;
using CefSharp.Wpf;
using Extensions;
using Logic;

namespace Worker.CefSharp.WPF
{
    public partial class MainWindow
    {
        public ChromiumWebBrowser Browser;

        public MainWindow()
        {
            Title = "Worker CefSharp WPF";

            InitializeComponent();

            var settings = new CefSettings();
            settings.IgnoreCertificateErrors = true;
            Cef.Initialize(settings, true, null);

            Browser = new ChromiumWebBrowser();

            var boundObj = new BoundObject();
            Browser.RegisterJsObject("bound", boundObj);

            DockPanel.Children.Add(Browser);

            UpdateBindings();

            Browser.ConsoleMessage += (sender, eventArgs) =>
            {
                Debug.WriteLine(eventArgs.Message);
            };

            Loaded += MainWindow_Loaded;
        }

        private void UpdateBindings()
        {
            var addressBinding = new Binding
            {
                Path = new PropertyPath(nameof(Browser.Address)),
                Source = Browser
            };

            BindingOperations.SetBinding(Address, TextBlock.TextProperty, addressBinding);

            var progressBinding = new Binding
            {
                Path = new PropertyPath(nameof(Browser.IsLoading)),
                Source = Browser
            };

            BindingOperations.SetBinding(ProgressBar, ProgressBar.IsIndeterminateProperty, progressBinding);
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await TestLogic.Run(Browser);
        }

        public void ShowDevTools(object sender, RoutedEventArgs e)
        {
            Browser.ShowDevTools();
        }
    }
}

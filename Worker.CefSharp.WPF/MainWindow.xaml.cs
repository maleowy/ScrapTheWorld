using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using CefSharp;
using CefSharp.Wpf;

namespace Worker.CefSharp.WPF
{
    public partial class MainWindow
    {
        public ChromiumWebBrowser Browser;

        public MainWindow()
        {
            Title = "Worker CefSharp WPF";

            InitializeComponent();

            Browser = new ChromiumWebBrowser();
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
            Browser.Load("https://duckduckgo.com");
            await Task.Delay(3000);
            await Browser.EvaluateScriptAsync("document.querySelector('#search_form_input_homepage').value = 'Test';");
            await Task.Delay(1000);
            await Browser.EvaluateScriptAsync("document.querySelector('#search_button_homepage').click();");
            await Task.Delay(1000);
            await Browser.EvaluateScriptAsync("console.log(document.title)");
        }

        public void ShowDevTools(object sender, RoutedEventArgs e)
        {
            Browser.ShowDevTools();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            Cef.Shutdown();
            Application.Current.Shutdown();
        }
    }
}

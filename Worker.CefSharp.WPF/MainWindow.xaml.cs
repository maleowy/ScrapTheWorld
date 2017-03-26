using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using CefSharp;
using CefSharp.Wpf;

namespace Worker.CefSharp.WPF
{
    public partial class MainWindow
    {
        private ChromiumWebBrowser Browser;

        public MainWindow()
        {
            InitializeComponent();

            Browser = new ChromiumWebBrowser();
            DockPanel.Children.Add(Browser);

            Loaded += MyWindow_Loaded;
        }

        private async void MyWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Browser.Load("https://duckduckgo.com");
            await Task.Delay(3000);
            await Browser.EvaluateScriptAsync("document.querySelector('#search_form_input_homepage').value = 'Test';");
            await Task.Delay(1000);
            await Browser.EvaluateScriptAsync("document.querySelector('#search_button_homepage').click();");
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

using System.Windows;
using CefSharp;

namespace Worker.CefSharp.WPF
{
    public partial class App
    {
        public App()
        {
            ShutdownMode = ShutdownMode.OnExplicitShutdown;

            var settings = new CefSettings();
            Cef.Initialize(settings, true, null);

            MainWindow window = new MainWindow();
            window.Show();
        }
    }
}

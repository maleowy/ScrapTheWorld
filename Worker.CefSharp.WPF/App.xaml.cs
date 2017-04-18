using System.Windows;
using CefSharp;

namespace Worker.CefSharp.WPF
{
    public partial class App
    {
        public App()
        {
            MainWindow window = new MainWindow();
            window.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Cef.Shutdown();
            base.OnExit(e);
        }
    }
}

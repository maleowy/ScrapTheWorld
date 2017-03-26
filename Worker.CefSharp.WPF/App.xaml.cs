using System.Windows;

namespace Worker.CefSharp.WPF
{
    public partial class App
    {
        public App()
        {
            ShutdownMode = ShutdownMode.OnExplicitShutdown;

            MainWindow window = new MainWindow();
            window.Show();
        }
    }
}

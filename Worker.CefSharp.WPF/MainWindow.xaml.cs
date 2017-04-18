using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using CefSharp;
using CefSharp.Wpf;
using EasyNetQ;
using Extensions;
using Models;
using static Logic.Logic;

namespace Worker.CefSharp.WPF
{
    public partial class MainWindow
    {
        public static IBus Bus;
        public ChromiumWebBrowser Browser;
        public static ISubscriptionResult SubscriptionResult;

        public MainWindow()
        {
            Title = "Worker CefSharp WPF";

            InitializeComponent();

            Bus = RabbitHutch.CreateBus(GetBusConfiguration());
            InitializeChromium();

            Loaded += MainWindow_Loaded;

            Closed += (sender, args) =>
            {
                SubscriptionResult.Dispose();
            };
        }

        private void InitializeChromium()
        {
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
            await Browser.WaitForInitializationAsync();

            SubscriptionResult = Bus.SubscribeAsync("subscriptionId", GetLogic(url => Task.FromResult(Browser.LoadPage(url)),
                script =>
                {
                    string result = null;
                    AutoResetEvent waitHandle = new AutoResetEvent(false);

                    Task.Run(async () =>
                    {
                        await Dispatcher.Invoke(async () =>
                        {
                            result = await Browser.EvaluateScriptWithReturnAsync(script);
                            waitHandle.Set();
                        });
                    });

                    waitHandle.WaitOne();

                    return Task.FromResult(result);
                },
                async node => await Bus.PublishAsync(node),
                async result => await Bus.PublishAsync(new Result { Data = result }),
                ex => Console.WriteLine(ex.Message)));
        }

        public void ShowDevTools(object sender, RoutedEventArgs e)
        {
            Browser.ShowDevTools();
        }
    }
}

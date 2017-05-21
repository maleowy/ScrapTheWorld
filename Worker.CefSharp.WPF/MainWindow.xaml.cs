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
using Serilog;
using Serilog.Core;
using static Logic.Logic;
using static Helpers.Helpers;

namespace Worker.CefSharp.WPF
{
    public partial class MainWindow
    {
        public static IBus Bus;
        public ChromiumWebBrowser Browser;
        public static ISubscriptionResult SubscriptionResult;
        public static Logger Logger;

        public MainWindow()
        {
            Title = "Worker CefSharp WPF";

            InitializeComponent();

            Bus = RabbitHutch.CreateBus(GetBusConfiguration(FindRabbit()));

            Logger = new LoggerConfiguration()
                .WriteTo.RollingFile("log.txt", retainedFileCountLimit: 7)
                .CreateLogger();

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

            Browser = new ChromiumWebBrowser
            {
                LifeSpanHandler = new MyLifeSpanHandler(),
                RequestHandler = new MyRequestHandler(),
                RenderProcessMessageHandler = new MyRenderProcessMessageHandler(),
                BrowserSettings = {ImageLoading = CefState.Disabled}
            };

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

            SubscriptionResult = Bus.SubscribeAsync("subscriptionId", GetLogic(node => Logger.Information("{@Node}", node),
                url => Task.FromResult(Browser.LoadPage(url)),
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
                async node => await Bus.PublishAsync(new Result { Node = node }),
                async node =>
                {
                    Logger.Error("{@Node}", node);
                    await Bus.PublishAsync(new ErrorResult { Node = node });
                },
                node => {}));
        }

        public void ShowDevTools(object sender, RoutedEventArgs e)
        {
            Browser.ShowDevTools();
        }
    }
}

﻿using System.Diagnostics;
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

        public MainWindow()
        {
            Title = "Worker CefSharp WPF";

            InitializeComponent();

            Bus = RabbitHutch.CreateBus(GetBusConfiguration());
            InitializeChromium();

            Loaded += MainWindow_Loaded;
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

            Bus.SubscribeAsync("subscriptionId", GetLogic(async url => await Browser.LoadPageAsync(url),
                async script =>
                {
                    string result = null;
                    await Dispatcher.Invoke(async () =>
                    {
                        result = await Browser.EvaluateScriptWithReturnAsync(script);
                    });

                    return await Task.FromResult(result);
                }));

            Publish();
        }

        private static void Publish()
        {
            Bus.Publish(new Node { Url = "http://www.wp.pl", Script = "document.title" });
            Bus.Publish(new Node { Url = "http://www.onet.pl", Script = "document.title" });
            Bus.Publish(new Node { Url = "http://www.interia.pl", Script = "document.title" });
        }

        public void ShowDevTools(object sender, RoutedEventArgs e)
        {
            Browser.ShowDevTools();
        }
    }
}

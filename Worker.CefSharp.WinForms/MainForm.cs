﻿// Copyright © 2010-2015 The CefSharp Authors. All rights reserved.
//
// Use of this source code is governed by a BSD-style license that can be found in the LICENSE file.

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Forms;
using CefSharp;
using CefSharp.WinForms;
using EasyNetQ;
using Extensions;
using Models;
using Serilog;
using Serilog.Core;
using static Logic.Logic;
using static Helpers.Helpers;

namespace Worker.CefSharp.WinForms
{
    public partial class MainForm : Form
    {
        public static IBus Bus;
        public ChromiumWebBrowser Browser;
        public static ISubscriptionResult SubscriptionResult;
        public static Logger Logger;

        public MainForm()
        {
            InitializeComponent();

            Text = "Worker CefSharp WinForms";

            Bus = RabbitHutch.CreateBus(GetBusConfiguration(FindRabbit()));

            Logger = new LoggerConfiguration()
                .WriteTo.RollingFile("log.txt", retainedFileCountLimit: 7)
                .CreateLogger();

            InitializeChromium();

            Load += MainForm_Loaded;

            FormClosed += (sender, args) =>
            {
                SubscriptionResult.Dispose();
                Cef.Shutdown();
                Environment.Exit(0);
            };
        }

        private async void MainForm_Loaded(object sender, EventArgs e)
        {
            await Browser.WaitForInitializationAsync();

            SubscriptionResult = Bus.SubscribeAsync("subscriptionId", GetLogic(node => Logger.Information("{@Node}", node),
                url => Task.FromResult(Browser.LoadPage(url)),
                script => Task.FromResult(Browser.EvaluateScriptWithReturn(script)),
                async node => await Bus.PublishAsync(node),
                async node => await Bus.PublishAsync(new Result { Node = node }),
                async node =>
                {
                    Logger.Error("{@Node}", node);
                    await Bus.PublishAsync(new ErrorResult { Node = node });
                },
                node => {}));
        }

        public void InitializeChromium()
        {
            var bitness = Environment.Is64BitProcess ? "x64" : "x86";
            var version = $"Chromium: {Cef.ChromiumVersion}, CEF: {Cef.CefVersion}, CefSharp: {Cef.CefSharpVersion}, Environment: {bitness}";
            DisplayOutput(version);

            Cef.EnableHighDPISupport();

            var settings = new CefSettings();
            settings.CefCommandLineArgs.Add("disable-gpu", "1");
            //settings.RemoteDebuggingPort = 9222;

            settings.IgnoreCertificateErrors = true;
            Cef.Initialize(settings, true, null);

            Browser = new ChromiumWebBrowser("about:blank")
            {
                Dock = DockStyle.Fill,
                LifeSpanHandler = new MyLifeSpanHandler(),
                RequestHandler = new MyRequestHandler(),
                RenderProcessMessageHandler = new MyRenderProcessMessageHandler(),
                BrowserSettings = { ImageLoading = CefState.Disabled }
            };

            var boundObj = new BoundObject();
            Browser.RegisterJsObject("bound", boundObj);

            toolStripContainer.ContentPanel.Controls.Add(Browser);

            Browser.LoadingStateChanged += OnLoadingStateChanged;
            Browser.ConsoleMessage += OnBrowserConsoleMessage;
            Browser.StatusMessage += OnBrowserStatusMessage;
            Browser.TitleChanged += OnBrowserTitleChanged;
            Browser.AddressChanged += OnBrowserAddressChanged;

            Browser.ConsoleMessage += (sender, eventArgs) =>
            {
                Debug.WriteLine(eventArgs.Message);
            };
        }

        private void OnBrowserConsoleMessage(object sender, ConsoleMessageEventArgs args)
        {
            DisplayOutput($"Line: {args.Line}, Source: {args.Source}, Message: {args.Message}");
        }

        private void OnBrowserStatusMessage(object sender, StatusMessageEventArgs args)
        {
            this.InvokeOnUiThreadIfRequired(() => statusLabel.Text = args.Value);
        }

        private void OnLoadingStateChanged(object sender, LoadingStateChangedEventArgs args)
        {
            SetCanGoBack(args.CanGoBack);
            SetCanGoForward(args.CanGoForward);

            this.InvokeOnUiThreadIfRequired(() => SetIsLoading(!args.CanReload));
        }

        private void OnBrowserTitleChanged(object sender, TitleChangedEventArgs args)
        {

        }

        private void OnBrowserAddressChanged(object sender, AddressChangedEventArgs args)
        {
            this.InvokeOnUiThreadIfRequired(() => urlTextBox.Text = args.Address);
        }

        private void SetCanGoBack(bool canGoBack)
        {
            this.InvokeOnUiThreadIfRequired(() => backButton.Enabled = canGoBack);
        }

        private void SetCanGoForward(bool canGoForward)
        {
            this.InvokeOnUiThreadIfRequired(() => forwardButton.Enabled = canGoForward);
        }

        private void SetIsLoading(bool isLoading)
        {
            goButton.Text = isLoading ? "Stop" : "Go";
            goButton.Image = isLoading ? Properties.Resources.nav_plain_red : Properties.Resources.nav_plain_green;

            HandleToolStripLayout();
        }

        public void DisplayOutput(string output)
        {
            this.InvokeOnUiThreadIfRequired(() => outputLabel.Text = output);
        }

        private void HandleToolStripLayout(object sender, LayoutEventArgs e)
        {
            HandleToolStripLayout();
        }

        private void HandleToolStripLayout()
        {
            var width = toolStrip1.Width;

            foreach (ToolStripItem item in toolStrip1.Items)
            {
                if (item != urlTextBox)
                {
                    width -= item.Width - item.Margin.Horizontal;
                }
            }

            urlTextBox.Width = Math.Max(0, width - urlTextBox.Margin.Horizontal - 18);
        }

        private void ExitMenuItemClick(object sender, EventArgs e)
        {
            Browser.Dispose();
            Cef.Shutdown();
            Close();
        }

        private void GoButtonClick(object sender, EventArgs e)
        {
            LoadUrl(urlTextBox.Text);
        }

        private void BackButtonClick(object sender, EventArgs e)
        {
            Browser.Back();
        }

        private void ForwardButtonClick(object sender, EventArgs e)
        {
            Browser.Forward();
        }

        private void UrlTextBoxKeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
            {
                return;
            }

            LoadUrl(urlTextBox.Text);
        }

        private void LoadUrl(string url)
        {
            if (Uri.IsWellFormedUriString(url, UriKind.RelativeOrAbsolute))
            {
                Browser.Load(url);
            }
        }

        private void showDevToolsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Browser.ShowDevTools();
        }
    }
}

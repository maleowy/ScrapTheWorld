using System;
using System.IO;
using System.Threading.Tasks;
using EasyNetQ;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Models;
using static Logic.Logic;
using OpenQA.Selenium.Support.UI;
using Serilog;
using Serilog.Core;

namespace Worker.Selenium
{
    class Program
    {
        public static IBus Bus;
        public static ISubscriptionResult SubscriptionResult;
        public static Logger Logger;

        static void Main(string[] args)
        {
            Console.Title = "Worker Selenium";

            Bus = RabbitHutch.CreateBus(GetBusConfiguration());

            Logger = new LoggerConfiguration()
                .WriteTo.ColoredConsole()
                .WriteTo.RollingFile("log.txt", retainedFileCountLimit: 7)
                .CreateLogger();

            var dir = AppDomain.CurrentDomain.BaseDirectory;
            dir = Path.GetFullPath(Path.Combine(dir, @"..\..\..\"));

            var chromePortable = Path.Combine(dir, @"GoogleChromePortable\GoogleChromePortable.exe");

            IWebDriver driver = new ChromeDriver(new ChromeOptions
            {
                BinaryLocation = chromePortable 
            });

            IJavaScriptExecutor jse = (IJavaScriptExecutor)driver;

            SubscriptionResult = Bus.SubscribeAsync("subscriptionId", GetLogic(node => Logger.Information("{@Node}", node),
                url => { driver.Navigate().GoToUrl(url);
                    driver.WaitForPageLoad();
                    return Task.FromResult(true); },
                script =>
                {
                    string result = (string)jse.ExecuteScript(script.Replace("(function() {", "").Replace("})();", ""));
                    return Task.FromResult(result);
                },
                async node => await Bus.PublishAsync(node),
                async node => await Bus.PublishAsync(new Result { Node = node }),
                async node =>
                {
                    Logger.Error("{@Node}", node);
                    await Bus.PublishAsync(new ErrorResult { Node = node });
                },
                node => { }));

            Console.ReadLine();

            SubscriptionResult.Dispose();

            driver.QuitAll();

            Environment.Exit(0);
        }

        private static void Test(IWebDriver driver, IJavaScriptExecutor jse)
        {
            driver.Navigate().GoToUrl("https://www.wp.pl");
            IWait<IWebDriver> wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));
            wait.Until(driver1 => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete"));
            var title = (string)jse.ExecuteScript("var self = {}; self.Results = [document.title]; return JSON.stringify([self]);");
        }
    }
}

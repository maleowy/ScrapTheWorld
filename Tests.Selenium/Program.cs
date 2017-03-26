using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace Tests.Selenium
{
    class Program
    {
        static void Main(string[] args)
        {
            IWebDriver driver = new ChromeDriver();

            driver.Navigate().GoToUrl("https://duckduckgo.com/");

            IWebElement input = driver.FindElement(By.Id("search_form_input_homepage"));
            input.SendKeys("Test");

            IWebElement button = driver.FindElement(By.Id("search_button_homepage"));
            button.Click();

            System.Console.WriteLine("Page title is: " + driver.Title);
            driver.Quit();

            Console.ReadLine();
        }
    }
}

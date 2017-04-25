using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace Worker.Selenium
{
    public static class Extensions
    {
        public static void WaitForPageLoad(this IWebDriver driver)
        {
            IWait<IWebDriver> wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));
            wait.Until(d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").Equals("complete"));
        }

        public static void QuitAll(this IWebDriver driver)
        {
            foreach (var handle in driver.WindowHandles)
            {
                driver.SwitchTo().Window(handle);
                driver.Close();
            }

            driver.Quit();
        }
    }
}

﻿using System.Threading.Tasks;
using CefSharp;
using Extensions;

namespace Logic
{
    public static class TestLogic
    {
        public static async Task Run1(IWebBrowser browser) // Task Delay based
        {
            await Task.Delay(3000);
            browser.Load("https://duckduckgo.com");
            await Task.Delay(3000);
            await browser.EvaluateScriptAsync("document.querySelector('#search_form_input_homepage').value = 'Test';");
            await Task.Delay(1000);
            await browser.EvaluateScriptAsync("document.querySelector('#search_button_homepage').click();");
            await Task.Delay(1000);
            await browser.EvaluateScriptAsync("console.log(document.title)");
        }

        public static async Task Run(IWebBrowser browser) // better one
        {
            await browser.WaitForInitializationAsync();
            await browser.LoadPageAsync("https://duckduckgo.com");
            await browser.EvaluateScriptAsync("document.querySelector('#search_form_input_homepage').value = 'Test';");
            await browser.EvaluateScriptAsync("document.querySelector('#search_button_homepage').click();");
            await browser.LoadPageAsync();
            string title = await browser.EvaluateScriptWithReturnAsync("document.title");
            await browser.EvaluateScriptAsync("console.log(document.title)");
            await browser.EvaluateScriptAsync("bound.reverseText(document.title)");
        }
    }
}

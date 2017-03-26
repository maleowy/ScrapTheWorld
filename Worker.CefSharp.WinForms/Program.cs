using System;
using System.Windows.Forms;

namespace Worker.CefSharp.WinForms
{
    public class Program
    {
        [STAThread]
        public static void Main()
        {
            var form = new MainForm();
            Application.Run(form);
        }
    }
}

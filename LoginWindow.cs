using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gecko;
using System.Windows.Forms;

namespace MatterHackers.MatterControl
{
    class LoginWindow
    {
        public static bool LoggedIn = false;
        private static Form f;
        [STAThread]
        public static void Create()
        {
            Xpcom.Initialize();
            var geckoWebBrowser = new GeckoWebBrowser { Dock = DockStyle.Fill };
            f = new Form();
            f.Controls.Add(geckoWebBrowser);
            f.TopMost = true;
            f.FormBorderStyle = FormBorderStyle.None;
            f.WindowState = FormWindowState.Maximized;
            geckoWebBrowser.Navigate("https://docs.google.com/a/oregonstate.edu/forms/d/1lEOh3MaMI5WUKKv5B2qupvRBOXLO_-F3H0mrVT6icdk/viewform");
            geckoWebBrowser.Navigated += c_Navigated;
            Application.Run(f);
        }

        static void c_Navigated(object sender, EventArgs e)
        {
            GeckoNavigatedEventArgs a = (GeckoNavigatedEventArgs) e;
            Console.WriteLine(a.Uri.ToString());

            if (a.Uri.ToString() == "https://docs.google.com/a/oregonstate.edu/forms/d/1lEOh3MaMI5WUKKv5B2qupvRBOXLO_-F3H0mrVT6icdk/formResponse")
            {
                LoggedIn = true;
                f.Close();
            }
        }
    }
}

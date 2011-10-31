using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using mshtml;
using Sidi.Util;

namespace Sidi.Sammy.Dkb
{
    public class DkbSession
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public DkbSession(string user, string password)
        {
            User = user;
            Password = password;
            ie = new SHDocVw.InternetExplorer();
            ie.Visible = true;
        }

        string User;
        string Password;
        SHDocVw.InternetExplorer ie;

        public void Navigate(string url)
        {
            ie.Navigate(url);
            Wait();
        }
        
        public void Login()
        {
            for (int loginAttempt = 0; loginAttempt < 3; ++loginAttempt)
            {
                try
                {
                    ie.Navigate("https://banking.dkb.de/dkb/-");
                    Wait();
                    var d = Document;
                    var userInput = (IHTMLInputTextElement)d.getElementById("j_username");
                    userInput.value = User;
                    var passwordInput = (IHTMLInputTextElement)d.getElementById("j_password");
                    passwordInput.value = Password;
                    dynamic loginButton = d.getElementById("buttonlogin");
                    loginButton.Click();
                    Wait();
                    return;
                }
                catch (Exception e)
                {
                    log.Warn("login failed", e);
                }
            }
        }

        public void Logout()
        {
            dynamic logoutElement = Document.getElementById("logout");
            logoutElement.Click();
            Wait();
            ie.Quit();
            ie = null;
        }

        IHTMLDocument3 Document
        {
            get
            {
                return (IHTMLDocument3) ie.Document;
            }
        }

        public IHTMLElement GetSearchResultTable()
        {
            return GetTableOfClass("searchResultTable");
        }

        IHTMLElement GetTableOfClass(string className)
        {
            var tables =
            Document.getElementsByTagName("TABLE")
                .Cast<IHTMLElement>()
                .ToList();
            return tables
                .First(e => className.Equals(e.GetAttribute("class")));
        }

        public IHTMLElement GetWhiteTable()
        {
            return Document.getElementsByTagName("td")
                .Cast<IHTMLElement>()
                .First(td => td.GetAttribute("class").Equals("whitetable", StringComparison.InvariantCultureIgnoreCase))
                .GetChildren().First();
        }

        void Wait()
        {
            while (ie.Busy)
            {
                Thread.Sleep(100);
            }
        }
    }
}

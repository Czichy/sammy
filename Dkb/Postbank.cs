using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SHDocVw;
using System.Threading;
using mshtml;
using Sidi.CommandLine;

namespace Sidi.Sammy.Dkb
{
    [Usage("Bank statements from https://direkt.postbank.de/direktportalApp/index.jsp")]
    public class Postbank
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        InternetExplorer ie;

        public Postbank(string user, string password)
        {
            this.user = user;
            this.password = password;
        }

        string user;
        string password;

        void Login()
        {
            ie = new InternetExplorer();
            ie.Visible = true;
            Navigate("https://direkt.postbank.de/direktportalApp/index.jsp");
            var d = Document;
            var userInput = (IHTMLInputTextElement)d.getElementById("j_username");
            userInput.value = user;
            var passwordInput = (IHTMLInputTextElement)d.getElementById("j_password");
            passwordInput.value = password;
            Navigate("Javascript:Login()");
        }

        public IList<Sidi.Sammy.Payment> GetPayments()
        {
            try
            {
                var payments = new List<Sidi.Sammy.Payment>();
                return payments;
            }
            finally
            {
                Logout();
            }
        }

        void Logout()
        {
        }

        IHTMLDocument3 Document
        {
            get
            {
                return (IHTMLDocument3)ie.Document;
            }
        }

        void Navigate(string url)
        {
            ie.Navigate(url);
            Wait();
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

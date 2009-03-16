using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Sidi.CommandLine;
using NUnit.Framework;
using System.IO;
using System.Collections;
using System.Text.RegularExpressions;

namespace Sidi.Sammy.Html_1822direkt
{
    [Usage("https://banking.1822direkt.com/JOBa1822Web/showLogin.do")]
    public class Html1822direkt : ICollector
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        static string bankCode = "50050201";

        string user;
        string pass;

        public Html1822direkt(string user, string pass)
        {
            this.user = user;
            this.pass = pass;
        }
        
        public class Scrape : ScrapeForm
        {
            private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

            public string user;
            public string pass;
            string ownAcctNo;
            
            public Scrape()
            {
                BodyHandler = this.Init;
            }

            public void Init(HtmlElement b)
            {
                actions.Enqueue(delegate()
                {
                    Command("showLogin.do");
                    BodyHandler = Login;
                });

                actions.Enqueue(delegate()
                {
                    BodyHandler = NextAction;
                    Command("accountTurnoversSearch.do?start=&accountings=SH&fixedPeriods=4");
                });
                
                actions.Enqueue(delegate()
                {
                    BodyHandler = getAccountsOverviewList;
                    Command("getAccountsOverviewList.do");
                });

                NextAction();
            }

            public void Login(HtmlElement b)
            {
                b.SetInput("login", user);
                b.SetInput("pin", pass);
                BodyHandler = NextAction;
                HtmlElement submitButton = b.Find(x => x.AttributeContains("value", "Anmelden"));
                submitButton.ClickElement();
            }

            public void NextAction(HtmlElement b)
            {
                NextAction();
            }

            Queue<System.Action> actions = new Queue<System.Action>();

            void getAccountsOverviewList(HtmlElement b)
            {
                // https://banking.1822direkt.com/JOBa1822Web/getAccountTurnovers.do?accNdx=0
                foreach (Uri i in b.GetLinks())
                {
                    if (i.ToString().Contains("getAccountTurnovers.do?accNdx"))
                    {
                        Uri accountUri = new Uri(i.ToString());
                        actions.Enqueue(delegate
                        {
                            BodyHandler = AccountTurnovers;
                            Browser.Url = accountUri;
                        });
                    }
                }

                NextAction();
            }

            void NextAction()
            {
                if (actions.Count == 0)
                {
                    BodyHandler = null;
                    Command("logoff.do");
                }
                else
                {
                    actions.Dequeue()();
                }
            }

            public void AccountTurnovers(HtmlElement b)
            {
                BodyHandler = DetailsOn;
                HtmlElement detailsOn = b.Find(x => x.Name == "allDetailsOn");
                if (detailsOn != null)
                {
                    detailsOn.ClickElement();
                }
                else
                {
                    DetailsOn(b);
                }
            }

            public void DetailsOn(HtmlElement b)
            {
                BodyHandler = AccountTurnoversList;
                Command("getAccountTurnoversList.do#");
            }

            public List<Payment> Payments = new List<Payment>();

            void Command(string command)
            {
                Browser.Url = new Uri("https://banking.1822direkt.com/JOBa1822Web/" + command);
            }

            public void AccountTurnoversList(HtmlElement b)
            {
                var table = b.Find(x => x.TagName == "TABLE" && x.AttributeIs("className", "listTable"));
                var tbody = table.Children[0];

                ownAcctNo = Regex.Match(Browser.Document.Title, @"\d+").Value;

                System.Collections.IEnumerator i = tbody.Children.GetEnumerator();
                HtmlElement c = i.MoveNext() ? (HtmlElement)i.Current : null;
                while (c != null)
                {
                    Payment p = new Payment();

                    p.EntryDate = DateTime.Parse(c.Children[1].InnerText);
                    p.ValueDate = DateTime.Parse(c.Children[2].InnerText);
                    p.Value = ParseValue(c.Children[5].InnerText);
                    p.EntryText = c.Children[3].InnerText;

                    var details = GetDetails(i, ref c);

                    p.Currency = "EUR";
                    details.TryGetValue("Name", out p.Name);
                    details.TryGetValue("Konto", out p.AcctNo);
                    details.TryGetValue("BLZ", out p.BankCode);
                    details.TryGetValue("VWZ", out p.Purpose);
                    // p.OwnName;
                    p.OwnAcctNo = ownAcctNo;
                    p.OwnBankCode = bankCode;

                    Payments.Add(p);
                }

                NextAction();
            }

            Dictionary<string, string> GetDetails(IEnumerator i, ref HtmlElement c)
            {
                Dictionary<string, string> details = new Dictionary<string, string>();
                string key = null;
                string value = null;
                bool isFirst = true;

                while (c != null)
                {
                    if (!isFirst && c.HasAttribute("title")) { break; }
                    isFirst = false;
                    try
                    {
                        var subRow = c.Children[4].Children[0].Children[0].Children[0];
                        string newKey = subRow.Children[0].InnerText.FirstWord();
                        string newValue = subRow.Children[1].InnerText.Trim();
                        if (String.IsNullOrEmpty(newKey))
                        {
                            value = value + "\r\n" + newValue;
                        }
                        else
                        {
                            if (!String.IsNullOrEmpty(key))
                            {
                                details[key] = value;
                            }
                            key = newKey;
                            value = newValue;
                        }
                    }
                    catch (Exception)
                    {
                    }

                    c = i.MoveNext() ? (HtmlElement)i.Current : null;
                }

                if (!String.IsNullOrEmpty(key))
                {
                    details[key] = value;
                }

                return details;
            }

            double ParseValue(string v)
            {
                return Double.Parse(Regex.Split(v, @"\s+")[0]);
            }
        }
        
        #region ICollector Members

        public IList<Payment> GetPayments()
        {
            Scrape scrape = new Scrape();
            scrape.user = user;
            scrape.pass = pass;
            scrape.Run();
            return scrape.Payments;
        }

        #endregion
    }
}

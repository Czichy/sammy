// Copyright (c) 2008, Andreas Grimme (http://andreas-grimme.gmxhome.de/)
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Sidi.Sammy.Dkb
{
    internal class DkbScrapeForm : ScrapeForm
    {
        public DkbScrapeForm()
        {
            BodyHandler = this.init;
        }

        internal class ResultTable : List<Dictionary<string, string>>
        {
        }

        internal ResultTable Result;
        string CreditCard;

        public void test2(HtmlElement b)
        {
            // <select class="pulldownPeriod long" name="slCreditCard" onchange="searchbuttonclick();" id="id-772007528_slCreditCard">
            HtmlElement selectCard = b.Find(x => x.TagName == "SELECT" && x.AttributeContains("id", "_slCreditCard"));
            string[] ccp = System.Text.RegularExpressions.Regex.Split(selectCard.GetSelectText(), "\\s*\\/\\s*");
            CreditCard = ccp[0];
            Result = ReadSearchResultTable(b);
            this.Close();
        }

        ResultTable ReadSearchResultTable(HtmlElement body)
        {
            HtmlElement t = body.Find(x => x.TagName == "TABLE" && x.AttributeIs("className", "searchResultTable"));
            ResultTable data = new ResultTable();
            foreach (HtmlElement tr in t.GetElementsByTagName("TR"))
            {
                Dictionary<string, string> row = new Dictionary<string, string>();
                row["OwnAcctId"] = CreditCard;
                foreach (HtmlElement td in tr.GetElementsByTagName("TD"))
                {
                    string[] idp = td.GetAttribute("headers").Split(new char[] { ':' });
                    if (idp.Length > 1)
                    {
                        string id = idp[1];
                        if (!String.IsNullOrEmpty(id))
                        {
                            row[id] = td.InnerText;
                        }
                    }
                }
                data.Add(row);
            }
            return data;
        }

        public void init(HtmlElement b)
        {
            Browser.Url = new Uri("https://banking.dkb.de/dkb/-?$part=Welcome.login");
            BodyHandler = this.login;
        }

        public string User;
        public string Password;

        public void login(HtmlElement b)
        {
            BodyHandler = this.LogonFinished;
            b.SetInput("username", User);
            b.SetInput("password", Password);
            b.ClickButton("buttonlogin");
        }

        List<string> paymentTransaction;

        void LogonFinished(HtmlElement b)
        {
            paymentTransaction = new List<string>();
            foreach (HtmlElement i in b.FindAll(x => x.TagName == "A" && x.AttributeContains("href", "paymentTransaction")))
            {
                paymentTransaction.Add(i.GetAttribute("href"));
            }
            paymentTransaction.RemoveAt(0);
            Result = new ResultTable();
            GetNextPaymentTransaction(b);
        }

        void GetNextPaymentTransaction(HtmlElement b)
        {
            if (paymentTransaction.Count == 0)
            {
                Logout(b);
            }
            else
            {
                string href = paymentTransaction[0];
                paymentTransaction.RemoveAt(0);
                BodyHandler = this.StatementLoaded;
                Browser.Url = new Uri(href);
            }
        }

        void StatementLoaded(HtmlElement b)
        {
            BodyHandler = this.StatementNextPage;
            b.Check("searchPeriod:0");
            DateTime postingDate = DateTime.Now - new TimeSpan(360, 0, 0, 0);
            b.SetInput("postingDate", postingDate.ToShortDateString());
            b.ClickButton("searchbutton");
        }

        void StatementNextPage(HtmlElement b)
        {
            // <select class="pulldownPeriod long" name="slCreditCard" onchange="searchbuttonclick();" id="id-772007528_slCreditCard">
            HtmlElement selectCard = b.Find(x => x.TagName == "SELECT" && x.AttributeContains("id", "_slCreditCard"));
            string[] ccp = System.Text.RegularExpressions.Regex.Split(selectCard.GetSelectText(), "\\s*\\/\\s*");
            CreditCard = ccp[0];
            Result.AddRange(ReadSearchResultTable(b));
            HtmlElement next = b.Find(x => x.AttributeContains("ClassName", "nextPage"));
            if (next == null)
            {
                GetNextPaymentTransaction(b);
            }
            else
            {
                BodyHandler = this.StatementNextPage;
                next.ClickElement();
            }
        }

        void Logout(HtmlElement b)
        {
            BodyHandler = null;
            b.Find(x => x.TagName == "INPUT" && x.AttributeIs("id", "logout")).ClickElement();
        }

    }
}

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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace screen_scrape
{
    public partial class DkbScrapeForm : Form
    {
        public DkbScrapeForm()
        {
            InitializeComponent();
            this.Location = new Point(0, 0);
            bodyHandler = this.test;
            webBrowser1.ScriptErrorsSuppressed = true;
            webBrowser1.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(webBrowser1_DocumentCompleted);
        }

        void Form1_Load(object sender, EventArgs e)
        {
        }

        public delegate void HandleBodyFunc(HtmlElement e);

        HandleBodyFunc bodyHandler;

        void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            try
            {
                HtmlElement b = webBrowser1.Document.Body;
                if (bodyHandler != null)
                {
                    bodyHandler(b);
                }
            }
            catch (Exception ex)
            {
            }
        }


        public void test(HtmlElement b)
        {
            bodyHandler = this.test2;
            webBrowser1.Url = new Uri("file://d:/temp/dkb.html");
        }

        public class ResultTable : List<Dictionary<string,string>>
        {
        }

        public ResultTable Result;

        public void test2(HtmlElement b)
        {
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
                data.Add(row);
                foreach (HtmlElement td in t.GetElementsByTagName("TD"))
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
            }
            return data;
        }
            
        public void init(HtmlElement b)
        {
            webBrowser1.Url = new Uri("https://banking.dkb.de/dkb/-?$part=Welcome.login");
            bodyHandler = this.login;
        }

        public void login(HtmlElement b)
        {
            bodyHandler = this.LogonFinished;
            b.SetInput("j_username", this.User);
            b.SetInput("j_password", this.Password);
            b.ClickButton("buttonlogin");
        }

        void LogonFinished(HtmlElement b)
        {
            GetCreditCardStatement();
        }

        void GetCreditCardStatement()
        {
            bodyHandler = this.StatementLoaded;
            webBrowser1.Url = new Uri("https://banking.dkb.de/dkb/-?$part=DkbTransactionBanking.index.menu&treeAction=selectNode&node=2.1&tree=menu");
        }

        void StatementLoaded(HtmlElement b)
        {
            bodyHandler = this.StatementNextPage;
            b.Check("searchPeriod:0");
            b.SetInput("postingDate", "26.08.2007");
            b.ClickButton("searchbutton");
        }

        void StatementNextPage(HtmlElement b)
        {
            bodyHandler = this.StatementNextPage;
            b.Find(x => x.AttributeContains("ClassName", "nextPage")).ClickElement();
        }
    }
}

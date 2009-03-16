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
using log4net;

namespace Sidi.Sammy
{
    class ScrapeApplicationContext : ApplicationContext
    {
        Form scrapeWindow;
        
        public ScrapeApplicationContext(Form scrapeWindow)
        {
            this.scrapeWindow = scrapeWindow;
            InitializeComponent();
            scrapeWindow.FormClosed += new FormClosedEventHandler(scrapeWindow_FormClosed);
        }

        void scrapeWindow_FormClosed(object sender, FormClosedEventArgs e)
        {
            ExitThreadCore();
        }

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
        }
    }

    public partial class ScrapeForm : Form
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static bool ShowBrowser = false;

        public void Run()
        {
            exception = null;
            this.Show();
            this.Visible = ShowBrowser;
            Application.Run(new ScrapeApplicationContext(this));
            if (exception != null)
            {
                throw exception;
            }
        }

        public ScrapeForm()
        {
            InitializeComponent();
            Browser.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(webBrowser1_DocumentCompleted);
            Browser.ScriptErrorsSuppressed = true;
            timeoutTimer.Tick += new EventHandler(timeoutTimer_Tick);
        }

        void timeoutTimer_Tick(object sender, EventArgs e)
        {
            string url = String.Empty;
            try
            {
                url = Browser.Url.ToString();
            }
            catch (Exception)
            {
            }
            exception = new TimeoutException(url);
            this.Close();
        }

        public delegate void HandleBodyFunc(HtmlElement e);

        public HandleBodyFunc BodyHandler;
        Exception exception = null;
        TimeSpan timeout = new TimeSpan(0, 1, 0);
        Timer timeoutTimer = new Timer();

        public TimeSpan Timeout
        {
            get { return timeout; }
            set { timeout = value; }
        }

        void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            try
            {
                if (!Browser.Url.Equals(e.Url))
                {
                    return;
                }

                timeoutTimer.Stop();    
                timeoutTimer.Interval = (int) timeout.TotalMilliseconds;
                timeoutTimer.Start();
                HtmlElement b = Browser.Document.Body;
                if (BodyHandler == null)
                {
                    log.Info("scraping complete");
                    this.Close();
                }
                else
                {
                    log.InfoFormat("Handler: {0}", BodyHandler.Method.Name);
                    BodyHandler(b);
                }
            }
            catch (Exception ex)
            {
                log.Error("scraping error", ex);
                this.Close();
                exception = ex;
            }
        }

    }
}

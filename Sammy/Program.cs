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
using Sidi.Persistence;
using System.Net.Sockets;
using System.Windows.Forms;
using System.Threading;
using log4net;
using Pop3 = Sidi.Net.Pop3;
using Sidi.IO;
using log4net.Appender;
using log4net.Layout;
using System.IO;
using log4net.Repository.Hierarchy;
using log4net.Core;

// Configure log4net using the .config file
[assembly: log4net.Config.XmlConfigurator(Watch = true)]
// This will cause log4net to look for a configuration file
// called ConsoleApp.exe.config in the application base
// directory (i.e. the directory containing ConsoleApp.exe)

namespace Sidi.Sammy
{
    class Program
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [STAThread]
        static void Main(string[] args)
        {
            // configure logging
            {
                RollingFileAppender a = new RollingFileAppender();
                a.DatePattern = "yyyy-MM-dd";
                a.AppendToFile = true;
                a.StaticLogFileName = false;
                a.File = FileUtil.CatDir(new string[]{ Program.DataDirectory, "log", "log-"});
                // Directory.GetParent(a.File).Create();
                a.RollingStyle = RollingFileAppender.RollingMode.Date;
                a.Layout = new PatternLayout("%date [%thread] %-5level %logger [%property{NDC}] - %message%newline");
                a.ActivateOptions();
                log4net.Config.BasicConfigurator.Configure(a);

                Hierarchy repository = log4net.LogManager.GetRepository() as Hierarchy;
                repository.Root.Level = Level.Debug;
            }

            log.Info("Startup");
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Sidi.Net.Pop3.Server server = new Sidi.Net.Pop3.Server();
            server.MailboxProvider = delegate(TcpClient client, string user, string pass)
            {
                Account a = new Account(user, pass);
                if (a.Exists && a.Read())
                {
                    Account.DefaultAccount = a;

                    // start collector thread
                    Thread collectorThread = new Thread
                    (
                        new ParameterizedThreadStart(delegate(object p)
                        {
                            Account account = (Account)p;
                            account.CheckCollect();
                        })
                    );
                    collectorThread.SetApartmentState(ApartmentState.STA);
                    collectorThread.Start(a);
                    return new PaymentMailBox(a);
                }
                else
                {
                    return null;
                }
            };

            try
            {
                server.StartLoopback();
            }
            catch (SocketException e)
            {
                log.Error("POP3 server cannot be started. Check if another POP3 server is already running.", e);
                MessageBox.Show("POP3 server cannot be started. Check if another POP3 server is already running.", Sammy.ApplicationName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Application.Run(new MyApplicationContext());
            server.Stop();
            log.Info("Shutdown");
        }

        public static string DataDirectory
        {
            get
            {
                string p = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                p = System.IO.Path.Combine(p, Sammy.ApplicationName);
                return p;
            }
        }
    }
}

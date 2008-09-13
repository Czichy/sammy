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
            log.Info("Startup");
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Sidi.Net.Pop3.Server server = new Sidi.Net.Pop3.Server();
            server.MailboxProvider = delegate(TcpClient client, string user, string pass)
            {
                Account a = new Account(user, pass);
                if (a.Exists && a.Read())
                {
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
    }
}

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
using System.IO;
using System.Xml.Serialization;
using System.Windows.Forms;
using System.Xml;
using Sidi.Persistence;
using log4net;

namespace Sidi.Sammy
{
    public class Account : IDisposable
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        string user;
        string password;
        Settings settings;

        public string User { get { return user; } }
        public string Password { set { password = value; } }

        public string UserFileName
        {
            get{
                return System.Text.RegularExpressions.Regex.Replace(User, @"\W", "_");
            }
        }
        
        public Collection<Payment> Payments
        {
            get
            {
                return new Collection<Payment>(DbPath);
            }
        }

        public Collection<PaymentMailBox.ReadFlag> ReadFlags
        {
            get
            {
                return new Collection<PaymentMailBox.ReadFlag>(DbPath);
            }
        }

        public Settings Settings
        {
            get
            {
                return settings;
            }
        }

        public Account(string user, string password)
        {
            log.InfoFormat("Ctor user={0}", user);
            this.user = user;
            this.password = password;
        }

        public Account()
        {
        }

        public bool Exists
        {
            get
            {
                return File.Exists(DbPath);   
            }
        }

        public void Create()
        {
            if (Exists)
            {
                throw new System.IO.IOException("Exists: " + DbPath);
            }
            log.InfoFormat("Create user={0}", user);
            Directory.GetParent(DbPath).Create();
            settings = new Settings();
            Collectors c = new Collectors();
            StringWriter w = new StringWriter();
            c.CreateSampleScript(w);
            settings.Script = w.ToString();
            Write();
        }

        public void Delete()
        {
            if (Exists)
            {
                File.Delete(DbPath);
            }
        }

        public static Account DefaultAccount
        {
            get; set;
        }

        public bool PromptForCredentials()
        {
            CredentialsDlg cred = new CredentialsDlg();
            while (true)
            {
                if (cred.ShowDialog() != DialogResult.OK)
                {
                    return false;
                }

                user = cred.textBoxUser.Text;
                password = cred.textBoxPassword.Text;

                if (!Exists || !Read())
                {
                    System.Windows.Forms.MessageBox.Show("Wrong user or password.");
                    continue;
                }

                return true;
            }
        }

        public bool ChangePassword()
        {
            ChangePasswordDlg dlg = new ChangePasswordDlg();
            while (dlg.ShowDialog() == DialogResult.OK)
            {
                Password = dlg.textBoxPassword.Text;
                return true;
            }
            return false;
        }

        public void Write()
        {
            log.InfoFormat("Write user={0}", user);
            MemoryStream m = new MemoryStream();
            Stream s = CryptUtil.EncryptStream(m, password);
            XmlSerializer ser = new XmlSerializer(typeof(Settings));
            XmlTextWriter xmlw = new XmlTextWriter(new StreamWriter(s));
            ser.Serialize(xmlw, settings);
            xmlw.Close();
            s.Close();
            Config<byte[]>()["script"] = m.ToArray();
        }

        public Sidi.Persistence.Dictionary<string, TValue> Config<TValue>()
        {
            return new Sidi.Persistence.Dictionary<string, TValue>(DbPath, "config");
        }
        
        public bool Read()
        {
            log.InfoFormat("Read user={0}", user);

            MemoryStream m = new MemoryStream(Config<byte[]>()["script"]);

            Stream s = CryptUtil.DecrytStream(m, password);
            try
            {
                XmlSerializer ser = new XmlSerializer(typeof(Settings));
                XmlTextReader xmlr = new XmlTextReader(new StreamReader(s));
                settings = (Settings)ser.Deserialize(xmlr);
                xmlr.Close();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                s.Close();
            }
        }

        string DbPath
        {
            get
            {
                string p = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                p = Path.Combine(p, Sammy.ApplicationName);
                return Path.Combine(p, UserFileName + ".sqlite");
            }
        }



        public DateTime NextCollect
        {
            get
            {
                DateTime v;
                if (!Config<DateTime>().TryGetValue("nextCollect", out v))
                {
                    v = DateTime.MinValue;
                }
                return v;
            }

            set
            {
                Config<DateTime>()["nextCollect"] = value;
                log.InfoFormat("Collect time for user {0} is set to {1}", User, value);
            }
        }

        public void CheckCollect()
        {
            DateTime nextCollect = NextCollect;
            log.InfoFormat("Collect time for user {0} is {1}", User, nextCollect);
            if (nextCollect < DateTime.Now)
            {
                NextCollect = DateTime.Now + new TimeSpan(1, 0, 0);
                try
                {
                    Collect();
                }
                catch (Exception e)
                {
                    log.Error("error during account collection", e);
                }
            }
        }

        public void Collect()
        {
            using (ThreadContext.Stacks["NDC"].Push(User))
            {
                log.InfoFormat("Collecting statements for user {0}", User);
                Collectors collector = new Collectors();
                collector.Payments = Payments;
                collector.Exec(Settings.Script);
                log.InfoFormat("Finished collecting statements for user {0}", User);
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}

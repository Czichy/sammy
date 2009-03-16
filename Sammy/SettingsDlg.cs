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
using System.Diagnostics;
using System.IO;

namespace Sidi.Sammy
{
    public partial class SettingsDlg : Form
    {
        Account account;

        public SettingsDlg(Account account)
        {
            InitializeComponent();
            this.account = account;
            this.account.Read();
            this.Text = String.Format("Settings for {0}", account.User);
            this.textBoxScript.Text = this.account.Settings.Script;
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            account.Settings.Script = textBoxScript.Text;
            account.Write();
            this.Close();
        }

        private void buttonChangePassword_Click(object sender, EventArgs e)
        {
            account.ChangePassword();
        }

        string FinAdminPath
        {
            get
            {
                return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                    @"Subsembly FinTS API\FinAdmin.exe");
            }
        }

        private void buttonHbci_Click(object sender, EventArgs e)
        {
            if (File.Exists(FinAdminPath))
            {
                Process p = new Process();
                p.StartInfo.FileName = FinAdminPath;
                p.Start();
            }
            else
            {
                if (MessageBox.Show("Install HBCI support?", Sammy.ApplicationName,
                    MessageBoxButtons.OKCancel) == DialogResult.OK)
                {
                    Process p = new Process();
                    p.StartInfo.FileName = "http://www.fints-api.de/fintsapi.html";
                    p.StartInfo.UseShellExecute = true;
                    p.Start();
                }
            }
        }

        private void buttonViewStatements_Click(object sender, EventArgs e)
        {
            StatementView v = new StatementView(account);
            v.ShowDialog();
        }

        private void buttonUpdate_Click(object sender, EventArgs e)
        {
            account.NextCollect = DateTime.Now;
        }
    }
}

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

namespace Sidi.Sammy
{
    public partial class CredentialsDlg : Form
    {
        public CredentialsDlg()
        {
            InitializeComponent();
        }

        private void buttonNewUser_Click(object sender, EventArgs e)
        {
            NewAccountDlg newAccount = new NewAccountDlg();
            while (newAccount.ShowDialog() == DialogResult.OK)
            {
                Account a = new Account(newAccount.textBoxUser.Text, newAccount.textBoxPassword.Text);
                a.Create();
                this.textBoxUser.Text = newAccount.textBoxUser.Text;
                this.textBoxPassword.Text = newAccount.textBoxPassword.Text;
                this.DialogResult = DialogResult.OK;
                this.Close();
                return;
            }
        }
    }
}

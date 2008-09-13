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
using System.Data.Common;
using System.Data.SqlClient;
using System.Data.SQLite;

namespace Sidi.Sammy
{
    public partial class StatementView : Form
    {
        public StatementView(Account a)
        {
            InitializeComponent();
            
            DbConnection c = a.Payments.Connection;
            SQLiteDataAdapter adapter = new SQLiteDataAdapter("select * from Payment", c.ConnectionString);
            DataSet ds = new DataSet();
            adapter.Fill(ds);
            dataGridView.DataSource = ds.Tables[0];
            dataGridView.Sort(dataGridView.Columns["EntryDate"], ListSortDirection.Descending);
            foreach (string i in new string[] { "Currency", "Value", "EntryDate", "ValueDate" })
            {
                dataGridView.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCellsExceptHeader;
            }
        }
    }
}

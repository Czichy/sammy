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
using Sidi.CommandLine;
using System.Diagnostics;
using log4net;

namespace Sidi.Sammy.Dkb
{
    [Usage("Credit card statements from https://banking.dkb.de")]
    public class Dkb : ICollector
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public Dkb(string user, string password)
        {
            User = user;
            Password = password;
        }

        public string User;
        public string Password;

        public IList<Sidi.Sammy.Payment> GetPayments()
        {
            DkbScrapeForm f = new DkbScrapeForm();
            f.User = User;
            f.Password = Password;

            f.Run();
            List<Sidi.Sammy.Payment> result = new List<Payment>();
            foreach (Dictionary<string, string> i in f.Result)
            {
                if (!i.ContainsKey("disposalDate"))
                {
                    continue;
                }
                try
                {
                    Sidi.Sammy.Payment p = new Payment();
                    string[] dates = System.Text.RegularExpressions.Regex.Split(i["disposalDate"], "\\s+");
                    p.ValueDate = DateTime.Parse(dates[0]);
                    p.EntryDate = DateTime.Parse(dates[1]);
                    string[] currencies = System.Text.RegularExpressions.Regex.Split(i["currency"], "\\s+");
                    p.Currency = currencies[0];
                    p.Purpose = i["text"];
                    p.Name = i["text"];
                    p.AcctNo = String.Empty;
                    p.BankCode = String.Empty;
                    string[] att = System.Text.RegularExpressions.Regex.Split(i["amountToTransfer"], "\\s+");
                    p.Value = (double) Decimal.Parse(att[0]);
                    if (att[1] == "S")
                    {
                        p.Value = -p.Value;
                    }
                    else if (att[1] == "H")
                    {
                        // haben
                    }
                    else
                    {
                        throw new System.IO.InvalidDataException(i["amountToTransfer"]);
                    }
                    p.OwnAcctNo = i["OwnAcctId"];
                    p.OwnBankCode = "12030000";
                    result.Add(p);
                }
                catch (Exception e)
                {
                    log.Error(String.Format("Download error. User={0}", User), e);
                }
            }
            return result;
        }
    }
}

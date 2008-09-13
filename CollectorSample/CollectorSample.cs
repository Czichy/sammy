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
using Sidi.CommandLine;
using System.Text.RegularExpressions;
using Sidi.Util;

namespace Sidi.Sammy.CollectorSample
{
    [Usage("Generates random test data")]
    public class CollectorSample : ICollector
    {
        public CollectorSample()
        {
        }

        int itemCount = 10;

        #region ICollector Members

        public IList<Payment> GetPayments()
        {
            Random r = new Random();
            List<Payment> list = new List<Payment>();
            for (int i = 0; i < itemCount; ++i)
            {
                Payment p = new Payment();
                p.Currency = "EUR";
                p.Value = (Decimal)((r.NextDouble() -0.5)* 2000.0);
                p.EntryDate = DateTime.Now;
                p.ValueDate = DateTime.Now;
                p.EntryText = r.String(16);
                p.Name = r.String(16);
                p.AcctNo = r.String(10, @"\d");
                p.BankCode = r.String(10, @"\d");
                p.Purpose = r.String(16);
                p.OwnAcctNo = "1234567890";
                p.OwnBankCode = "1234567890";
                list.Add(p);
            }
            return list;
        }

        #endregion
    }
}

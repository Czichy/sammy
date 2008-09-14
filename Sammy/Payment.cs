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
using System.IO;
using System.Security.Cryptography;
using Sidi.Util;

namespace Sidi.Sammy
{
    public class Payment
    {
        [RowId]
        public long Id;
        [Data]
        public string Currency;
        
        /// <summary>
        /// Amount that was transfered.
        /// </summary>
        /// Using data type Decimal would be better here, but sqlite cannot properly handle 
        /// DECIMAL numbers. See http://sqlite.phxsoftware.com/forums/p/1296/5595.aspx#5595
        [Data]
        public Double Value;
        [Data]
        public DateTime EntryDate;
        [Data]
        public DateTime ValueDate;
        [Data]
        public string EntryText;
        [Data]
        public string Name;
        [Data]
        public string AcctNo;
        [Data]
        public string BankCode;
        [Data]
        public string Purpose;
        [Data]
        public string OwnName;
        [Data]
        public string OwnAcctNo;
        [Data]
        public string OwnBankCode;

        public void FormatValue(TextWriter w, string key, string value)
        {
            if (value == null)
            {
                value = String.Empty;
            }
            StringReader r = new StringReader(value);
            w.WriteLine(String.Format("{0,20}: {1}", key, r.ReadLine()));
            for (string line = r.ReadLine(); line!=null; line = r.ReadLine())
            {
                w.WriteLine(String.Format("{0,20}: {1}", " ", line));
            }
        }

        public void Print(TextWriter o)
        {
            FormatValue(o, "Amount", String.Format("{1} {0}", Currency, String.Format("{0,32:0.00}", Value)));
            FormatValue(o, "Payer/Payee", Name);
            FormatValue(o, "Purpose", Purpose);
            FormatValue(o, "Entry/Value", String.Format("{0} / {1}", EntryDate.ToShortDateString(), ValueDate.ToShortDateString()));
            FormatValue(o, "Type", EntryText);
            FormatValue(o, "AcctNo/BankCode", String.Format("{0}/{1}", AcctNo, BankCode));
            FormatValue(o, "Own Name", OwnName);
            FormatValue(o, "Own AcctNo/BankCode", String.Format("{0}/{1}", OwnAcctNo, OwnBankCode));
        }

        static MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();

        public string Digest
        {
            get
            {
                MemoryStream m = new MemoryStream();
                StreamWriter w = new StreamWriter(m);
                w.WriteLine(Value);
                w.WriteLine(EntryDate);
                w.WriteLine(AcctNo);
                w.WriteLine(BankCode);
                w.WriteLine(Purpose);
                w.Flush();
                m.Seek(0, SeekOrigin.Begin);
                return md5.ComputeHash(m).HexString();
            }
        }
    }
}

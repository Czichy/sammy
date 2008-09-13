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
using System.Data.Common;
using Sidi.Persistence;
using System.IO;
using Sidi.Util;

namespace Sidi.Sammy
{
    public class PaymentMailBox : Sidi.Net.Pop3.IMailBox
    {
        public class Mail : Sidi.Net.Pop3.IMailItem
        {
            public long id;
            PaymentMailBox mailBox;
            Payment payment;

            public Mail(long a_id, PaymentMailBox a_mailBox)
            {
                id = a_id;
                mailBox = a_mailBox;
                payment = mailBox.payments[id];
            }
            
            #region IMailItem Members

            public string Uid
            {
                get
                {
                    return payment.Digest;
                }
            }

            string content;

            public string Content
            {
                get
                {
                    if (content == null)
                    {
                        StringWriter w = new StringWriter();
                        WriteHeader(w);
                        payment.Print(w);
                        content = w.ToString();
                    }
                    return content;
                }
            }

            void WriteHeader(TextWriter w)
            {
                Payment p = payment;
                w.WriteLine(String.Format("Date:     {0}", p.EntryDate.ToString("R")));
                w.WriteLine(String.Format("From:     \"{0}\" <sammy@localhost>", p.Name.OneLine(80)));
                w.WriteLine("Sender:   sammy@localhost");
                w.WriteLine(String.Format("To:     \"{0}\" <sammy-user@localhost>", p.OwnName.OneLine(80)));
                w.WriteLine(String.Format("Subject:   {0:0.00} {1} {2}", p.Value, p.Currency, p.Purpose.OneLine(256)));
                w.WriteLine(String.Format("Message-ID:  <{0}@sammy>", p.Digest));
                w.WriteLine();
            }

            public string Top(int lines)
            {
                StringWriter w = new StringWriter();
                WriteHeader(w);
                return w.ToString();
            }

            #endregion
        }

        public class ReadFlag
        {
            [RowId]
            public long Id;
            [Data, Indexed, Unique]
            public long PaymentId;
        }

        public Collection<Payment> payments;
        Collection<ReadFlag> readFlags;
        
        public PaymentMailBox(Account account)
        {
            payments = account.Payments;
            readFlags = account.ReadFlags;
            Fill();
        }

        string UnreadMailsQuery
        {
            get
            {
                return String.Format(
                    "select p.oid from Payment as p left outer join {0} as r on p.oid = r.PaymentId where r.oid isnull",
                    readFlags.Table);
            }
        }

        List<Sidi.Net.Pop3.IMailItem> mails;

        public void Fill()
        {
            mails = new List<Sidi.Net.Pop3.IMailItem>();
            DbCommand c = readFlags.Connection.CreateCommand();
            c.CommandText = UnreadMailsQuery;
            DbDataReader r;
            for (r = c.ExecuteReader(); r.Read(); )
            {
                long id = r.GetInt64(0);
                mails.Add(new Mail(id, this));
            }
            r.Close();
        }
    
        #region IMailBox Members

        public IList<Sidi.Net.Pop3.IMailItem>  Mails
        {
	        get { return mails; }
        }

        public void  Update(bool[] deleteFlags)
        {
 	        DbTransaction t = readFlags.BeginTransaction();
            for (int i=0; i<Mails.Count; ++i)
            {
                if (deleteFlags[i])
                {
                    ReadFlag r = new ReadFlag();
                    r.PaymentId = ((Mail)mails[i]).id;
                    readFlags.Add(r);
                }
            }
            t.Commit();
        }

        #endregion
    }
}

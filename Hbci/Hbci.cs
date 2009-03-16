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
using Subsembly.FinTS;
using Subsembly.Swift;
using Sidi.Util;
using Sidi.CommandLine;
using Sidi.Sammy;
using System.Diagnostics;
using log4net;
using System.IO;
using System.Reflection;

namespace Sidi.Sammy.Hbci
{
    [Usage("Any bank that supports HBCI. Requires to download http://www.fints-api.de/download/FinApiSetup.msi")]
    class Hbci : ICollector
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public Hbci(string accountId, string password)
        {
            AccountId = accountId;
            Password = password;

            AppDomain.CurrentDomain.AssemblyResolve +=new ResolveEventHandler(CurrentDomain_AssemblyResolve);
        }

        string FintsPath
        {
            get
            {
                return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                    @"Subsembly FinTS API\Assemblies");
            }
        }

        static Dictionary<string, Assembly> assemblies = new Dictionary<string, Assembly>();

        Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            if (!assemblies.ContainsKey(args.Name))
            {
                string[] d = args.Name.Split(new char[] { ',' });
                string p = Path.Combine(FintsPath, d[0] + ".dll");
                assemblies[args.Name] = Assembly.LoadFile(p);
            }
            return assemblies[args.Name];
        }
        
        string AccountId;
        string Password;

        #region ICollector Members

        public IList<Payment> GetPayments()
        {
            List<Payment> result = new List<Payment>();
            foreach (Statement i in DownloadStatement())
            {
                result.AddRange(i.Payments);
            }
            return result;
        }

        #endregion

        IList<Statement> DownloadStatement()
        {
            log.DebugFormat("Download {0}", AccountId);
            FinContact contact = FinContactFolder.Default.FindContact(AccountId);
            List<Statement> s = new List<Statement>();
            foreach (FinAcctInfo aAcctInfo in contact.UPD)
            {
                try
                {
                    FinAcct acct = aAcctInfo.Acct;
                    log.DebugFormat("Downloading account {0}/{1}", acct.BankCode, acct.AcctNo);
                    FinService service = new FinService(contact, acct.BankCode, acct.AcctNo, acct.Currency);
                    service.LogOn(Password);
                    FinAcctMvmtsSpecifiedPeriod aAcctMvmts = service.DownloadStatement(SwiftDate.NullDate);
                    service.LogOff();

                    if (aAcctMvmts != null)
                    {
                        s.Add(ToStatement(acct, aAcctMvmts.BookedTrans));
                    }
                }
                catch (Exception e)
                {
                    log.Error("Error while downloading " + AccountId, e);
                }
            }
            return s;
        }

        Payment ToPayment(FinAcct account, SwiftStatementLine s)
        {
            Payment p = new Payment();

            p.Currency = s.Currency;
            p.Value = (double) s.DecValue;
            p.EntryDate = s.EntryDate.ToDateTime();
            p.ValueDate = s.ValueDate.ToDateTime();
            p.EntryText = s.EntryText;
            p.Name = new string[] { s.PayeePayerName1, s.PayeePayerName2 }.JoinLines();
            p.AcctNo = s.PayeePayerAcctNo;
            p.BankCode = s.PayeePayerBankCode;
            p.Purpose = s.PaymtPurpose.JoinLines();
            p.OwnAcctNo = account.AcctNo;
            p.OwnBankCode = account.BankCode;

            return p;
        }

        Statement ToStatement(FinAcct acct, SwiftStatementReader aStmtReader)
        {
            Statement s = new Statement();
            s.Payments = new List<Payment>();
            SwiftStatement aMT940 = SwiftStatement.ReadMT940(aStmtReader, true);
            s.ClosingBalance = aMT940.ClosingBalance.DecValue;
            s.AccountIdentification = aMT940.AccountIdentification;
            foreach (SwiftStatementLine sl in aMT940.StatementLines)
            {
                s.Payments.Add(ToPayment(acct, sl));
            }
            return s;
        }
    }
}

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
using mshtml;
using Sidi.Util;
using System.Text.RegularExpressions;
using System.Globalization;

namespace Sidi.Sammy.Dkb
{
    [Usage("Credit card statements from https://banking.dkb.de")]
    public class Dkb : ICollector
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public Dkb(string user, string password)
        {
            User = user;
            Password = password;
        }

        public string User;
        public string Password;

        CultureInfo culture = CultureInfo.GetCultureInfo("de-DE");

        void ParseValue(string text, out double value, out string currency)
        {
            var p = Regex.Split(text, @"\s+");
            var sign = p[1].Equals("S") ? -1.0 : 1.0;
            value = sign * Double.Parse(p[0], culture);
            currency = p[2];
        }

        const string AcctNoKey = "Kontonummer/ Bankleitzahl:";

        string GetValue(Dictionary<string, string> d, params string[] keys)
        {
            var k = keys.FirstOrDefault(i => d.ContainsKey(i));
            return k == null ? String.Empty : d[k];
        }

        public IList<Sidi.Sammy.Payment> GetPayments()
        {
            var session = new DkbSession(User, Password);
            session.Login();
            try
            {
                var t = (IHTMLElement) session.GetSearchResultTable();
                var accounts = t.ParseTable();
                accounts = accounts.Take(accounts.Length - 1).ToArray();
                if (log.IsDebugEnabled) log.Debug(accounts.Dump());

                var payments = new List<Sidi.Sammy.Payment>();
                
                int accountIndex = 0;
                foreach (var a in accounts)
                {
                    var url = String.Format("https://banking.dkb.de/dkb/-?$part=DkbTransactionBanking.content.banking.FinancialStatus.FinancialStatus&$event=paymentTransaction&table=cashTable&row={0}", accountIndex);
                    session.Navigate(url);
                    var transactions = session.GetSearchResultTable();

                    var transactionUrls = transactions.GetAllChildren()
                        .Where(img => img.tagName.Equals("IMG", StringComparison.InvariantCultureIgnoreCase) && img.GetAttribute("alt").Equals("Details"))
                        .Select(img => img.parentElement.GetAttribute("href"))
                        .ToList();

                    log.Debug(transactionUrls.Join());

                    foreach (var transactionUrl in transactionUrls)
                    {
                        session.Navigate(transactionUrl);

                        var detailsTable = session.GetWhiteTable().ParseTable();
                        if (log.IsDebugEnabled) log.Debug(detailsTable.Dump());
                        var details = new Dictionary<string, string>();
                        foreach (var i in detailsTable)
                        {
                            details[i[0].Trim()] = i[1].Trim();
                        }
                        if (log.IsDebugEnabled)
                        {
                            foreach (var i in details)
                            {
                                log.InfoFormat("{0}={1}", i.Key.Quote(), i.Value.Quote());
                            }
                        }

                        var p = new Payment();
                        p.OwnAcctNo = a[0];
                        p.OwnName = a[1];
                        p.OwnBankCode = "12030000";

                        var dateString = GetValue(details, "Buchungstag:", "Belegdatum:");
                        if (!String.IsNullOrEmpty(dateString))
                        {
                            p.EntryDate = DateTime.Parse(dateString, culture);
                        }
                            
                        p.ValueDate = DateTime.Parse(details["Wertstellung:"], culture);
                        ParseValue(details["Betrag:"], out p.Value, out p.Currency);
                        details.TryGetValue("Auftraggeber/Begünstigter:", out p.Name);

                        if (details.ContainsKey(AcctNoKey))
                        {
                            var parts = Regex.Split(details[AcctNoKey], @"\s*/\s*");
                            p.AcctNo = parts[0];
                            p.BankCode = parts[1];
                        }

                        p.EntryText = GetValue(details, "Buchungstext:");

                        p.Purpose = GetValue(details, "Verwendungszweck:", "Umsatzbeschreibung:");

                        payments.Add(p);
                    }
                    ++accountIndex;
                }

                return payments;
            }
            finally
            {
                session.Logout();
            }
        }
    }
}

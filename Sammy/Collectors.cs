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
using Cmd = Sidi.CommandLine;
using Sidi.Util;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using log4net;

namespace Sidi.Sammy
{
    [Usage("Collects payment statements")]
    public class Collectors : CommandLineHandler
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public Sidi.Persistence.Collection<Payment> Payments;

        IDictionary<string, Type> collectors = new Dictionary<string, Type>();

        public Collectors()
        {
        }

        [Usage("If true, shows the browser window during screen scraping.")]
        public bool ShowBrowser
        {
            set
            {
                ScrapeForm.ShowBrowser = value;
            }

            get
            {
                return ScrapeForm.ShowBrowser;
            }
        }

        [Usage("Execute command options from scriptFile")]
        public void ExecFile(string scriptFile)
        {
            Tokenizer tokenizer = new Sidi.Util.Tokenizer(new StreamReader(scriptFile));
            string[] args = tokenizer.Tokens.ToArray();
            Sidi.CommandLine.Parser.Run(this, args);
        }

        public void Exec(string script)
        {
            Tokenizer tokenizer = new Sidi.Util.Tokenizer(new StringReader(script));
            string[] args = tokenizer.Tokens.ToArray();
            Sidi.CommandLine.Parser.Run(this, args);
        }

        static string Syntax(Type collectorType)
        {
            ConstructorInfo i = collectorType.GetConstructors()[0];
            string parameters = String.Join(" ", Array.ConvertAll(i.GetParameters(), new Converter<ParameterInfo, string>(delegate(ParameterInfo pi)
            {
                return String.Format("[{1} {0}]", pi.Name, pi.ParameterType.Name);
            })));
            return String.Format("{0} {1}", collectorType.Name, parameters);
        }

        public void CreateSampleScript(TextWriter w)
        {
            w.WriteLine("# This is a sample configuration script for sammy.");
            w.WriteLine("# Uncomment the lines below to configure how sammy collects");
            w.WriteLine("# your account statements.");
            w.WriteLine("# Check http://code.google.com/p/sammy/wiki/ConfigurationScript for details.");
            w.WriteLine();

            Parser p = new Parser(this);
            foreach (Cmd.Action i in p.Actions)
            {
                i.PrintScriptFileSample(w);
            }
            foreach (Option i in p.Options)
            {
                i.PrintScriptFileSample(w);
            }

            w.WriteLine("# Collector plugins");
            w.WriteLine();

            foreach (Type i in CollectorPlugins)
            {
                string u = Usage.Get(i);
                if (u != null)
                {
                    w.Write("# ");  w.WriteLine(u);
                    w.Write("# "); w.WriteLine(Collectors.Syntax(i));
                    w.WriteLine();
                }
            }
        }

        string PluginDir
        {
            get
            {
                string p = Assembly.GetExecutingAssembly().Location;
                p = Directory.GetParent(p).FullName;
                return p;
            }
        }

        IEnumerable<Type> CollectorPlugins
        {
            get
            {
                DirectoryInfo d = new DirectoryInfo(PluginDir);
                foreach (FileInfo i in d.GetFiles("*.dll"))
                {
                    Assembly a = Assembly.LoadFile(i.FullName);
                    foreach (Type ci in a.GetTypes())
                    {
                        if (typeof(ICollector).IsAssignableFrom(ci))
                        {
                            yield return ci;
                        }
                    }
                }
            }
        }

        Dictionary<string, string> accountAlias = new Dictionary<string, string>();

        [Usage("Specify a human readable name for an account")]
        public void AccountAlias(string bankCode, string accountNo, string alias)
        {
            accountAlias.Add(String.Join("/", new string[] { bankCode, accountNo }), alias);
        }

        string OwnName(Payment p)
        {
            string id = String.Join("/", new string[] { p.OwnBankCode, p.OwnAcctNo });
            if (accountAlias.ContainsKey(id))
            {
                return accountAlias[id];
            }
            else
            {
                return String.Format("{0}", id);
            }
        }

        public void GetPayments(ICollector collector)
        {
            log.InfoFormat("Start collecting with collector {0}", collector.GetType());
            IList<Payment> payments = collector.GetPayments();
            log.InfoFormat("Finished collecting with collector {0}. {1} transactions collected.", collector.GetType(), payments.Count);
            AddPayments(payments);

        }

        public void AddPayments(IEnumerable<Payment> payments)
        {
            int totalCount = 0;
            int newCount = 0;
            foreach (Payment p in payments)
            {
                ++totalCount;
                p.OwnName = OwnName(p);
                if (!Payments.Contains(p))
                {
                    Payments.Add(p);

                    ++newCount;
                    StringWriter w = new StringWriter();
                    w.WriteLine("new:");
                    p.Print(w);
                    log.Debug(w.ToString());
                }
                else
                {
                    StringWriter w = new StringWriter();
                    w.WriteLine("known:");
                    p.Print(w);
                    log.Debug(w.ToString());
                }
            }
            log.InfoFormat("Total: {0}, New: {1}", totalCount, newCount);
        }

        public Type GetCollector(string name)
        {
            string p = Assembly.GetExecutingAssembly().Location;
            p = Directory.GetParent(p).FullName;
            p = Path.Combine(p, name + ".dll");
            Assembly a = Assembly.LoadFile(p);
            foreach (Type i in a.GetTypes())
            {
                if (typeof(ICollector).IsAssignableFrom(i) && i.Name == name)
                {
                    return i;
                }
            }
            throw new InvalidDataException(name);
        }

        #region CommandLineHandler Members

        public void BeforeParse(IList<string> args)
        {
        }

        public void UnknownArgument(IList<string> args)
        {
            Type collectorType = GetCollector(args[0]);
            args.RemoveAt(0);

            ConstructorInfo ci = collectorType.GetConstructors()[0];
            ParameterInfo[] parameters = ci.GetParameters();
            object[] parameterValues = new object[parameters.Length];
            for (int i = 0; i < parameters.Length; ++i)
            {
                parameterValues[i] = Parser.ParseValue(args[0], parameters[i].ParameterType);
                args.RemoveAt(0);
            }
            ICollector collector = (ICollector) ci.Invoke(parameterValues);
            GetPayments(collector);
            
        }

        #endregion
    }
}

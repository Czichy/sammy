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
using NUnit.Framework;
using Sidi.Sammy;
using System.Reflection;
using System.IO;
using System.Data;
using Sidi.Util;
using Sidi.Persistence;
using log4net.Appender;
using log4net.Layout;

namespace Sidi.Sammy.Test
{
    [TestFixture]
    public class AccountUt : TestBase
    {
        [SetUp]
        public void SetUp()
        {
            a = new Account(user, pass);
            if (a.Exists)
            {
                a.Delete();
            }
            a.Create();
        }

        [TearDown]
        public void TearDown()
        {
            a.Dispose();
        }

        Account a;

        [Test]
        public void Create()
        {
            if (a.Exists)
            {
                a.Delete();
            }
            a.Create();
        }

        [Test]
        public void ReadWrite()
        {
            string script = "hello, script";
            a.Settings.Script = script;
            a.Write();

            using (Account b = new Account(user, pass))
            {
                b.Read();
                Assert.AreEqual(script, b.Settings.Script);
            }
        }

        [Test]
        public void Hbci()
        {
            Collectors c = new Collectors();
            c.Payments = a.Payments;
            c.ExecFile(SecretFile("test-hbci.command"));
        }

        [Test]
        public void Dkb()
        {
            Collectors c = new Collectors();
            c.Payments = a.Payments;
            c.ExecFile(SecretFile("test-dkb.command"));
        }

        [Test, Explicit("")]
        public void Html1822direkt()
        {
            Collectors c = new Collectors();
            c.Payments = a.Payments;
            ScrapeForm.ShowBrowser = true;
            c.ExecFile(SecretFile("test-1822direkt.command"));
        }

        class Schema
        {
            [RowId]
            public long Id = 0;

            [Data]
            public string sql = null;
        }

        [Test]
        public void DbScheme()
        {
            Sidi.Persistence.Collection<Schema> schema = new Sidi.Persistence.Collection<Schema>(a.Payments.Connection, " sqlite_master");
            foreach (Schema i in schema)
            {
                i.DumpProperties(Console.Out);
            }
            schema.Close();
        }
    }
}

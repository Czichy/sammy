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

namespace Sammy.Test
{
    [TestFixture]
    public class AccountUt
    {
        [SetUp]
        public void Setup()
        {
        }

        [TearDown]
        public void TearDown()
        {
        }

        [Test]
        public void Create()
        {
            Account a = new Account(user, pass);
            if (a.Exists)
            {
                a.Delete();
            }
            a.Create();
        }

        string user = "test-name";
        string pass = "test-pass";

        [Test]
        public void ReadWrite()
        {
            string script = "hello, script";
            Account a = new Account(user, pass);
            if (a.Exists)
            {
                a.Delete();
            }
            a.Create();
            a.Settings.Script = script;
            a.Write();

            Account b = new Account(user, pass);
            b.Read();
            Assert.AreEqual(script, b.Settings.Script);
        }

        [Test]
        public void Hbci()
        {
            Account a = new Account(user, pass);
            if (a.Exists)
            {
                a.Delete();
            }
            a.Create();
            Collectors c = new Collectors();
            c.Payments = a.Payments;
            c.ExecFile(@"D:\doc\office\Finanzen\test-hbci.command");
        }

        [Test]
        public void Dkb()
        {
            Account a = new Account(user, pass);
            if (a.Exists)
            {
                a.Delete();
            }
            a.Create();
            Collectors c = new Collectors();
            c.Payments = a.Payments;
            c.ExecFile(@"D:\doc\office\Finanzen\test-dkb.command");
        }

        [Test]
        public void Html1822direkt()
        {
            Account a = new Account(user, pass);
            if (a.Exists) { a.Delete(); }
            a.Create();
            Collectors c = new Collectors();
            c.Payments = a.Payments;
            c.ExecFile(@"D:\doc\office\Finanzen\test-1822direkt.command");
        }

        class Schema
        {
            [RowId]
            public long Id;

            [Data]
            public string sql;
        }

        [Test]
        public void DbScheme()
        {
            Account a = new Account(user, pass);
            if (a.Exists) a.Delete();
            a.Create();
            Sidi.Persistence.Collection<Schema> schema = new Sidi.Persistence.Collection<Schema>(a.Payments.Connection, " sqlite_master");
            foreach (Schema i in schema)
            {
                i.DumpProperties(Console.Out);
            }
            schema.Close();
            a.Delete();
        }
    }
}

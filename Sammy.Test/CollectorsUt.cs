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
using Sidi.IO;

namespace Sidi.Sammy.Test
{
    [TestFixture]
    public class CollectorsUt : TestBase
    {
        [Test]
        public void TestGetCollector()
        {
            Collectors c = new Collectors();
            Type t = c.GetCollector("Dkb");
            Assert.AreEqual("Dkb", t.Name);
        }

        [Test]
        public void TestCollectorSample()
        {
            ICollector c = new Sidi.Sammy.CollectorSample.CollectorSample();
            string path = FileUtil.BinFile(@"test-data\test.sqlite");
            if (File.Exists(path)) File.Delete(path);
            Sidi.Persistence.Collection<Payment> pc = new Sidi.Persistence.Collection<Payment>(path);
            IList<Payment> p = c.GetPayments();
            foreach (Payment i in p)
            {
                pc.Add(i);
            }

            foreach (Payment i in p)
            {
                Assert.AreEqual(true, pc.Contains(i));
            }
        }
    }
}

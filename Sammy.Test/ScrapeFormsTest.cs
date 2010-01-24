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
using Sidi.CommandLine;
using System.Windows.Forms;
using System.Threading;

namespace Sidi.Sammy.Test
{
    [TestFixture]
    public class ScrapeFormsTest : TestBase
    {
        [Test]
        public void ScrapeFormTest1()
        {
            ScrapeForm f = new ScrapeForm();
            f.Run();
        }

        [Test, ExpectedException(ExpectedMessage="test")]
        public void ExceptionHandling()
        {
            ScrapeForm f = new ScrapeForm();
            string exceptionMessage = "test";
            f.BodyHandler = delegate(HtmlElement e)
            {
                throw new Exception(exceptionMessage);
            };
            f.Run();
        }

        [Test, ExpectedException(ExceptionType=typeof(TimeoutException))]
        public void Timeout()
        {
            ScrapeForm f = new ScrapeForm();
            f.Timeout = new TimeSpan(0,0,1);
            f.BodyHandler = delegate(HtmlElement e)
            {
                Thread.Sleep((int) f.Timeout.TotalMilliseconds + 500);
            };
            f.Run();
        }
    }
}

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

namespace Sidi.Sammy.Test
{
    [TestFixture]
    public class StatementViewTest : TestBase
    {
        [Test, Explicit("interactive")]
        public void View()
        {
            Account a = new Account(user, pass);
            StatementView v = new StatementView(a);
            v.ShowDialog();
        }

    }
}

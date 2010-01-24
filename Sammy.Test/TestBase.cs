using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net.Appender;
using log4net.Layout;
using Sidi.Util;

namespace Sidi.Sammy.Test
{
    public class TestBase
    {
        static TestBase()
        {
            DebugAppender a = new DebugAppender();
            a.Layout = new SimpleLayout();
            log4net.Config.BasicConfigurator.Configure(a);
        }

        public string user = "test-name";
        public string pass = "test-pass";

        public string SecretFile(string name)
        {
            return @"D:\office\Finanzen".CatDir(name);
        }
    }
}

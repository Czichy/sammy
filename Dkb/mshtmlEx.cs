using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using mshtml;
using System.Web;
using Sidi.Util;
using System.IO;

namespace Sidi.Sammy.Dkb
{
    public static class mshtmlEx
    {
        public static string GetAttribute(this IHTMLElement element, string name)
        {
            var a = ((IHTMLElement4)element).getAttributeNode(name);
            if (a == null)
            {
                return String.Empty;
            }

            var r = a.nodeValue;
            if (r == null)
            {
                return String.Empty;
            }
            return r;
        }

        public static IEnumerable<IHTMLElement> GetChildren(this IHTMLElement e)
        {
            return ((IHTMLElementCollection)e.children)
                .Cast<IHTMLElement>();
        }

        public static IEnumerable<IHTMLElement> GetAllChildren(this IHTMLElement e)
        {
            return ((IHTMLElementCollection)e.all)
                .Cast<IHTMLElement>();
        }

        public static IEnumerable<IHTMLElement> GetChildren(this IHTMLElement e, string tagName)
        {
            return e.GetChildren()
                .Where(i => i.tagName.Equals(tagName, StringComparison.InvariantCultureIgnoreCase));
        }

        public static string GetInnerText(this IHTMLElement e)
        {
            var r = e.innerText;
            if (r == null)
            {
                r = String.Empty;
            }
            return HttpUtility.HtmlDecode(r);
        }

        public static string[][] ParseTable(this IHTMLElement table)
        {
            return table.GetChildren("TBODY")
                .SelectMany(tbody => tbody.GetChildren("TR")
                    .Select(tr =>
                    {
                        return tr.GetChildren("TD")
                            .Select(td => td.GetInnerText())
                            .ToArray();
                    }))
                    .Skip(1)
                    .ToArray();
        }

        public static string Dump(this string[][] table)
        {
            var w = new StringWriter();
            int i = 0;
            foreach (var r in table)
            {
                w.WriteLine("{0}: {1}", i, r.Join("|"));
                ++i;
            }
            return w.ToString();
        }
    }
}

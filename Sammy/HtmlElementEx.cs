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
using System.IO;

namespace Sidi.Sammy
{
    public static class HtmlElementEx
    {
        public static void Dump(this HtmlElement e, TextWriter o, int indent)
        {
            for (int i = 0; i < indent; ++i)
            {
                o.Write(" ");
            }
            o.WriteLine(String.Format("{0} {1} class={2}", e.TagName, e.Id, e.GetAttribute("ClassName")));
            ++indent;
            foreach (HtmlElement i in e.Children)
            {
                i.Dump(o, indent);
            }
        }

        public delegate bool FilterFunc(HtmlElement e);

        public static IEnumerable<HtmlElement> FindAll(this HtmlElement e, FilterFunc f)
        {
            if (f(e))
            {
                yield return e;
            }

            foreach (HtmlElement i in e.Children)
            {
                foreach (HtmlElement fc in i.FindAll(f))
                {
                    yield return fc;
                }
            }
        }

        public static HtmlElement Find(this HtmlElement e, FilterFunc f)
        {
            if (f(e))
            {
                return e;
            }

            foreach (HtmlElement i in e.Children)
            {
                HtmlElement found = i.Find(f);
                if (found != null)
                {
                    return found;
                }
            }
            return null;
        }

        public static void SetInput(this HtmlElement e, string key, string value)
        {
            foreach (HtmlElement userInput in e.GetElementsByTagName("INPUT"))
            {
                string id = userInput.Id;
                if (String.IsNullOrEmpty(id)) id = String.Empty;
                if (id.Contains(key))
                {
                    userInput.SetAttribute("value", value);
                    return;
                }
            }
            throw new Exception(String.Format("input {0} not found", key));
        }

        public static void Check(this HtmlElement e, string key)
        {
            foreach (HtmlElement userInput in e.GetElementsByTagName("INPUT"))
            {
                string id = userInput.Id;
                if (String.IsNullOrEmpty(id)) id = String.Empty;
                if (id.Contains(key))
                {
                    userInput.SetAttribute("checked", "checked");
                }
            }
        }

        public static void ClickButton(this HtmlElement e, string idSubstr)
        {
            foreach (HtmlElement userInput in e.GetElementsByTagName("BUTTON"))
            {
                string id = userInput.Id;
                if (String.IsNullOrEmpty(id)) id = String.Empty;
                if (id.Contains(idSubstr))
                {
                    userInput.InvokeMember("click");
                }
            }
        }

        public static void ClickElement(this HtmlElement e)
        {
            e.InvokeMember("click");
        }

        public static bool AttributeContains(this HtmlElement e, string attr, string substr)
        {
            string v = e.GetAttribute(attr);
            if (v == null)
            {
                return false;
            }
            return v.Contains(substr);
        }
        
        public static bool AttributeIs(this HtmlElement e, string attr, string str)
        {
            string v = e.GetAttribute(attr);
            if (v == null)
            {
                return false;
            }
            return v == str;
        }

        public static string GetSelectText(this HtmlElement select)
        {
            int index = Int32.Parse(select.GetAttribute("value"));
            return select.Children[index].InnerText;
        }

    }
}

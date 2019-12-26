using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shiro.Guts
{
    internal static class MiscHelper
    {
        internal static string Interpolate(Interpreter shiro, string s)
        {
            var retVal = new StringBuilder();

            var code = "";
            var isCode = false;
            var indent = 0;
            for(var i=0; i<s.Length; i++)
            {
                var c = s[i];
                switch(c)
                {
                    case '{':
                        if (!isCode)
                            isCode = true;
                        else {
                            code += c;
                            indent += 1;
                        }
                        break;

                    case '}':
                        if (!isCode)
                            Interpreter.Error("Found end-interpolate brace without matching start.  Use %} if you want to use a curly brace in an interpolated string.");

                        if (indent > 0)
                        {
                            indent -= 1;
                            code += c;
                        }
                        else
                        {
                            isCode = false;
                            var val = shiro.Eval(code).ToString();
                            retVal.Append(val);
                            code = "";
                        }
                        break;

                    case '%':
                        if (isCode)
                            code += c;
                        else
                        {
                            try
                            {
                                if (s[i + 1] == '}' || s[i + 1] == '{')
                                    retVal.Append(s[++i]);
                            }
                            catch (Exception)
                            {
                                retVal.Append("%");
                            }
                        }
                        break;

                    default:
                        if (!isCode)
                            retVal.Append(c);
                        else
                            code += c;
                        break;
                }
            }

            return retVal.ToString();
        }

        internal static Token MixIn(Interpreter shiro, Token toke, string[] mixins)
        {
            var retVal = toke.Clone();

            foreach(var name in mixins)
            {
                var mixin = shiro.Symbols.GetImplementer(name);
                foreach(var prop in mixin.Children)
                {
                    var pn = prop.Name;
                    if (!retVal.Children.HasProperty(pn))
                        retVal.Children.AddProperty(pn, mixin.Children.GetProperty(pn).Clone());
                }
            }

            return retVal;
        }
    }
}

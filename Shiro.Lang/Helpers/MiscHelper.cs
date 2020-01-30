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
                            var val = shiro.Eval(code, false).ToString();
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
                    if (pn == name)
                        continue;       //Skip constructors

                    if (!retVal.Children.HasProperty(shiro, pn))
                        retVal.Children.AddProperty(shiro, pn, mixin.Children.GetProperty(shiro, pn).Clone());
                }
            }

            return retVal;
        }

        internal static Token DoesItQuack(Interpreter shiro, Token checkThis, Token mixin, string implName)
        {
            if (!checkThis.IsParent)
                Interpreter.Error("Can't check to see if '" + checkThis.ToString() + "' implements anything because it's not a list");

            foreach (var prop in mixin.Children)
            {
                var pn = prop.Name;
                if (pn == implName)
                    continue;           //Ignore CTOR

                if (!checkThis.Children.HasProperty(shiro, pn))
                    return Token.False;

                var node = mixin.Children.GetProperty(shiro, pn);
                var ctNode = checkThis.Children.GetProperty(shiro, pn);
                if (node.IsFunction) {
                    if(!ctNode.IsFunction)
                        return Token.False;

                    if (node.Params.Count != ctNode.Params.Count)
                        return Token.False;

                    for(var i=0; i<node.Params.Count; i++)
                    {
                        var np = node.Params[i];
                        var ctp = ctNode.Params[i];

                        if (!string.IsNullOrEmpty(np.Predicate) && np.Predicate != ctp.Predicate)
                            return Token.False;
                    }
                }
            }

            return Token.True;
        }

        internal static Token EvaluateObject(Interpreter shiro, Token val, bool atomic = false)
        {
            if (!val.IsObject)
                return val;

            for (var i = 0; i < val.Children.Count; i++)
            {
                var pn = val.Children[i].Name;
                if (val.Children[i].IsObject)
                    val.Children[i] = EvaluateObject(shiro, val.Children[i], atomic).SetName(pn);
                else if (!val.Children[i].IsFunction)
                    val.Children[i] = val.Children[i].Eval(shiro, atomic, true).SetName(pn);
            }

            return val;
        }
    }
}

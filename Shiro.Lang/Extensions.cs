using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Shiro
{
    public static class Extensions
    {
        public static bool RunningAsCompiledCode = false;       //Yeah this should be somewhere else, w/e IDC

        public static bool LookAhead(this string s, int pos, string lookFor)
        {
            for (var i=pos+1; i < s.Length; i++)
            {
                var c = s[i];
                if (c == ' ' || c == '\t' || c == '\r' || c == '\n')
                    continue;

                if (c == lookFor[0])
                {
                    if (s.Length >= pos + i + lookFor.Length)
                        foreach (var c2 in lookFor)
                            if (s[i++] != c2)
                                return false;

                    return true;
                }
                else
                    return false;
            }
            return false;
        }
        public static string ToFlatString(this List<Token> tokes)
        {
            StringBuilder ret = new StringBuilder("(");

            foreach (var t in tokes)
            {
                if (t.IsParent)
                    ret.Append($"{t.Children.ToFlatString()} ");
                else
                    ret.Append($"{t.ToString()} ");
            }

            return ret.ToString().Trim() + ")";
        }
        public static string ToJSONArray(this List<Token> tokes, Interpreter shiro)
        {
            StringBuilder ret = new StringBuilder("[");

            foreach (var t in tokes)
                ret.Append($"\"{t.Eval(shiro).ToString()}\", ");

            return ret.ToString().Trim().TrimEnd(',') + "]";
        }
        public static string ToJSON(this Token toke, Interpreter shiro, bool evaluate = false)
        {
            if (!toke.IsParent)
                return $"\"{toke.ToString()}\"";
            if (string.IsNullOrEmpty(toke.Children[0].Name))
                return toke.Children.ToJSONArray(shiro);
                    
            StringBuilder ret = new StringBuilder("{");

            foreach (var t in toke.Children)
                if(evaluate)
                    ret.Append($"\"{t.Name}\": {t.Eval(shiro).ToJSON(shiro, true)}, ");
                else
                    ret.Append($"\"{t.Name}\": {t.ToJSON(shiro, false)}, ");

            return ret.ToString().Trim().TrimEnd(',') + "}";
        }

        public static bool HasProperty(this List<Token> tokes, Interpreter shiro, string name)
        {
            if (tokes.Any(t => t.Name != null && t.Name.ToLower() == name.ToLower()))
                return true;

            if (shiro.Symbols.CurrentEnclosure != null)
                return shiro.Symbols.CurrentEnclosure.Children.HasProperty(shiro, name);

            return false;
        }

        public static List<Token> Quote(this List<Token> tokes)
        {
            if (tokes.Count == 0)
                return tokes;

            var retVal = new List<Token>();
            retVal.AddRange(tokes);
            retVal.RemoveAt(0);

            return retVal;
        }

        public static Token GetProperty(this List<Token> tokes, Interpreter shiro, string name)
        {
            if (!tokes.HasProperty(shiro, name))
                return Token.Nil;

            var toke = tokes.FirstOrDefault(t => t.Name != null && t.Name.ToLower() == name.ToLower());
            if (toke != null)
                return toke;

            if (shiro.Symbols.CurrentEnclosure != null)
                return shiro.Symbols.CurrentEnclosure.Children.GetProperty(shiro, name);

            return Token.Nil;
        }
        public static bool SetProperty(this List<Token> tokes, Interpreter shiro, string name, Token val)
        {
            if (!tokes.HasProperty(shiro, name))
                return false;

            var toke = tokes.FirstOrDefault(t => t.Name != null && t.Name.ToLower() == name.ToLower());
            if (toke == null)
                toke = shiro.Symbols.CurrentEnclosure.Children.First(t => t.Name != null && t.Name.ToLower() == name.ToLower());

            if (val.IsParent)
            {
                toke.Children = val.Children;
                toke.Params = val.Params;
            }
            else
                toke.Toke = val.Toke;
            return true;
        }
        public static void AddProperty(this List<Token> tokes, Interpreter shiro, string name, Token val)
        {
            if (tokes.HasProperty(shiro, name))
                throw new Exception("Token List AddProperty called when the property already exists.  Property was: " + name);

           tokes.Add(val.Clone(name));
        }

        public static bool ValidateParamCount(this List<Token> tokes, int expected, bool orGreaterThan = false)
        {
            if(!orGreaterThan)
                return tokes.Count == expected + 1;
            
            return tokes.Count >= expected + 1;
        }

        internal static object TypeCoerce(this string s)
        {
            long l;
            decimal d;

            if (long.TryParse(s, out l))
                return l;

            if (decimal.TryParse(s, out d))
                return d;

            return s;
        }
        
        public static TcpState GetState(this TcpClient tcpClient)
        {
            var foo = IPGlobalProperties.GetIPGlobalProperties()
                .GetActiveTcpConnections()
                .SingleOrDefault(x => x.RemoteEndPoint.Equals(tcpClient.Client.RemoteEndPoint));
            return foo != null ? foo.State : TcpState.Unknown;
        }
    }
}

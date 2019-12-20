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

        public static string ToFlatString(this List<Token> tokes)
        {
            StringBuilder ret = new StringBuilder("");

            foreach (var t in tokes)
            {
                if (t.IsParent)
                    ret.Append($"({t.Children.ToFlatString()}) ");
                else
                    ret.Append($"{t.ToString()} ");
            }

            return ret.ToString().Trim();
        }
        public static string ToJSONArray(this List<Token> tokes, Interpreter merp)
        {
            StringBuilder ret = new StringBuilder("[");

            foreach (var t in tokes)
                ret.Append($"\"{t.Eval(merp).ToString()}\", ");

            return ret.ToString().Trim().TrimEnd(',') + "]";
        }
        public static string ToJSON(this Token toke, Interpreter merp, bool evaluate = false)
        {
            if (!toke.IsParent)
                return $"\"{toke.ToString()}\"";
            if (string.IsNullOrEmpty(toke.Children[0].Name))
                return toke.Children.ToJSONArray(merp);
                    
            StringBuilder ret = new StringBuilder("{");

            foreach (var t in toke.Children)
                if(evaluate)
                    ret.Append($"\"{t.Name}\": {t.Eval(merp).ToJSON(merp, true)}, ");
                else
                    ret.Append($"\"{t.Name}\": {t.ToJSON(merp, false)}, ");

            return ret.ToString().Trim().TrimEnd(',') + "}";
        }

        public static bool HasProperty(this List<Token> tokes, string name)
        {
            return tokes.Any(t => t.Name != null && t.Name.ToLower() == name.ToLower());
        }

        public static List<Token> Quote(this List<Token> tokes)
        {
            var retVal = new List<Token>();
            retVal.AddRange(tokes);
            retVal.RemoveAt(0);

            return retVal;
        }

        public static Token GetProperty(this List<Token> tokes, string name)
        {
            if (!tokes.HasProperty(name))
                return Token.Nil;

            return tokes.FirstOrDefault(t => t.Name != null && t.Name.ToLower() == name.ToLower());
        }
        public static bool SetProperty(this List<Token> tokes, string name, Token val)
        {
            if (!tokes.HasProperty(name))
                return false;

            if(val.IsParent)
                tokes.First(t => t.Name != null && t.Name.ToLower() == name.ToLower()).Children = val.Children;
            else
                tokes.First(t => t.Name != null && t.Name.ToLower() == name.ToLower()).Toke = val.Toke;
            return true;
        }
        public static void AddProperty(this List<Token> tokes, string name, Token val)
        {
            if (tokes.HasProperty(name))
                throw new Exception("Token List AddProperty called when the property already exists.  Property was: " + name);

            if (val.IsParent)
                tokes.Add(new Token(name, val.Children));
            else
                tokes.Add(new Token(name, val.Toke));
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

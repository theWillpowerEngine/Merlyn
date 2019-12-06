using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Merlyn
{
    public class Token
    {
        #region Core Tokens

        public static Token Nil => new Token()
        {
            Toke = null,
            Children = null
        };

        public static Token True => new Token()
        {
            Toke = "true",
            Children = null
        };

        public static Token False => new Token()
        {
            Toke = "false",
            Children = null
        };
       #endregion

        #region CTORs

        public Token(string s)
        {
            if (s == null)
            {
                Toke = null;
                Children = null;
                return;
            }

            Toke = s.TypeCoerce();
        }

        public Token(string name, List<Token> val)
        {
            Name = name;
            Children = val;
        }
        public Token(string name, object val)
        {
            Name = name;
            Toke = val;
        }

        public Token(List<Token> c)
        {
            Children = c;
        }
        public Token(params Token[] cs)
        {
            Children = new List<Token>(cs);
        }
        #endregion

        public string Name;
        public object Toke;
        public List<Token> Children;
        public Guid LetTableId = Guid.Empty;
        public List<string> Params = null;

        public bool IsParent => Children != null;
        public bool IsNil => (Children == null && Toke == null);
        public bool IsNumeric => Toke is long || Toke is decimal;
        public bool IsDecimal => Toke is decimal;
        public bool IsFunction => Params != null;

        public new string ToString()
        {
            if (Toke != null)
                return Toke.ToString();
            if (IsParent)
                return Children.ToFlatString();
            
            return "nil";
        }

        internal Token Eval(Merpreter merp)
        {
            if (IsParent)
                return merp.Eval(Children);
               
            return this;
        }
    }
}

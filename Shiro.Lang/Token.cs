using Shiro.Guts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shiro
{
    public class Token
    {
        #region Core Tokens

        public static Token Nil => new Token()
        {
            Toke = null,
            Children = null
        };

        public static Token EmptyList => new Token()
        {
            Toke = null,
            Children = new List<Token>()
        };

        public static Token NamedEmptyList(string name)
        {
            return new Token()
            {
                Toke = null,
                Name = name,
                Children = new List<Token>()
            };
        }

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

        public static Token Error(Interpreter shiro, string msg)
        {
            var ret = new Token();
            ret.Toke = null;
            ret.Children = new List<Token>();

            ret.Children.AddProperty(shiro, "error", True);
            ret.Children.AddProperty(shiro, "message", new Token(msg));

            return ret;
        }

        internal Token SetName(string key)
        {
            Name = key;
            return this;
        }
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
        public Token()
        {
            Toke = null;
            Children = null;
        }
        #endregion

        public string Name;
        public object Toke;
        public List<Token> Children;
        public Guid LetTableId = Guid.Empty;
        public List<Param> Params = null;

        public bool IsBeingAwaited = false;

        public Token Enclosure = null;

        public bool IsParent => Children != null;
        public bool IsNil => (Children == null && (Toke == null || (Toke is Token && (Toke as Token).IsNil)));
        public bool IsNumeric => Toke is long || Toke is decimal;
        public bool IsDecimal => Toke is decimal;
        public bool IsFunction => Params != null;
        public bool IsObject => IsParent && Children.Count > 0 && !string.IsNullOrEmpty(Children[0].Name);
        public bool IsQuotedList = false;
        public bool IsLambdaWhichCanBeCalledWithParameters => IsFunction && (Params.Count == 0 || (Params.Count - Params.Count(p => p.DefaultValue != null) == 0));

        public bool IsTrue {
            get
            {
                if (this == True) return true;
                if(Toke == True.Toke) return true;
                if(IsNumeric && (long)Toke != 0) return true;
                return false;
            }
        }

        public Token Clone(string name = null)
        {
            if (IsParent)
                return new Token(name ?? Name, Children.ToArray().ToList())
                {
                    Params = Params,     
                    Enclosure = Enclosure?.Clone()
                    //Don't clone IsBeingAwaited because only one of them is getting delivered
                };
            else
                return new Token(Toke?.ToString()) {
                    Name = name,
                    Enclosure = Enclosure?.Clone()
                };
        }

        public new string ToString()
        {
            if (IsNil)
                return "nil";
            if (Toke != null)
                return Toke.ToString();
            if (IsParent)
                return Children.ToFlatString();
            
            return "nil";
        }

        public Token Eval(Interpreter shiro, bool atomic = false)
        {
            if (IsParent)
            {
                var res = shiro.Eval(Children, atomic);
                return res;
            }                
               
            return this;
        }

        public Token EvalLambda(Token thisToke, Interpreter shiro, params Token[] args)
        {
            if (!IsFunction) { 
                Interpreter.Error("Attempted to evaluate something as a lambda that's not a lambda.  It was: " + ToString());
                return Nil;
            }

            Guid letId = Guid.NewGuid();
            int i = 0;
            int canSkip = Params.Count(p => p.HasDefault);
            int mustSkip = Params.Count - args.Length;

            if (mustSkip < 0)
                Interpreter.Error($"Too many parameters passed to lambda '{ToString()}', expected at most {Params.Count}, found {args.Length} instead");
            if (mustSkip > canSkip)
                Interpreter.Error($"Not enough parameters passed to lambda '{ToString()}', expected at least {Params.Count - canSkip}, found {args.Length} instead");

            foreach (var pn in Params)
            {
                if (pn.HasDefault && mustSkip > 0)
                {
                    pn.LetOrError(shiro, pn.DefaultValue.Clone(), letId);
                    mustSkip -= 1;
                }
                else
                    pn.LetOrError(shiro, args[i++].Eval(shiro), letId);
            }

            if (mustSkip != 0)
                Interpreter.Error($"Couldn't match parameters passed to lambda '{ToString()}' with the defaults provided.  This usually means you passed too few parameters, {mustSkip} more are needed");

            if (thisToke != null)
                shiro.Symbols.Let("this", thisToke, letId);

            shiro.Symbols.PushTardEnclosure(thisToke?.Enclosure ?? Enclosure);

            try
            {
                var retVal = Eval(shiro);
                return retVal;
            }
            finally
            {
                shiro.Symbols.ClearLetId(letId);
                Enclosure = shiro.Symbols.PopTardEnclosure();
            }
        }
    }
}

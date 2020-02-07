using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Shiro.Nimue;

namespace Shiro.Guts
{
    public class Symbols
    {
        private Interpreter _shiro;

        private readonly Dictionary<string, Token> SymbolTable = new Dictionary<string, Token>();
        private readonly Dictionary<string, Token> LetTable = new Dictionary<string, Token>();
        private readonly Dictionary<string, Func<Token>> AutoSymbols = new Dictionary<string, Func<Token>>();

        private static readonly List<Token> CheckClosureScope = null;

        private Stack<Token> EnclosureStack = new Stack<Token>(); 

        public Token CurrentEnclosure => EnclosureStack.Count > 0 ? EnclosureStack.Peek() : null;

        internal class LetOverride
        {
            internal string Name, LetId;
            internal Token Value;
            internal string LetIdHiddenBy;
        }

        private readonly Stack<LetOverride> LetOverrideStack = new Stack<LetOverride>();

        private readonly Dictionary<string, Token> Implementers = new Dictionary<string, Token>();

        private readonly Dictionary<string, Token> FunctionTable = new Dictionary<string, Token>();
        private readonly Dictionary<string, Func<Interpreter, Token, Token>> AutoFunctions = new Dictionary<string, Func<Interpreter, Token, Token>>();
        private readonly Dictionary<string, string> AutoFunctionHelpTips = new Dictionary<string, string>();

        public bool IsAwaiting => SymbolTable.Values.Any(t => t.IsBeingAwaited);

        internal static class AutoVars
        {
            internal static string ConnectionId = "id";
            internal static string TelnetInput = "input";
            internal static string HttpRequest = "request";
        }

        internal void CloneFrom(Symbols s)
        {
            foreach (var key in s.SymbolTable.Keys)
                SymbolTable.Add(key, s.SymbolTable[key].Clone());
            foreach (var key in s.LetTable.Keys)
            {
                if (SymbolTable.ContainsKey(key))
                    LetTable.Add(key, s.LetTable[key].Clone());
                else
                    SymbolTable.Add(key, s.LetTable[key].Clone());      //lets go in the global scope of the resulting symbol table for ghetto closure scope
            }
            foreach (var key in s.AutoSymbols.Keys)
                if(!AutoSymbols.ContainsKey(key))
                    AutoSymbols.Add(key, s.AutoSymbols[key]);

            foreach (var key in s.Implementers.Keys)
                Implementers.Add(key, s.Implementers[key].Clone());

            foreach (var key in s.FunctionTable.Keys)
                FunctionTable.Add(key, s.FunctionTable[key].Clone());
            foreach (var key in s.AutoFunctions.Keys)
                if (!AutoFunctions.ContainsKey(key))
                    AutoFunctions.Add(key, s.AutoFunctions[key]);
        }

        public Token Get(string name)
        {
            if (AutoSymbols.ContainsKey(name))
                return AutoSymbols[name]();
            if (LetTable.ContainsKey(name))
                return LetTable[name];
            if (CheckClosureScope.HasProperty(_shiro, name))
                return CheckClosureScope.GetProperty(_shiro, name);
            if (SymbolTable.ContainsKey(name))
            {
                var awaiting = SymbolTable[name].IsBeingAwaited;
                while(awaiting)
                {
                    _shiro.DispatchPublications();
                    lock(SymbolTable[name])
                    {
                        awaiting = SymbolTable[name].IsBeingAwaited;
                    }
                }

                return SymbolTable[name];
            }

            Interpreter.Error("Attempt to get value of non-existant variable: " + name);
            return Token.Nil;
        }

        public bool CanGet(string name)
        {
            if (AutoSymbols.ContainsKey(name))
                return true;
            if (LetTable.ContainsKey(name))
                return true;
            if (CheckClosureScope.HasProperty(_shiro, name))
                return true;
            if (SymbolTable.ContainsKey(name))
                return true;
            return false;
        }

        public Token GetGlobal(string name)
        {
            if (SymbolTable.ContainsKey(name))
            {
                var awaiting = SymbolTable[name].IsBeingAwaited;
                while (awaiting)
                {
                    _shiro.DispatchPublications();
                    lock (SymbolTable[name])
                    {
                        awaiting = SymbolTable[name].IsBeingAwaited;
                    }
                }

                return SymbolTable[name];
            }

            Interpreter.Error("Attempt to get value of non-existant global variable: " + name);
            return Token.Nil;
        }

        public bool CanGetGlobal(string name)
        {
            if (SymbolTable.ContainsKey(name))
                return true;
            return false;
        }

        public void Set(string name, Token val)
        {
            if (CheckClosureScope.HasProperty(_shiro, name))
                CheckClosureScope.SetProperty(_shiro, name, val);
            else if (!SymbolTable.ContainsKey(name))
                SymbolTable.Add(name, val);
            else
                SymbolTable[name] = val;
        }

        public void UnSet(string name)
        {
            if (SymbolTable.ContainsKey(name))
                SymbolTable.Remove(name);
        }

        public void Let(string name, Token val, Guid letId)
        {
            val = val.Clone();
            val.LetTableId = letId;

            lock (LetTable)
            {
                if (!LetTable.ContainsKey(name))
                    LetTable.Add(name, val);
                else
                {
                    LetOverrideStack.Push(new LetOverride()
                    {
                        Name = name,
                        Value = LetTable[name],
                        LetId = LetTable[name].LetTableId.ToString(),
                        LetIdHiddenBy = letId.ToString()
                    });
                    LetTable[name] = val;
                }
            }
        }

        internal void AddAutoVar(string name, Func<Token> val)
        {
            if (!AutoSymbols.ContainsKey(name))
                AutoSymbols.Add(name, val);
            else
                AutoSymbols[name] = val;
                
        }

        public void ReLet(string name, Token val)
        {
            if (!LetTable.ContainsKey(name))
                Interpreter.Error("Can't relet " + name + ", no such variable exists in the current let-scope.");

            val = val.Clone();
            val.LetTableId = LetTable[name].LetTableId;
            lock (LetTable)
                LetTable[name] = val;
        }

        public void ClearLetId(Guid letId)
        {
            lock (LetTable)
            {
                var removeThese = LetTable.Keys.Where(k => LetTable[k].LetTableId == letId).ToArray().ToList();
                var replaceThese = new List<LetOverride>();

                while (LetOverrideStack.Count > 0 && LetOverrideStack.Peek().LetIdHiddenBy == letId.ToString())
                    replaceThese.Add(LetOverrideStack.Pop());

                foreach (var key in removeThese)
                    LetTable.Remove(key);

                foreach (var ls in replaceThese)
                    if (LetTable.ContainsKey(ls.Name))
                        LetTable[ls.Name] = ls.Value;
                    else
                        LetTable.Add(ls.Name, ls.Value);
            }
        }

        public Token UnEnclose()
        {
            return EnclosureStack.Pop();
        }

        public void Enclose(Token tardEnclosure)
        {
            EnclosureStack.Push(tardEnclosure);
        }

        public Token GetImplementer(string name)
        {
            if (Implementers.ContainsKey(name))
                return Implementers[name];

            Interpreter.Error("Attempt to get non-existant implementer: " + name);
            return Token.Nil;
        }

        public bool CanGetImplementer(string name)
        {
            if (Implementers.ContainsKey(name))
                return true;
            return false;
        }

        public void SetImplementer(string name, Token val)
        {
            if (!Implementers.ContainsKey(name))
                Implementers.Add(name, val);
            else
                Implementers[name] = val;
        }

        public bool FuncExists(string name)
        {
            if (name == null)
                return false;
            if (FunctionTable.ContainsKey(name))
                return true;
            if (AutoFunctions.ContainsKey(name))
                return true;
            return false;
        }

        public void AddFunc(string name, Token val)
        {
            if(!val.IsFunction)
                Interpreter.Error($"Token declared as function '{name}' is not a function token");
            if (!FunctionTable.ContainsKey(name))
                FunctionTable.Add(name, val);
            else
                FunctionTable[name] = val;
        }

        public void AddAutoFunc(string name, Func<Interpreter, Token, Token> val, string helpTip)
        {
            if (!AutoFunctions.ContainsKey(name))
            {
                AutoFunctions.Add(name, val);
                AutoFunctionHelpTips.Add(name, helpTip ?? ";no help tip specified.  Bad Library Writer.  Bad!");
            }
            else
            {
                AutoFunctions[name] = val;
                AutoFunctionHelpTips[name] = helpTip ?? ";no help tip specified.  Bad Library Writer.  Bad!";
            }
        }

        public Token CallFunc(string name, Interpreter shiro, params Token[] args)
        {
            if (!FuncExists(name))
            {
                Interpreter.Error("Attempt to call undefined function: " + name);
                return Token.Nil;
            }

            if (AutoFunctions.ContainsKey(name))
            {
                var res = AutoFunctions[name](shiro, new Token(args));
                return res;
            }
            else
            {
                Guid letId = Guid.NewGuid();
                var func = FunctionTable[name];

                int i = 0;
                int canSkip = func.Params.Count(p => p.HasDefault);
                int mustSkip = func.Params.Count - args.Length;

                if(mustSkip < 0)
                    Interpreter.Error($"Too many parameters passed to function '{name}', expected at most {func.Params.Count}, found {args.Length} instead");
                if(mustSkip > canSkip)
                    Interpreter.Error($"Not enough parameters passed to function '{name}', expected at least {func.Params.Count - canSkip}, found {args.Length} instead");

                var leftToSkip = new int[func.Params.Count];
                var curSkip = 0;
                for(var j=func.Params.Count - 1; j > -1; j--)
                {
                    if (func.Params[j].HasDefault)
                        curSkip += 1;

                    leftToSkip[j] = curSkip;
                }

                if (curSkip < mustSkip)
                    Interpreter.Error("Skippable parameter mismatch -- this is an internal error that should be impossible");

                try
                {
                    int k = 0;
                    foreach (var pn in func.Params)
                    {
                        if (pn.HasDefault && mustSkip > 0 && mustSkip == leftToSkip[k])
                        {
                            pn.LetOrError(shiro, pn.DefaultValue.Clone(), letId);
                            mustSkip -= 1;
                        }
                        else
                            pn.LetOrError(shiro, args[i++].Eval(shiro), letId);

                        k += 1;
                    }

                    if (mustSkip != 0)
                        Interpreter.Error($"Couldn't match parameters passed to function '{name}' with the defaults provided.  This usually means you passed too few parameters, {mustSkip} more are needed");

                    var retVal = func.Eval(shiro);
                    ClearLetId(letId);
                    return retVal;
                }
                catch (Exception)
                {
                    ClearLetId(letId);
                    throw;
                }
            }
        }

        internal string GetFunctionsForAutoComplete()
        {
            StringBuilder sb = new StringBuilder();

            foreach (var f in FunctionTable.Keys)
                sb.Append(f + " ");

            foreach (var f in AutoFunctions.Keys)
                sb.Append(f + " ");

            foreach (var f in Implementers.Keys)
                sb.Append(f + "? ");

            return sb.ToString().Trim();
        }

        public Symbols(Interpreter shiro)
        {
            _shiro = shiro;

            AutoSymbols.Add("MerVer", () => new Token(Interpreter.Version));
            AutoSymbols.Add("IsServing", () => Server.Serving ? Token.True : Token.False);
        }

        public void BeginAwaiting(string s1)
        {
            var val = Token.Nil.Clone();
            val.IsBeingAwaited = true;
            Set(s1, val);
        }

        public void Deliver(string s1, Token res)
        {
            lock(SymbolTable[s1])
                SymbolTable[s1] = res;
        }

        public bool IsVarBeingAwaited(string s1)
        {
            lock (SymbolTable[s1])
                return SymbolTable[s1].IsBeingAwaited;
        }

        internal string GetHelpTipFor(string word)
        {
            if (AutoFunctionHelpTips.ContainsKey(word))
                return AutoFunctionHelpTips[word];

            if (FunctionTable.ContainsKey(word))
            {
                var ret = new StringBuilder("(");
                ret.Append(word);

                foreach (var parm in FunctionTable[word].Params)
                {
                    ret.Append(" <");
                    ret.Append(parm.Predicate == null ? parm.Name : (parm.Name + ":" + parm.Predicate));
                    ret.Append(">");
                }
                ret.Append(")");
                return ret.ToString();
            }

            return null;
        }

        internal Token GetLetScopeAsTardEnclosure()
        {
            Token retVal = new Token();
            retVal.Children = new List<Token>();

            foreach (var key in LetTable.Keys)
                retVal.Children.Add(LetTable[key].Clone(key));

            return retVal;
        }
    }
}

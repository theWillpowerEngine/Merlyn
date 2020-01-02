using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Shiro.Nimue;

namespace Shiro.Guts
{
    internal class Symbols
    {
        private Interpreter _shiro;

        private readonly Dictionary<string, Token> SymbolTable = new Dictionary<string, Token>();
        private readonly Dictionary<string, Token> LetTable = new Dictionary<string, Token>();
        private readonly Dictionary<string, Func<Token>> AutoSymbols = new Dictionary<string, Func<Token>>();

        private readonly Dictionary<string, Token> Implementers = new Dictionary<string, Token>();

        private readonly Dictionary<string, Token> FunctionTable = new Dictionary<string, Token>();
        private readonly Dictionary<string, Func<Interpreter, Token, Token>> AutoFunctions = new Dictionary<string, Func<Interpreter, Token, Token>>();

        internal bool IsAwaiting => SymbolTable.Values.Any(t => t.IsBeingAwaited);

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
                LetTable.Add(key, s.LetTable[key].Clone());
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
            if (SymbolTable.ContainsKey(name))
            {
                var awaiting = SymbolTable[name].IsBeingAwaited;
                while(awaiting)
                {
                    Thread.Sleep(10);
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
                    Thread.Sleep(10);
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
            if (!SymbolTable.ContainsKey(name))
                SymbolTable.Add(name, val);
            else
                SymbolTable[name] = val;
        }

        public void Let(string name, Token val, Guid letId)
        {
            val.LetTableId = letId;     //I *think* this is good?
            if (!LetTable.ContainsKey(name))
                LetTable.Add(name, val);
            else
                LetTable[name] = val;
        }

        public void ClearLetId(Guid letId)
        {
            var removeThese = LetTable.Keys.Where(k => LetTable[k].LetTableId == letId).ToArray().ToList();
            foreach (var key in removeThese)
                LetTable.Remove(key);
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

        public void AddAutoFunc(string name, Func<Interpreter, Token, Token> val)
        {
            if (!AutoFunctions.ContainsKey(name))
                AutoFunctions.Add(name, val);
            else
                AutoFunctions[name] = val;
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
                if (func.Params.Count != args.Length)
                    Interpreter.Error($"Incorrect number of params passed to function '{name}', expected {func.Params.Count}, found {args.Length} instead");

                int i = 0;

                foreach (var pn in func.Params)
                    pn.LetOrError(shiro, args[i++].Eval(shiro), letId);

                var retVal = func.Eval(shiro);
                ClearLetId(letId);
                return retVal;
            }
        }

        internal string GetFunctionsForAutoComplete()
        {
            StringBuilder sb = new StringBuilder();

            foreach (var f in FunctionTable.Keys)
                sb.Append(f + " ");

            foreach (var f in AutoFunctions.Keys)
                sb.Append(f + " ");

            return sb.ToString().Trim();
        }

        public Symbols(Interpreter shiro)
        {
            _shiro = shiro;

            AutoSymbols.Add("MerVer", () => new Token(Interpreter.Version));
            AutoSymbols.Add("IsServing", () => Server.Serving ? Token.True : Token.False);
        }

        internal void BeginAwaiting(string s1)
        {
            var val = Token.Nil.Clone();
            val.IsBeingAwaited = true;
            Set(s1, val);
        }

        internal void Deliver(string s1, Token res)
        {
            lock(SymbolTable[s1])
            {
                SymbolTable[s1] = res;
            }
        }
    }
}

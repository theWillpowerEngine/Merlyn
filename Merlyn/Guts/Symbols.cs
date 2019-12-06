using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Merlyn.Nimue;

namespace Merlyn.Guts
{
    internal class Symbols
    {
        private Merpreter _merp;

        private readonly Dictionary<string, Token> SymbolTable = new Dictionary<string, Token>();
        private readonly Dictionary<string, Token> LetTable = new Dictionary<string, Token>();
        private readonly Dictionary<string, Func<Token>> AutoSymbols = new Dictionary<string, Func<Token>>();

        private readonly Dictionary<string, Token> FunctionTable = new Dictionary<string, Token>();
        private readonly Dictionary<string, Func<Token, Token>> AutoFunctions = new Dictionary<string, Func<Token, Token>>();

        internal static class AutoVars
        {
            internal static string ConnectionId = "id";
            internal static string TelnetInput = "input";
            internal static string HttpRequest = "request";
        }

        public Token Get(string name)
        {
            if (AutoSymbols.ContainsKey(name))
                return AutoSymbols[name]();
            if (LetTable.ContainsKey(name))
                return LetTable[name];
            if (SymbolTable.ContainsKey(name))
                return SymbolTable[name];

            Merpreter.Error("Attempt to get value of non-existant variable: " + name);
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

        public bool FuncExists(string name)
        {
            if (FunctionTable.ContainsKey(name))
                return true;
            return false;
        }

        public void AddFunc(string name, Token val)
        {
            if(!val.IsFunction)
                Merpreter.Error($"Token declared as function '{name}' is not a function token");
            if (!FunctionTable.ContainsKey(name))
                FunctionTable.Add(name, val);
            else
                FunctionTable[name] = val;
        }

        public Token CallFunc(string name, Merpreter merp, params Token[] args)
        {
            if (!FuncExists(name))
            {
                Merpreter.Error("Attempt to call undefined function: " + name);
                return Token.Nil;
            }

            Guid letId = Guid.NewGuid();
            var func = FunctionTable[name];
            if(func.Params.Count != args.Length)
                Merpreter.Error($"Incorrect number of params passed to function '{name}', expected {func.Params.Count}, found {args.Length} instead");

            int i = 0;
            foreach (var pn in func.Params)
                Let(pn, args[i++], letId);

            var retVal = func.Eval(merp);
            ClearLetId(letId);
            return retVal;
        }

        public Symbols(Merpreter merp)
        {
            _merp = merp;

            AutoSymbols.Add("MerVer", () => new Token(Merpreter.Version));
            AutoSymbols.Add("IsServing", () => Server.Serving ? Token.True : Token.False);
        }
    }
}

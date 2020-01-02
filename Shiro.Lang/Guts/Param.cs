using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shiro.Guts
{
    public class Param
    {
        public string Name;
        public string Predicate;

        public Param(string name)
        {
            //Technically this should be a new keyword and a reader shortcut but I don't feel like it.
            if (name.Contains(":"))
            {
                var eles = name.Split(':');
                if (eles.Length != 2)
                    Interpreter.Error("Predicate-Parameters can only have a single predicate.  Problem name was: " + name);

                Name = eles[0];
                Predicate = eles[1];
            } else 
                Name = name;
        }
        public Param(string name, string pred)
        {
            Name = name;
            Predicate = pred;
        }

        public Token BuildPredicate(Token val)
        {
            if (string.IsNullOrEmpty(Predicate))
                Interpreter.Error("Attempt to build parameter-check predicate for parameter which doesn't have a predicate: " + Name);
            return new Token(new Token(Predicate), val);
        }

        internal void LetOrError(Interpreter shiro, Token token, Guid letId)
        {
            if(Predicate == null)
                shiro.Symbols.Let(Name, token, letId);
            else
            {
                var pred = BuildPredicate(token);
                if (MathHelper.Not(pred.Eval(shiro)).Toke == Token.False.Toke)
                    shiro.Symbols.Let(Name, token, letId);
                else
                    Interpreter.Error($"Value '{token.ToString()}' did not match param-predicate {Predicate} for paramater {Name}.");
            }
        }
    }
}

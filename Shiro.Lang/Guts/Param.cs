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
        public Token DefaultValue = null;
        public bool HasDefault => DefaultValue != null;

        public Param(Interpreter shiro, string name)
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

            //Ditto
            if (Name.Contains("="))
            {
                var elesAgain = Name.Split('=');
                if (elesAgain.Length != 2)
                    Interpreter.Error("Too many '=' in function definition for parameter " + name);

                Name = elesAgain[0];
                DefaultValue = shiro.Scan("'" + elesAgain[1] + "'").Children[0];

                if (Predicate != null)
                {
                    var pred = BuildPredicate(DefaultValue);
                    if (MathHelper.Not(pred.Eval(shiro)).Toke != Token.False.Toke)
                        Interpreter.Error($"Default value '{DefaultValue.ToString()}' for parameter {Name} does not match the required predicated {Predicate}.  The whole thing is a bust before we even begin (sad trombone).");
                }
            }
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

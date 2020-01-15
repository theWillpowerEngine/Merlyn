using Shiro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace std
{
    public class Library : Interpreter.ShiroPlugin
    {
        public override void RegisterAutoFunctions(Interpreter shiro)
        {
            shiro.RegisterAutoFunction("inherit", (i, toke) =>
            {
                if (toke.Children.Count != 1)
                    Interpreter.Error("inherit autofunction expects 1 param (name of thing to inherit from).  If you want multiple inheritance consider multiple inherits, or inherit combined with mixin");

                var name = toke.Children[0].Eval(i).ToString();
                var sym = GetSym(shiro);

                if(!sym.CanGet(name))
                    Interpreter.Error($"Can't get {name} to inherit from.  Did you mean to mixin or new an implementer?");

                var parent = sym.Get(name);
                if (!parent.IsObject)
                    Interpreter.Error($"Only objects can be inherited from, not {parent.ToString()}");

                return parent.Clone();
            }, "(inherit <name of object>)");

            shiro.RegisterAutoFunction("sleep", (i, toke) =>
            {
                if (toke.Children.Count != 1)
                    Interpreter.Error("sleep autofunction expects 1 param (amount of time (in ms) to sleep)");

                if(!toke.Children[0].IsNumeric)
                    Interpreter.Error("Parameter to sleep should be numeric, not " + toke.Children[0].ToString());

                var dur = (long)toke.Children[0].Toke;
                Thread.Sleep((int)dur);
                return Token.Nil;

            }, "(inherit <name of object>)");


            //            shiro.Eval(@"(do (defn assert (test s:str?) (pnb (if (! $test) (str 'FAIL: ' $s '%n') '')))
            //(defn assert-not (test s:str?) (pnb (if $test (str 'FAIL: ' $s '%n') '')))
            //(defn assert-eq (t1 t2 s:str?) (pnb (if (!= $t1 $t2) `FAIL ({$t1} != {$t2}): {$s}%n` '')))
            //(defn assert-throws (f:fn? s:str?) (pnb (if (!= 'dingleberry' (catch (f) 'dingleberry')) `Fail, didn't throw: {$s}` '')))
            //(defn assert-fails (f:fn? s:str?) (pnb (if (!= 'dingleberry' (try (f) 'dingleberry')) `Fail, didn't throw: {$s}` '')))
            //(defn assert-works (f:fn? s:str?) (pnb (if (= 'dingleberry' (try (f) 'dingleberry')) `Failed, didn't work: {$s}` ''))))", false);
        }
    }
}

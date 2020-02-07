using System;
using System.IO;
using Shiro;

namespace test
{
	public class Library : Interpreter.ShiroPlugin
    {
		public override void RegisterAutoFunctions(Interpreter shiro)
		{
            shiro.RegisterAutoFunction("assert-read-fail", (i, toke) =>
            {
                try
                {
                    var t = shiro.Eval(toke.Children[0].ToString());
                }
                catch (Exception)
                {
                    return Token.True;
                }
                Interpreter.Output(toke.Children[1].ToString() + Environment.NewLine);
                return Token.False;                
            }, "(assert-read-fail <string to try to read> <assert failure message>)");

            shiro.Eval(@"(do (defn assert (test s:str?) (pnb (if (! $test) (str 'FAIL: ' $s '%n') '')))
(defn assert-not (test s:str?) (pnb (if $test (str 'FAIL: ' $s '%n') '')))
(defn assert-eq (t1 t2 s:str?) (pnb (if (!= $t1 $t2) `FAIL ({$t1} != {$t2}): {$s}%n` '')))
(defn assert-throws (f:fn? s:str?) (pnb (if (!= 'dingleberry' (catch (f) 'dingleberry')) `FAIL, didn't throw: {$s}%n` '')))
(defn assert-fails (f:fn? s:str?) (pnb (if (!= 'dingleberry' (try (f) 'dingleberry')) `FAIL, didn't throw: {$s}%n` '')))
(defn assert-works (f:fn? s:str?) (pnb (if (= 'dingleberry' (try (f) 'dingleberry')) `FAIL, didn't work: {$s}%n` ''))))", false);
        }
	}
}

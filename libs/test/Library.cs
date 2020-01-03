using System;
using System.IO;
using Shiro.Interop;
using Shiro;

namespace test
{
	public class Library : ShiroPlugin
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
            });

            shiro.Eval(@"(do (defn assert (test s:str?) (pnb (if (! $test) (str 'FAIL: ' $s '%n') '')))
(defn assert-not (test s:str?) (pnb (if $test (str 'FAIL: ' $s '%n') '')))
(defn assert-eq (t1 t2 s:str?) (pnb (if (!= $t1 $t2) `FAIL ({$t1} != {$t2}): {$s}%n` '')))
(defn assert-throws (f:fn? s:str?) (pnb (if (!= 'dingleberry' (catch (f) 'dingleberry')) `Fail, didn't throw: {$s}` '')))
(defn assert-fails (f:fn? s:str?) (pnb (if (!= 'dingleberry' (try (f) 'dingleberry')) `Fail, didn't throw: {$s}` '')))
(defn assert-works (f:fn? s:str?) (pnb (if (= 'dingleberry' (try (f) 'dingleberry')) `Failed, didn't work: {$s}` ''))))");
        }
	}
}

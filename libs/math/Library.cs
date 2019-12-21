using System;
using Shiro.Interop;
using Shiro;

namespace math
{
	public class Library : ShiroPlugin
	{
		public override void RegisterAutoFunctions(Interpreter shiro)
		{
			shiro.RegisterAutoFunction("sqrt", (i, toke) =>
			{
				var t = toke.Children[0];
				if (!t.IsNumeric)
					Interpreter.Error("sqrt function requires a numeric value, not " + t.ToString());

				if (t.Toke is long)
					return new Token(Math.Sqrt((long)t.Toke).ToString());
				if (t.Toke is decimal)
					return new Token(Math.Sqrt((double)((decimal)t.Toke)).ToString());

				Interpreter.Error("Internal error in sqrt -- there must be a new numeric type that I didn't handle.");
				return Token.Nil;
			});
		}
	}
}

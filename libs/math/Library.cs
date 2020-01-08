using System;
using Shiro;

namespace math
{
	public class Library : Interpreter.ShiroPlugin
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
			}, "(sqrt <number>)");

            shiro.RegisterAutoFunction("abs", (i, toke) =>
            {
                var t = toke.Children[0];
                if (!t.IsNumeric)
                    Interpreter.Error("abs function requires a numeric value, not " + t.ToString());

                if (t.Toke is long)
                    return new Token(Math.Abs((long)t.Toke).ToString());
                if (t.Toke is decimal)
                    return new Token(Math.Abs((double)((decimal)t.Toke)).ToString());

                Interpreter.Error("Internal error in abs -- there must be a new numeric type that I didn't handle.");
                return Token.Nil;
            }, "(abs <number>)");

            shiro.RegisterAutoFunction("int", (i, toke) =>
            {
                var t = toke.Children[0];
                if (!t.IsNumeric)
                    Interpreter.Error("int function requires a numeric value, not " + t.ToString());

                if (t.Toke is long)
                    return t;
                if (t.Toke is decimal)
                    return new Token(((long)(decimal)t.Toke).ToString());

                Interpreter.Error("Internal error in int -- there must be a new numeric type that I didn't handle.");
                return Token.Nil;
            }, "(sqrt <decimal>)");

            shiro.RegisterAutoFunction("int?", (i, toke) =>
            {
                var t = toke.Children[0];

                if (t.Toke is long)
                    return Token.True;

                return Token.False;
            }, "(int? <value>)");

            shiro.RegisterAutoFunction("dec?", (i, toke) =>
            {
                var t = toke.Children[0];

                if (t.Toke is decimal)
                    return Token.True;

                return Token.False;
            }, "(dec? <value>)");

            shiro.RegisterAutoFunction("rand", (i, toke) =>
            {
                var t = toke.Children[0];
                if (!t.IsNumeric)
                    Interpreter.Error("rand function requires a numeric value, not " + t.ToString());

                int upper = (int)t.Toke;
                return new Token(new Random().Next(0, upper).ToString());
            }, "(rand <upper-bound-as-number>)");
        }
	}
}

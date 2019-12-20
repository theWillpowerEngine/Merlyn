using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shiro.Guts
{
    internal static class MathHelper
    {
        internal static Token Add(params Token[] tokes)
        {
            Token retVal = new Token();
            bool isDec = false;
            if (tokes.Any(t => !t.IsNumeric))
                Interpreter.Error("Cannot add non-numeric types");

            isDec = tokes.Any(t => t.IsDecimal);

            decimal d = 0;
            foreach (var t in tokes)
            {
                if (t.IsDecimal)
                    d += (decimal)t.Toke;
                else
                    d += (long)t.Toke;
            }

            retVal.Toke = isDec ? d : (long)d;
            return retVal;
        }

        internal static Token Subtract(params Token[] tokes)
        {
            Token retVal = new Token();
            bool isFirst = true;
            bool isDec = false;
            if (tokes.Any(t => !t.IsNumeric))
                Interpreter.Error("Cannot subtract non-numeric types");

            isDec = tokes.Any(t => t.IsDecimal);

            decimal d = 0;
            foreach (var t in tokes)
            {
                if (isFirst)
                {
                    if (t.IsDecimal)
                        d = (decimal)t.Toke;
                    else
                        d = (long)t.Toke;

                }
                else
                {
                    if (t.IsDecimal)
                        d -= (decimal)t.Toke;
                    else
                        d -= (long)t.Toke;

                }

                isFirst = false;
            }

            retVal.Toke = isDec ? d : (long)d;
            return retVal;
        }

        internal static Token Multiply(params Token[] tokes)
        {
            Token retVal = new Token();
            bool isFirst = true;
            bool isDec = false;
            if (tokes.Any(t => !t.IsNumeric))
                Interpreter.Error("Cannot multiply non-numeric types");

            isDec = tokes.Any(t => t.IsDecimal);

            decimal d = 0;
            foreach (var t in tokes)
            {
                if (isFirst)
                {
                    if (t.IsDecimal)
                        d = (decimal)t.Toke;
                    else
                        d = (long)t.Toke;

                }
                else
                {
                    if (t.IsDecimal)
                        d *= (decimal)t.Toke;
                    else
                        d *= (long)t.Toke;

                }

                isFirst = false;
            }

            retVal.Toke = isDec ? d : (long)d;
            return retVal;
        }

        internal static Token Divide(params Token[] tokes)
        {
            Token retVal = new Token();
            bool isFirst = true;
            if (tokes.Any(t => !t.IsNumeric))
                Interpreter.Error("Cannot divide non-numeric types");

            decimal d = 0;
            foreach (var t in tokes)
            {
                if (isFirst)
                {
                    if (t.IsDecimal)
                        d = (decimal)t.Toke;
                    else
                        d = (long)t.Toke;
                }
                else
                {
                    if (t.IsDecimal)
                        d /= (decimal)t.Toke;
                    else
                        d /= (long)t.Toke;

                }

                isFirst = false;
            }

            retVal.Toke = d;
            return retVal;
        }

        internal static Token Equals(Token t1, Token t2)
        {
            if (t1.IsNil && t2.IsNil)
                return Token.True;
            else if(t1.IsNil || t2.IsNil)
                return Token.False;

            if(t1.Toke.Equals(t2.Toke))
                return Token.True;

            //Check long <=> decimal comparisons
            if (t1.IsDecimal && t2.IsNumeric)
            {
                if (((decimal)t1.Toke).Equals((decimal)((long)t2.Toke)))
                    return Token.True;
            } else if (t2.IsDecimal && t1.IsNumeric)
            {
                if (((decimal)t2.Toke).Equals((decimal)((long)t1.Toke)))
                    return Token.True;
            }
            return Token.False;
        }

        internal static Token GreaterThan(Token t1, Token t2)
        {
            if (t1.IsNil || t2.IsNil)
                Interpreter.Error("Attempted to compare values (GT) with at least one value being nil");
            
            if (t1.Toke.Equals(t2.Toke))
                return Token.False;

            if(t1.IsDecimal && t2.IsDecimal)
            {
                if (((decimal) t1.Toke) > ((decimal) t2.Toke))
                    return Token.True;
                else
                    return Token.False;
            }
            if (t1.IsDecimal && t2.IsNumeric)
            {
                if (((decimal)t1.Toke) > ((decimal)((long)t2.Toke)))
                    return Token.True;
                else
                    return Token.False;
            }
            if (t2.IsDecimal && t1.IsNumeric)
            {
                if (((decimal)t2.Toke) <= ((decimal)((long)t1.Toke)))
                    return Token.True;
                else
                    return Token.False;
            }
            if (t1.IsNumeric && t2.IsNumeric)
            {
                if (((long)t1.Toke) > ((long)t2.Toke))
                    return Token.True;
                else
                    return Token.False;
            }

            Interpreter.Error("Tried to compared values (GT) but I don't know how");
            return Token.False;
        }

        internal static Token LessThan(Token t1, Token t2)
        {
            if (t1.IsNil || t2.IsNil)
                Interpreter.Error("Attempted to compare values (LT) with at least one value being nil");

            if (t1.Toke.Equals(t2.Toke))
                return Token.False;

            if (t1.IsDecimal && t2.IsDecimal)
            {
                if (((decimal)t1.Toke) < ((decimal)t2.Toke))
                    return Token.True;
                else
                    return Token.False;
            }
            if (t1.IsDecimal && t2.IsNumeric)
            {
                if (((decimal)t1.Toke) < ((decimal)((long)t2.Toke)))
                    return Token.True;
                else
                    return Token.False;
            }
            if (t2.IsDecimal && t1.IsNumeric)
            {
                if (((decimal)t2.Toke) >= ((decimal)((long)t1.Toke)))
                    return Token.True;
                else
                    return Token.False;
            }
            if (t1.IsNumeric && t2.IsNumeric)
            {
                if (((long)t1.Toke) < ((long)t2.Toke))
                    return Token.True;
                else
                    return Token.False;
            }

            Interpreter.Error("Tried to compared values (LT) but I don't know how");
            return Token.False;
        }

        internal static Token Not(Token t1)
        {
            if (t1.IsNil)
                return Token.True;

            if (t1.Toke == null && t1.Children.Count == 0)
                return Token.True;

            if (t1.IsDecimal && (decimal) t1.Toke == 0)
                return Token.True;

            if (t1.IsNumeric && (long)t1.Toke == 0)
                return Token.True;

            if (t1.Toke.ToString() == Token.False.Toke.ToString())
                return Token.True;

            if (t1.Toke.ToString() == "")
                return Token.True;

            return Token.False;
        }
    }
}

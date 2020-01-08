using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shiro.Support
{
	public static class Helptips
	{
		public static Interpreter Shiro;

		public static string GetFor(string word)
		{
			switch(word.ToLower())
			{
                case "+":
                case "-":
                case "*":
                case "/":
                    return $"({word} <number> <number>...)";

                case "=":
                case "!=":
                case ">":
                case "<":
                case ">=":
                case "<=":
                    return $"({word} <value> <value>)";

                case "list?":
                case "num?":
                case "str?":
                case "obj?":
                case "nil?":
                    return $"({word} <value>)";

                case ".":
                case ".?":
                    return $"({word} <object value> <name>...)";

                case "!":
                    return $"(! <value>)";

                case "await":
                    return "(await <name> (<async list>))";

                case "awaith":
                case "hermeticAwait":
                    return "(awaith <name> (<async list>))";

                case "awaiting?":
					return "(awaiting? <name>)";

				case "atom":
					return "(atom (...))";

                case "def?":
                    return "(def? <name>)";

                case "do":
                    return "(do (...) ...)   ; evaluates to the eval-value of the final list";

                case "enclose":
                    return "(enclose {<privates>} {<rest of object>})";

                case "error?":
                    return "(error? <value>)  ; check for errors in async-list results";

                case "fn?":
                    return "(fn? <value or name>)";

                case "gv":
                    return "(gv <name>)";

                case "impl":
                case "implementer":
                    return "(impl <name> {<implementer body>})";

                case "impl?":
                case "quack?":
                    return $"({word} <value> <implementer name>)";

                case "lower":
                    return "(lower <string>)";

                case "new":
                    return "(new <implementer name>)";

                case "nop":
                    return "(nop)  ; does nothing.  Processes any pending queue messages unless in an atom";

                case "print":
                    return "(print <value>...)";

                case "printnb":
                case "pnb":
                    return "(printnb <value>...)  ; no line breaks between values";

                case "pub":
					return "(pub '<queue name>' <value>)";
					
				case "queue?":
					return "(queue? <string>)";

				case "sub":
					return "(sub '<queue name>' (...))  ; $val contains the thing published";

				case "switch":
					return "(switch <value> <value/predicate/lambda> (...) [...] [(<default>)])";

				case "undef":
					return "(undef <name>)";

                case "upper":
                    return "(upper <string>)";

                case "v":
                    return "(v <name>)  ; consider the $<name> reader shortcut instead";

                default:
                    return Shiro.GetHelpTipFor(word) ?? word;

                    // len tcp mixin try catch throw .c.call interpolate import if json jsonv dejson pair quote string str def set sod eval skw concat v   
                    // let  defn filter map apply kw params nth range while contains split fn => .s.set.d.def.sod telnet send sendTo sendAll stop http content route status rest
			}
		}
	}
}

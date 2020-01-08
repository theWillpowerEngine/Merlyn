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
				case "awaiting?":
					return "(awaiting? <name>)";

				case "atom":
					return "(atom (...))";

				case "error?":
					return "(error? <value>)";

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

				default:
					return word;

				// enclose gv awaith hermeticAwait await len tcp impl implementer mixin impl? quack ? try catch throw .c.call interpolate import do if json jsonv dejson pair print printnb pnb quote string str def set sod eval skw concat v. .? +-* / = ! != > < <= >= list ? obj ? num ? str ? def ? fn ? nil ? let nop defn filter map apply kw params nth range while contains upper lower split fn => .s.set.d.def.sod telnet send sendTo sendAll stop http content route status rest
			}
		}
	}
}

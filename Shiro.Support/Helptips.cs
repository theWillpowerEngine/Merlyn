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
                    return $"({word} <object value> <name>...)  ; consider the <object>.<name>...  reader shortcut";

                case "!":
                    return $"(! <value>)";

                case "and":
                case "or":
                    return $"({word} <value> <value>...)";

                case "apply":
                case "map":
                    return $"({word} <command> <list>)";

                case "await":
                    return "(await <name> (<async list>))";

                case "awaith":
                case "hermeticAwait":
                    return "(awaith <name> (<async list>))";

                case "awaiting?":
					return "(awaiting? <name>)";

				case "atom":
					return "(atom (...))";

                case "def":
                case "set":
                case "sod":
                    return $"({word} <name> <value>)";

                case ".s":
                case ".set":
                case ".d":
                case ".def":
                case ".sod":
                    return $"({word} <name> <name>... <value>)";

                case ".c":
                case ".call":
                    return $"({word} <object-value> <name>... (<params>))  ; in most cases the dot-unwinding reader shortcut can do this without the keyword";

                case "catch":
                    return "(catch (...) (<catch>) [(<finally>)])  ; the thrown value is in $ex in the catch list";

                case "concat":
                    return "(concat <value>...)";

                case "contains":
                    return "(contains <string-or-list-to-search> <value-to-search-for>)";

                case "content":
                    return "(content <content-type-string> <value>)  ; Conditional:  Nimue http mode";

                case "def?":
                    return "(def? <name>)";

                case "defn":
                    return "(defn <name> (<params>) (...))";

                case "dejson":
                    return "(dejson <string>)";

                case "do":
                    return "(do (...) ...)   ; evaluates to the eval-value of the final list";

                case "enclose":
                    return "(enclose {<privates>} {<rest of object>})";

                case "error?":
                    return "(error? <value>)  ; check for errors in async-list results";

                case "eval":
                    return "(eval <list>)";

                case "filter":
                    return "(filter <predicate/lambda> <list>)";

                case "fn":
                case "=>":
                    return $"({word} (<params>) (...))";

                case "fn?":
                    return "(fn? <value or name>)";

                case "gv":
                    return "(gv <name>)";

                case "if":
                    return "(if <condition> <value> [<value>])";

                case "impl":
                case "implementer":
                    return "(impl <name> {<implementer body>})";

                case "impl?":
                case "quack?":
                    return $"({word} <value> <implementer name>)";

                case "import":
                    return "(import <library or file name>)";

                case "interpolate":
                    return "(interpolate <string>)  ; consider the `...` reader shortcut instead";

                case "json":
                    return "(json <object>)   ; serializes as-is (no evaluation)";
                case "jsonv":
                    return "(jsonv <object>)  ; evaluates all Shiro before serializing";

                case "kw":
                case "params":
                    return $"({word} <list>)";

                case "len":
                    return "(len <list or string>)";

                case "let":
                    return "(let (<paired param/value list>) (...))";

                case "lower":
                    return "(lower <string>)";

                case "mixin":
                    return "(mixin <implementer name>... <object>)";

                case "new":
                    return "(new <implementer name> [<param>...])";

                case "nop":
                    return "(nop)  ; does nothing.  Processes pending queues unless in an atom";

                case "nth":
                    return "(nth <number> <list>)";

                case "pair":
                    return "(pair <name> <value>)   ; in most cases the inline-object format can be used instead";

                case "print":
                    return "(print <value>...)";

                case "printnb":
                case "pnb":
                    return "(printnb <value>...)  ; no line breaks between values";

                case "pub":
					return "(pub '<queue name>' <value>)";
					
				case "queue?":
					return "(queue? <string>)";

                case "quote":
                    return "(quote ...)   ; consider using the '(...) reader shortcut";

                case "range":
                    return "(range <start position> <max-length> <list>)";

                case "rest":
                    return "(rest <data-source-list> <name of id>)  ; Conditional:  Nimue http mode";

                case "route":
                    return "(route <value/predicate/lambda> (...) [...])    ; Conditional:  Nimue http mode";

                case "send":
                    return "(send <value>)  ; Conditional:  Nimue telnet and tcp modes";

                case "sendall":
                    return "(sendAll <value>)  ; Conditional:  Nimue telnet and tcp modes";

                case "sendto":
                    return "(sendTo <connectionId> <value>)  ; Conditional:  Nimue telnet and tcp modes";

                case "skw":
                    return "(skw <word> <list>)";

                case "split":
                    return "(split <string> <delimiter>)";

                case "status":
                    return "(status <http-status-code> <value>)  ; Conditional:  Nimue http mode";

                case "stop":
                    return "(stop [<value>])  ; Conditional:  all Nimue modes";

                case "string":
                case "str":
                    return "(str <value> ...)";

                case "sub":
					return "(sub '<queue name>' (...))  ; $val contains the thing published";

				case "switch":
					return "(switch <value> <value/predicate/lambda> (...) [...] [(<default>)])";

                case "throw":
                    return "(throw <value>)";

                case "try":
                    return "(try (...) [(<catch>)] [(<finally>)])  ; the error is in $ex in the catch list";

				case "undef":
					return "(undef <name>)";

                case "upper":
                    return "(upper <string>)";

                case "v":
                    return "(v <name>)  ; consider the $<name> reader shortcut instead";

                case "while":
                    return "(while <condition> (...))";

                case "http":
                    return "(http <port #> (<request-handler>))";

                case "telnet":
                case "tcp":
                    return $"({word} <port #> (<command-handler>) [(<on-connect>)])";

                default:
                    return Shiro.GetHelpTipFor(word) ?? word;
			}
		}
	}
}

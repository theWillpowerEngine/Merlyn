using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Merlyn;

namespace Merp
{
    class Program
    {
        public static bool KeepREPLing = true;
		public static bool ShowResult = false;

        static void Main(string[] args)
        {
			Merpreter.Output = s =>
			{
				Console.WriteLine(s);
			};
			var merp = new Merpreter();

            var code = "";
            while (KeepREPLing)
            {
                var line = Console.ReadLine();
                if (line != "")
                    code += line + Environment.NewLine;
                else
                {
                    try
                    {
                        Console.WriteLine();
                        var ret = merp.Eval(code);
						if (ShowResult)
							Console.WriteLine("[result] " + ret.ToString());
						Console.WriteLine();

                    }
                    catch (ApplicationException aex)
                    {
                        Console.WriteLine("[error] " + aex.Message);
                        Console.WriteLine();
                    }
                    finally
                    {
                        code = "";
                    }
                }
            }
        }
    }
}

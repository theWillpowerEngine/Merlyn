using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using Merlyn;

namespace Merl
{
    class Options
    {
        public bool Interpreting = false;
        public bool Compiling = false;
        public bool ShowResult = false;
        public string EXEName;
        public string[] Files;

        public bool HasFiles => Files.Length > 0;
    }

    class Program
    {
        public static bool KeepREPLing = true;
        public static Options Options = new Options();

        private static void SetOptionsFromCmdLine(string[] args)
        {
            if (args.Length == 0)
                return;

            var files = new List<string>();
            bool atFiles = false;

            for(var i=0; i<args.Length; i++)
            {
                var arg = args[i];
                if (!arg.StartsWith("-"))
                    atFiles = true;

                if (!atFiles) {
                    switch (arg)
                    {
                        case "-sr":
                            Options.ShowResult = true;
                            break;

                        case "-c":
                            Options.Compiling = true;
                            Options.EXEName = "program.exe";
                            break;

                        case "-o":
                            i += 1;
                            Options.EXEName = args[i];
                            if (!Options.EXEName.ToLower().EndsWith(".exe"))
                                Options.EXEName += ".exe";
                            break;
                    }
                }
                else
                {
                    files.Add(arg);
                }

                Options.Files = files.ToArray();
                if (Options.HasFiles && !Options.Compiling)
                {
                    Options.Interpreting = true;
                    if (Options.Files.Length != 1)
                    {
                        Console.WriteLine("You can only interpret one Merlyn file at a time.  I'll only be reading" + Options.Files[0]);
                    }
                }
            }
        }

        static void Main(string[] args)
        {
            SetOptionsFromCmdLine(args);

            if(!Options.Compiling)
            {
                if(!Options.Interpreting)
                {
                    //Welcome to the REPL
                    Console.WriteLine("    merp running in interactive, REPL mode.  If you want to do other things try 'merp -help'");
                    Console.WriteLine("    Merlyn interpreter version: " + Merpreter.Version);
                    Console.WriteLine("        (double-tap Enter to run buffered code)");
                    Console.WriteLine();

                    Merpreter.Output = s =>
                    {
                        Console.Write(s);
                    };
                    var merp = new Merpreter();

                    var code = "";
                    while (KeepREPLing)
                    {
                        Console.Write(":>  ");
                        var line = Console.ReadLine();
                        if (line != "")
                            code += line + Environment.NewLine;
                        else
                        {
                            try
                            {
                                var ret = merp.Eval(code);
                                if (Options.ShowResult)
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
                } else
                {
                    //Interpret files
                }
            } else
            {
                //Compile time
                var c = new Compiler("main");
                c.AddMerlynModule("test", "(defn say-hi () (print 'hello nurse!'))");
                c.AddMerlynModule("main", "(do (import test) (say-hi))");

                var path = @"D:\Code";

                AssemblyName[] a = Assembly.GetExecutingAssembly().GetReferencedAssemblies();
                foreach (AssemblyName an in a)
                    if (an.FullName.ToLower().Contains("merlyn"))
                    {
                        if (File.Exists(path + "\\Merlyn.dll"))
                            File.Delete(path + "\\Merlyn.dll");
                        File.Copy(Assembly.Load(an).Location, path + "\\Merlyn.dll");
                    }


                System.CodeDom.Compiler.CompilerError ce;
                c.Compile("dan.exe", path, out ce);

                if (ce == null)
                    Console.WriteLine("Compile success");
                else
                    Console.WriteLine("Compile failed: " + ce.ErrorText);

                Console.ReadLine();
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using Shiro;
using Shiro.Build;

namespace Shiro.Cons
{
    class Options
    {
        public bool Interpreting = false;
        public bool Compiling = false;
        public bool ShowResult = false;
        public string EXEName;
        public string EntryModule = "main";
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

                        case "-ep":
                            i += 1;
                            Options.EntryModule = args[i];
                            break;
                    }
                }
                else
                {
                    var f = arg;
                    if (!f.Contains("."))
                        f += ".shr";
                    files.Add(f);
                }

                Options.Files = files.ToArray();
                if (Options.HasFiles && !Options.Compiling)
                {
                    Options.Interpreting = true;
                }
            }
        }

        static void Main(string[] args)
        {
            SetOptionsFromCmdLine(args);

            if (!Options.Compiling)
            {
                var shiro = new Interpreter();

                if (!Options.Interpreting)
                {
                    //Welcome to the REPL
                    Console.WriteLine("    shiro is running in interactive, REPL mode.  If you want to do other things try 'merp -help'");
                    Console.WriteLine("    Shiro interpreter version: " + Interpreter.Version);
                    Console.WriteLine("        (double-tap Enter to run buffered code)");
                    Console.WriteLine();

                    Interpreter.Output = s =>
                    {
                        Console.Write(s);
                    };

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
                                var ret = shiro.Eval(code);
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
                    var modules = new Dictionary<string, string>();
                    Interpreter.LoadModule = (m, s) => {
                        if (modules.ContainsKey(s.ToLower()))
                        {
                            shiro.Eval(modules[s.ToLower()]);
                            return true;
                        }

                        return Interpreter.DefaultModuleLoader(m, s);
                    };

                    foreach (var f in Options.Files)
                    {
                        var name = f.Split('.')[0].ToLower();
                        var code = File.ReadAllText(f);

                        modules.Add(name, code);
                    }

                    if(!modules.ContainsKey(Options.EntryModule.ToLower()))
                    {
                        Console.WriteLine("Entry point " + Options.EntryModule + " was not found.  Nothing to execute.");
                        return;
                    }
                    try
                    {
                        var res = shiro.Eval(modules[Options.EntryModule.ToLower()]);
                        if (Options.ShowResult)
                            Console.WriteLine("[result] " + res.ToString());
                    }
                    catch (ApplicationException aex)
                    {
                        Console.WriteLine("[error] " + aex.Message);
                        Console.WriteLine();
                    }
                }
            }
            else
            {
                //Compile time
                var c = new Compiler(Options.EntryModule);
                bool hadMain = false;
                foreach(var f in Options.Files)
                {
                    var name = f.Split('.')[0];
                    c.AddShiroModule(name, File.ReadAllText(f));

                    if (name == Options.EntryModule)
                        hadMain = true;
                }

                if (!hadMain)
                {
                    Console.WriteLine("Expected entry point for compiled application (" + Options.EntryModule + ") wasn't found.");
                }
                else
                {
                    var path = Directory.GetCurrentDirectory() + "\\bin";
                    if (!Directory.Exists(path))
                        Directory.CreateDirectory(path);

                    AssemblyName[] a = Assembly.GetExecutingAssembly().GetReferencedAssemblies();
                    foreach (AssemblyName an in a)
                        if (an.FullName.ToLower().Contains("shiro"))
                        {
                            if (File.Exists(path + "\\Shiro.Lang.dll"))
                                File.Delete(path + "\\Shiro.Lang.dll");

                            var shiroPath = new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName;
                            File.Copy(shiroPath + "\\Shiro.Lang.dll", path + "\\Shiro.Lang.dll");
                        }

                    System.CodeDom.Compiler.CompilerError ce;
                    c.Compile(Options.EXEName, path, out ce);

                    if (ce == null)
                        Console.WriteLine("Compile success");
                    else
                    {
                        Console.WriteLine("Compile failed: " + ce.ErrorText + "; press enter to continue");
                        Console.ReadLine();
                    }
                }
            }
        }
    }
}

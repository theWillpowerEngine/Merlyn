using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.IO;

using Microsoft.CSharp;
using System.CodeDom.Compiler;

namespace Shiro.Build
{
    public class Compiler
    {
        StringBuilder _mods = new StringBuilder();
        string start = "";
        string code = "";

        public Compiler(string startModule)
        {
            Stream st = Assembly.GetExecutingAssembly().GetManifestResourceStream("Shiro.Support.Merges.CompiledBase.txt");
            st.Seek(0, SeekOrigin.Begin);
            StreamReader sr = new StreamReader(st);
            code = sr.ReadToEnd();

            start = startModule;
        }

        public void AddShiroModule(string name, string code)
        {
            _mods.AppendLine($"Modules.Add(\"{name.ToLower()}\", @\"{code.Replace("\"", "\"\"")}\");");
        }

        public bool Compile(string outFile, string path, out CompilerError compilerError)
        {
            bool result = true;
            compilerError = null;

            code = code.Replace("##MODULES##", _mods.ToString());
            code = code.Replace("##START##", start);

            CSharpCodeProvider cSharpProvider = new CSharpCodeProvider();
#pragma warning disable CS0618 // Type or member is obsolete
            ICodeCompiler codeCompiler = cSharpProvider.CreateCompiler();
#pragma warning restore CS0618 // Type or member is obsolete

            // Create a CompilerParameters object that specifies assemblies referenced
            //  by the source code and the compiler options chosen by the user.
            CompilerParameters cp = new CompilerParameters(new string[] { "System.dll", path + @"\Shiro.Lang.dll" }, path + "\\" + outFile);

            cp.GenerateExecutable = true;
            cp.IncludeDebugInformation = false;
            cp.GenerateInMemory = false;
            cp.TreatWarningsAsErrors = false;
            cp.CompilerOptions = "/optimize";
            cp.MainClass = "Compiled.Program";

            try
            {
                // Compile the source code.
                CompilerResults compilerResults = codeCompiler.CompileAssemblyFromSource(cp, code);

                // Check for errors.
                if (compilerResults.Errors.Count > 0)
                {
                    compilerError = compilerResults.Errors[0];
                    result = false;
                }
            }
            catch (Exception)
            {
                result = false;
            }

            return result;
        }
    }
}

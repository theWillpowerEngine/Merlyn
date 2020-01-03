using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shiro;
using Shiro.Interop;
using Shiro.Support;

namespace ShIDE
{
    public class ShiroProject : ShiroPlugin
    {
        public static string ProjectFileDirectory = "";

        public override void RegisterAutoFunctions(Interpreter shiro)
        {
            shiro.RegisterAutoFunction("sh-install", (i, toke) =>
            {
                if (toke.Children.Count != 1)
                    Interpreter.Error("sh-install expects 1 parameters , not " + toke.Children.Count);

                var name = toke.Children[0].Eval(shiro).ToString();

                return Libraries.Install(Program.LibDirectory, ProjectFileDirectory, name) ? Token.True : Token.False;
            });

            shiro.RegisterAutoFunction("sh-project", (i, toke) =>
            {
                if (toke.Children.Count != 2)
                    Interpreter.Error("sh-project expects 2 parameters (a name and a list), not " + toke.Children.Count);

                var name = toke.Children[0].Eval(shiro).ToString();
                var tree = toke.Children[1];
                if (!tree.IsParent)
                    Interpreter.Error("Second parameter to sh-project must be a list, not " + tree.ToString());

                return new Token(new Token[] { new Token("name", name), new Token("proj", new List<Token>(new Token[] { tree.Eval(shiro) }))});
            });

            shiro.RegisterAutoFunction("shp-file", (i, toke) =>
            {
                if (toke.Children.Count != 2)
                    Interpreter.Error("shp-file expects 2 parameters (a name and a path), not " + toke.Children.Count);

                var name = toke.Children[0].Eval(shiro).ToString();
                var path = toke.Children[1].Eval(shiro).ToString();

                return new Token(new Token[] { new Token("name", name), new Token("path", path) });
            });

            shiro.RegisterAutoFunction("shp-folder", (i, toke) =>
            {
                if (toke.Children.Count != 2)
                    Interpreter.Error("shp-folder expects 2 parameters (a name and a list), not " + toke.Children.Count);

                var name = toke.Children[0].Eval(shiro).ToString();
                var tree = toke.Children[1];
                if (!tree.IsParent)
                    Interpreter.Error("Second parameter to shp-folder must be a list, not " + tree.ToString());

                return new Token(new Token[] { new Token("name", name), new Token("files", new List<Token>(new Token[] { tree.Eval(shiro) })) });
            });
        }
    }
}

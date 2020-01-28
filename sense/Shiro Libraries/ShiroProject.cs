using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Shiro;
using Shiro.Support;

namespace Shiro.Sense
{
    public class ShiroProject : Interpreter.ShiroPlugin
    {
        public static string ProjectFileDirectory = "";
        public static List<string> Libs = new List<string>();
        internal static string CurrentlyOpenProject;

        public override void RegisterAutoFunctions(Interpreter shiro)
        {
            shiro.RegisterAutoFunction("sh-install", (i, toke) =>
            {
                if (toke.Children.Count != 1)
                    Interpreter.Error("sh-install expects 1 parameters , not " + toke.Children.Count);

                var name = toke.Children[0].Eval(shiro).ToString();

                var worked = Libraries.Install(Program.LibDirectory, ProjectFileDirectory, name);
                if (!Libs.Contains(name))
                    Libs.Add(name);

                return worked ? Token.True : Token.False;
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

        internal static List<Token> DeleteFileFromTree(Interpreter shiro, string filePathOnDisk, Token projectTreeProject)
        {
            var newKids = new List<Token>();

            foreach (var child in projectTreeProject.Children)
            {
                if (child.Children.HasProperty(shiro, "path"))
                {
                    if(child.Children.GetProperty(shiro, "path").ToString() != filePathOnDisk)
                        newKids.Add(child);
                }
                else
                {
                    var kid = child;
                    kid.Children.GetProperty(shiro, "files").Children[0] = new Token(DeleteFileFromTree(shiro, filePathOnDisk, kid.Children.GetProperty(shiro, "files").Children[0]).ToArray());
                    newKids.Add(child);
                }
            }

            return newKids;
        }

        private static string BuildShiroProjectFilesAndFolders(Interpreter shiro, Token projectTreeProject)
        {
            var ret = new StringBuilder();

            foreach (var child in projectTreeProject.Children)
            {
                if (child.Children.HasProperty(shiro, "path"))
                {
                    ret.AppendLine($"      (shp-file '{child.Children.GetProperty(shiro, "name").ToString()}' '{child.Children.GetProperty(shiro, "path").ToString()}')");
                }
                else
                {
                    ret.AppendLine($"    (shp-folder '{child.Children.GetProperty(shiro, "name").ToString()}' '(");
                    ret.AppendLine(BuildShiroProjectFilesAndFolders(shiro, child.Children.GetProperty(shiro, "files").Children[0]));
                    ret.AppendLine("    ))");
                }
            }

            return ret.ToString();
        }

        internal static string GetShiroProjectFileContent(Interpreter shiro, Token projectTree)
        {
            var ret = new StringBuilder(@"(do (import shiro-project) ");

            foreach (var lib in Libs)
                ret.AppendLine($"    (sh-install '{lib}')");

            ret.AppendLine($"    (sh-project {projectTree.Children.GetProperty(shiro, "name").ToString()} '(");

            ret.AppendLine(BuildShiroProjectFilesAndFolders(shiro, projectTree.Children.GetProperty(shiro, "proj").Children[0]));

            ret.Append(")))");
            return ret.ToString();
        }

        internal static Token ExtractProjectTreeFromTreeViewOpenParenLOLCloseParen(Interpreter shiro, TreeView tree)
        {
            var retVal = Token.EmptyList;

            return retVal;
        }
    }
}

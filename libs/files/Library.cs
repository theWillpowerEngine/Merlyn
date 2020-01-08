using System;
using System.IO;
using Shiro;

namespace files
{
	public class Library : Interpreter.ShiroPlugin
    {
		public override void RegisterAutoFunctions(Interpreter shiro)
		{
            shiro.RegisterAutoFunction("read-file", (i, toke) =>
            {
                var t = toke.Children[0];
                var fileName = t.ToString();

                return new Token(File.ReadAllText(fileName));
            }, "(read-file <file name>)");

            shiro.RegisterAutoFunction("write-file", (i, toke) =>
            {
                var t = toke.Children[0];
                var fileName = t.ToString();
                t = toke.Children[1];
                var contents = t.Eval(shiro);

                File.WriteAllText(fileName, contents.ToString());
                return contents;
            }, "(write-file <file name> <value>)");

            shiro.RegisterAutoFunction("append-file", (i, toke) =>
            {
                var t = toke.Children[0];
                var fileName = t.ToString();
                t = toke.Children[1];
                var contents = t.Eval(shiro);

                File.AppendAllText(fileName, contents.ToString());
                return contents;
            }, "(append-file <file name> <value>)");

            shiro.RegisterAutoFunction("nuke-file", (i, toke) =>
            {
                var t = toke.Children[0];
                var fileName = t.ToString();

                if (!File.Exists(fileName))
                    return Token.False;

                File.Delete(fileName);
                return Token.True;
            }, "(nuke-file <file name>)");

            shiro.RegisterAutoFunction("file?", (i, toke) =>
            {
                var t = toke.Children[0];
                var fileName = t.ToString();

                return File.Exists(fileName) ? Token.True : Token.False;
            }, "(file? <file name>)");

            shiro.RegisterAutoFunction("dir?", (i, toke) =>
            {
                var t = toke.Children[0];
                var fileName = t.ToString();

                return Directory.Exists(fileName) ? Token.True : Token.False;
            }, "(dir? <directory name>)");

            shiro.RegisterAutoFunction("mkdir", (i, toke) =>
            {
                var t = toke.Children[0];
                var fileName = t.ToString();

                if (Directory.Exists(fileName))
                    return Token.False;

                Directory.CreateDirectory(fileName);
                return Directory.Exists(fileName) ? Token.True : Token.False;
            }, "(mkdir <directory name>)");

            shiro.RegisterAutoFunction("rmdir", (i, toke) =>
            {
                var t = toke.Children[0];
                var fileName = t.ToString();

                if (!Directory.Exists(fileName))
                    return Token.False;

                Directory.Delete(fileName);
                return Token.True;
            }, "(rmdir <directory name>)  ; evaluates to true or false");
        }
	}
}

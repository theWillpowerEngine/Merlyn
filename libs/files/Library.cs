using System;
using System.IO;
using Shiro.Interop;
using Shiro;

namespace files
{
	public class Library : ShiroPlugin
	{
		public override void RegisterAutoFunctions(Interpreter shiro)
		{
            shiro.RegisterAutoFunction("read-file", (i, toke) =>
            {
                var t = toke.Children[0];
                var fileName = t.ToString();

                return new Token(File.ReadAllText(fileName));
            });

            shiro.RegisterAutoFunction("write-file", (i, toke) =>
            {
                var t = toke.Children[0];
                var fileName = t.ToString();
                t = toke.Children[1];
                var contents = t.Eval(shiro);

                File.WriteAllText(fileName, contents.ToString());
                return contents;
            });

            shiro.RegisterAutoFunction("append-file", (i, toke) =>
            {
                var t = toke.Children[0];
                var fileName = t.ToString();
                t = toke.Children[1];
                var contents = t.Eval(shiro);

                File.AppendAllText(fileName, contents.ToString());
                return contents;
            });

            shiro.RegisterAutoFunction("nuke-file", (i, toke) =>
            {
                var t = toke.Children[0];
                var fileName = t.ToString();

                if (!File.Exists(fileName))
                    return Token.False;

                File.Delete(fileName);
                return Token.True;
            });

            shiro.RegisterAutoFunction("file?", (i, toke) =>
            {
                var t = toke.Children[0];
                var fileName = t.ToString();

                return File.Exists(fileName) ? Token.True : Token.False;
            });

            shiro.RegisterAutoFunction("dir?", (i, toke) =>
            {
                var t = toke.Children[0];
                var fileName = t.ToString();

                return Directory.Exists(fileName) ? Token.True : Token.False;
            });

            shiro.RegisterAutoFunction("mkdir", (i, toke) =>
            {
                var t = toke.Children[0];
                var fileName = t.ToString();

                if (Directory.Exists(fileName))
                    return Token.False;

                Directory.CreateDirectory(fileName);
                return Directory.Exists(fileName) ? Token.True : Token.False;
            });

            shiro.RegisterAutoFunction("rmdir", (i, toke) =>
            {
                var t = toke.Children[0];
                var fileName = t.ToString();

                if (!Directory.Exists(fileName))
                    return Token.False;

                Directory.Delete(fileName);
                return Token.True;
            });
        }
	}
}

using System;
using System.IO;

namespace Shiro.Support
{
    public static class Libraries
    {
        public static bool Install(string libDir, string destDir, string lib)
        {
            var path = libDir + "\\libs\\" + lib;

            if (Directory.Exists(path))
            {
                foreach (var file in Directory.GetFiles(path))
                    File.Copy(file, Path.Combine(destDir, Path.GetFileName(file)), true);

                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool Uninstall(string libDir, string destDir, string lib)
        {
            var path = libDir + "\\libs\\" + lib;

            if (Directory.Exists(path))
            {
                foreach (var file in Directory.GetFiles(path))
                    if (File.Exists(Path.Combine(destDir, Path.GetFileName(file))))
                        File.Delete(Path.Combine(destDir, Path.GetFileName(file)));

                return true;
            }
            else
            {
                return false;
            }
        }
    }
}

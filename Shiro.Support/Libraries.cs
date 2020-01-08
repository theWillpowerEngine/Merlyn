using System;
using System.Collections.Generic;
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

        public static List<string> GetAvailable(string libDir)
        {
            var path = libDir + "\\libs";

            if (Directory.Exists(path))
            {
                var retVal = new List<string>();

                foreach (var folder in Directory.GetDirectories(path))
                {
                    var eles = folder.Split('\\');
                    retVal.Add(eles[eles.Length - 1]);
                }
                retVal.Sort();
                return retVal;
            }
            else
            {
                return new List<string>();
            }
        }
    }
}

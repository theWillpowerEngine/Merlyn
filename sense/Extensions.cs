using ScintillaNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shiro.Sense
{
    internal static class Extensions
    {
        internal static bool ContainsKeyInSomeFashion(this Dictionary<string, Document> dic, string key)
        {
            if (dic.ContainsKey(key))
                return true;
            
            foreach(var k in dic.Keys)
            {
                var transformedKey = k.TrimStart('*').Trim();
                if (transformedKey == key)
                    return true;
            }

            return false;
        }
    }
}

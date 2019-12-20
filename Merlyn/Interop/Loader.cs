using Merlyn.Guts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Merlyn.Interop
{
	internal static class Loader
	{
		private static List<Assembly> LoadedDLLs = new List<Assembly>();

		internal static bool LoadDLL(string name)
		{
			try
			{
				var a = Assembly.LoadFrom(name);
				LoadedDLLs.Add(a);
                return true;
			}
			catch (Exception)
			{
                return false;
			}
		}

		internal static void LoadLibrary(string lib, Symbols sym)
		{

		}
	}
}

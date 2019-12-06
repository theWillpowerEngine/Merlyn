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

		internal static void LoadDLL(string name)
		{
			try
			{
				if (name == "MSL")
					name = "MSL.dll";

				var a = Assembly.LoadFrom(name);
				LoadedDLLs.Add(a);
			}
			catch (Exception)
			{
				Merpreter.Error("Could not import dll: " + name);
			}
		}

		internal static void LoadLibrary(string lib, Symbols sym)
		{

		}
	}
}

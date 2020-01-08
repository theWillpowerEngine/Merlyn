using Shiro.Guts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Shiro.Guts
{
	internal class Loader
	{
		private Dictionary<string, Assembly> LoadedDLLs = new Dictionary<string, Assembly>();

		internal bool LoadDLL(Interpreter shiro, string name)
		{
			if (LoadedDLLs.ContainsKey(name.ToLower()))
				return true;
			
			try
			{
				var a = Assembly.LoadFrom(Directory.GetCurrentDirectory() + "\\" + name + ".dll");
				LoadedDLLs.Add(name.ToLower(), a);

				var plugins = GetEnumerableOfType<Interpreter.ShiroPlugin>(a).ToList();
				foreach(var plugin in plugins)
					plugin.RegisterAutoFunctions(shiro);

                return true;
			}
			catch (Exception)
			{
                return false;
			}
		}

		internal IEnumerable<T> GetEnumerableOfType<T>(Assembly a, params object[] constructorArgs) where T : class
		{
			List<T> objects = new List<T>();
			foreach (Type type in a.GetTypes()
				.Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(T))))
			{
				objects.Add((T)Activator.CreateInstance(type, constructorArgs));
			}
			objects.Sort();
			return objects;
		}
	}
}

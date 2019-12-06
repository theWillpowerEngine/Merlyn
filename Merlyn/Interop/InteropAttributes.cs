using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Merlyn.Interop
{
	[AttributeUsage(AttributeTargets.Class)]
	public class MerlynLibAttribute : Attribute
	{
		public string Name;
	}

	[AttributeUsage(AttributeTargets.Method)]
	public class MerlynMethodAttribute : Attribute
	{
		public string Name;
	}
}

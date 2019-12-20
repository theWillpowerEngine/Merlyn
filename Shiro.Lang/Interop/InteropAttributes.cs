using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shiro.Interop
{
	[AttributeUsage(AttributeTargets.Class)]
	public class ShiroLibAttribute : Attribute
	{
		public string Name;
	}

	[AttributeUsage(AttributeTargets.Method)]
	public class ShiroMethodAttribute : Attribute
	{
		public string Name;
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shiro.Interop
{
	public abstract class ShiroPlugin
	{
		public abstract void RegisterAutoFunctions(Interpreter shiro);
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Merlyn.Interop
{
	internal class LoadedDLL
	{
		internal Assembly DLL;
		internal Dictionary<string, object> Libraries = new Dictionary<string, object>();
		internal Dictionary<string, object> LibraryLocks = new Dictionary<string, object>();
	}
}

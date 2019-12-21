using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shiro.Guts
{
	internal static class NetHelper
	{
		internal static bool IsRouteMatch(string request, string checkAgainst)
		{
			request = request.TrimStart('/').ToLower();
			checkAgainst = checkAgainst.TrimStart('/').ToLower();

			if (request == checkAgainst)
				return true;

			if (checkAgainst.EndsWith("*") && request.StartsWith(checkAgainst.TrimEnd('*')))
				return true;

			return false;
		}
	}
}
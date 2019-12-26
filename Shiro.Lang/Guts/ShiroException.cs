using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shiro
{
	public class ShiroException : ApplicationException
	{
		public readonly Token Exception;
		
		public ShiroException(Token t) : base("Shiro threw an exception.  Check the Exception property for the list that was thrown.")
		{
			Exception = t;
		}
	}
}

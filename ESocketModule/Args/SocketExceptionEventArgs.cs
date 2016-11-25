using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESocket.Args
{
	class SocketExceptionEventArgs
	{
		public SocketExceptionEventArgs(Exception error)
		{
			Error = error;
		}

		public Exception Error { get; private set; }
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESocket.Args
{
	public class SocketExceptionEventArgs
	{
		public SocketExceptionEventArgs(Type senderType, Exception error)
		{
			SenderType = senderType;
			Error = error;
		}

		public Type SenderType { get; private set; }
		public Exception Error { get; private set; }
	}
}

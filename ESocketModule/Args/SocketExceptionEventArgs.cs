using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESocket.Args
{
	public class SocketExceptionEventArgs : EventArgs
	{
		public SocketExceptionEventArgs(Type senderType, Exception error, bool canIgnore)
		{
			SenderType = senderType;
			Error = error;
			CanIgnore = canIgnore;
		}
		public bool CanIgnore { get; private set; }
		public Type SenderType { get; private set; }
		public Exception Error { get; private set; }
	}
}

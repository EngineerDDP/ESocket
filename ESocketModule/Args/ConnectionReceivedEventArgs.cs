using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ESocket.Controller;

namespace ESocket.Args
{
	public class ConnectionReceivedEventArgs
	{
		public ConnectionReceivedEventArgs(ISingleClient client)
		{
			RecvTime = new DateTime(DateTime.Now.Ticks);
			Client = client;
		}

		public DateTime RecvTime { get; private set; }
		public Controller.ISingleClient Client { get; private set; }
	}
}

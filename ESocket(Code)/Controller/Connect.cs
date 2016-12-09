using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Networking;
using Windows.Networking.Sockets;

namespace ESocket.Controller
{
	public class Connect
	{
		public static async Task<ISingleClient> ToRemoteAddress(String hostname, int port)
		{
			HostName remotehost = new HostName(hostname);
			StreamSocket ss = new StreamSocket();
			await ss.ConnectAsync(remotehost, port.ToString());
			ITransmitter trans = new MainTransmitter(ss);
			ISingleClient client = new SingleClient(trans, false, 0);
			return client;
		}
	}
}

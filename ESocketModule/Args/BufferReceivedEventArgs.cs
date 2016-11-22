using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ESocket.Pack;

namespace ESocket.Args
{
	class BufferReceivedEventArgs
	{

		/// <summary>
		/// 数据接收时间
		/// </summary>
		public DateTime RecvTime { get; private set; }
		public Pack.Buffer Value { get; private set; }
		public String RemoteHostName { get; private set; }
		public String RemotePortName { get; private set; }
		public String LocalPortName { get; private set; }
		public BufferReceivedEventArgs(DateTime recvTime, Pack.Buffer value, string remoteHostName, string remotePortName, string localPortName)
		{
			RecvTime = recvTime;
			Value = value;
			RemoteHostName = remoteHostName;
			RemotePortName = remotePortName;
			LocalPortName = localPortName;
		}
	}
}

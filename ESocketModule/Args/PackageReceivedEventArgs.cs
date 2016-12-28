using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ESocket.Pack;
using Windows.Networking;

namespace ESocket.Args
{
	class PackageReceivedEventArgs : EventArgs
	{
		public PackageReceivedEventArgs(DateTime recvTime, Package value, HostName remoteHostName, string remoteServiceName, string localServiceName)
		{
			RecvTime = recvTime;
			Value = value;
			RemoteHostName = remoteHostName;
			RemoteServiceName = remoteServiceName;
			LocalServiceName = localServiceName;
		}

		/// <summary>
		/// 数据接收时间
		/// </summary>
		public DateTime RecvTime { get; private set; }
		/// <summary>
		/// 数据包
		/// </summary>
		public Pack.Package Value { get; private set; }

		#region Regular Information
		/// <summary>
		/// 远端主机名
		/// </summary>
		public HostName RemoteHostName { get; private set; }
		/// <summary>
		/// 远端服务名
		/// </summary>
		public String RemoteServiceName { get; private set; }
		/// <summary>
		/// 本地服务名
		/// </summary>
		public String LocalServiceName { get; private set; }
		#endregion
	}
}

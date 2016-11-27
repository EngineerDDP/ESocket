using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking;

namespace ESocket.Args
{
	/// <summary>
	/// 连接超时异常,包含远端地址、服务名，本地端口号信息
	/// </summary>
	public class ConnectionTimeoutEventArgs : EventArgs
	{
		public ConnectionTimeoutEventArgs(HostName remoteHostName, string remoteServiceName, string localServiceName)
		{
			RemoteHostName = remoteHostName;
			RemoteServiceName = remoteServiceName;
			LocalServiceName = localServiceName;
		}

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

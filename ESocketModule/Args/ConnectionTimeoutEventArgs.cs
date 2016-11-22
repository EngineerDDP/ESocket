using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESocket.Args
{
	/// <summary>
	/// 连接超时异常,包含远端地址、服务名，本地端口号信息
	/// </summary>
	class ConnectionTimeoutEventArgs : EventArgs
	{
		/// <summary>
		/// 远端主机名
		/// </summary>
		public String RemoteHostName { get; private set; }
		/// <summary>
		/// 远端服务名
		/// </summary>
		public String RemoteServiceName { get; private set; }
		/// <summary>
		/// 本地主机名
		/// </summary>
		public String LocalServiceName { get; private set; }
		/// <summary>
		/// 构造函数
		/// </summary>
		/// <param name="remoteHostName">远端主机名</param>
		/// <param name="remoteServiceName">远端服务名</param>
		/// <param name="localServiceName">本地服务名</param>
		public ConnectionTimeoutEventArgs(string remoteHostName, string remoteServiceName, string localServiceName)
		{
			RemoteHostName = remoteHostName;
			RemoteServiceName = remoteServiceName;
			LocalServiceName = localServiceName;
		}

	}
}

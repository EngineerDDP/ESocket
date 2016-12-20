using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking;

namespace ESocket.Args
{
	/// <summary>
	/// 应用层数据包参数列表
	/// </summary>
	public class MessageReceivedEventArgs
	{
		public MessageReceivedEventArgs(HostName remoteHostName, HostName localHostName, string remoteServiceName, string type, string str, Stream data, object obj, DateTime recvTime, DateTime sendTime)
		{
			RemoteHostName = remoteHostName;
			LocalHostName = localHostName;
			RemoteServiceName = remoteServiceName;
			Type = type;
			Str = str;
			Data = data;
			Obj = obj;
			this.RecvTime = recvTime;
			this.SendTime = sendTime;
		}

		/// <summary>
		/// 远端主机名
		/// </summary>
		public HostName RemoteHostName { get; private set; }
		/// <summary>
		/// 本地主机名
		/// </summary>
		public HostName LocalHostName { get; private set; }
		/// <summary>
		/// 远端服务名
		/// </summary>
		public String RemoteServiceName { get; private set; }
		/// <summary>
		/// 数据类型标记
		/// </summary>
		public String Type { get; private set; }
		/// <summary>
		/// 附加字符串
		/// </summary>
		public String Str { get; private set; }
		/// <summary>
		/// 数据体
		/// </summary>
		public Stream Data { get; private set; }
		/// <summary>
		/// 解析到的对象（如果存在的话）
		/// </summary>
		public Object Obj { get; private set; }
		/// <summary>
		/// 接收时间点
		/// </summary>
		public DateTime RecvTime { get; private set; }
		/// <summary>
		/// 发送时间点
		/// </summary>
		public DateTime SendTime { get; private set; }
	}
}

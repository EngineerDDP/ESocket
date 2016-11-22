using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ESocket.Args;
using ESocket.Pack;

namespace ESocket.Transmitter
{
	/// <summary>
	/// 辅助数据收发单元
	/// </summary>
	class SideTransmitter : ITransmitter
	{
		/// <summary>
		/// 总下载量
		/// </summary>
		public ulong TotalDownload { get; private set; }
		/// <summary>
		/// 总运行时间
		/// </summary>
		public uint TotalRunningTime { get; private set; }
		/// <summary>
		/// 总上传
		/// </summary>
		public ulong TotalUpload { get; private set; }
		/// <summary>
		/// 连接超时事件
		/// </summary>
		public event EventHandler<ConnectionTimeoutEventArgs> OnConnectionTimeout;

		public Package RecvPackage()
		{
			throw new NotImplementedException();
		}

		public void SendPackage(Package pack)
		{
			throw new NotImplementedException();
		}
	}
}

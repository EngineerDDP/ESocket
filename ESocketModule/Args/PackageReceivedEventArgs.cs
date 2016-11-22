using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ESocket.Pack;

namespace ESocket.Args
{
	class PackageReceivedEventArgs
	{
		/// <summary>
		/// 数据接收时间
		/// </summary>
		public DateTime RecvTime { get; private set; }
		public Pack.Package Value { get; private set; }
		public PackageReceivedEventArgs(DateTime recvTime, Package value)
		{
			RecvTime = recvTime;
			Value = value;
		}

	}
}

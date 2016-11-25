using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESocket.Transmitter
{
	/// <summary>
	/// 子链接，为访问Transmitter提供统一的接口
	/// </summary>
	class SingleClient
	{
		/// <summary>
		/// 标识该链接是否为服务器端
		/// </summary>
		private Boolean IsServerPort;
		/// <summary>
		/// 连接管理器
		/// </summary>
		private IManager ConnectionManager;
		/// <summary>
		/// 远端链接
		/// </summary>
		private ITransmitter Transmitter;
		/// <summary>
		/// 对一个链接的唯一标识符
		/// </summary>
		public Int64 ID { get; private set; }
		/// <summary>
		/// 对其父连接的标识符
		/// </summary>
		public Int64 OnwerID { get; private set; }
		/// <summary>
		/// 初始化
		/// </summary>
		/// <param name="transmitter"></param>
		public SingleClient(ITransmitter transmitter)
		{
			ConnectionManager = new Manager();
			Transmitter = transmitter;
		}
	}
}

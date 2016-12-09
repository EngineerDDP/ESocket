using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ESocket.Pack
{
	/// <summary>
	/// 数据缓冲区标记
	/// </summary>
	internal class BufferTag
	{
		/// <summary>
		/// 数据名称标记
		/// </summary>
		public String Name { get; set; }
		/// <summary>
		/// 是否为用户数据，true为用户数据，false为系统协商包
		/// </summary>
		public Boolean IsUserMsg { get; set; }
		/// <summary>
		/// 数据发送时间
		/// </summary>
		public DateTime Dispatch { get; set; }
		/// <summary>
		/// 用户字符串，用于提供简单标记
		/// </summary>
		public String UserString { get; set; }
		/// <summary>
		/// 数据内容长度
		/// </summary>
		public Int64 DataLength { get; set; }
	}
}

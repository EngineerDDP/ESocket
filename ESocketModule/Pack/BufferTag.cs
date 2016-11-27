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
	[DataContract]
	internal class BufferTag
	{
		/// <summary>
		/// 数据名称标记
		/// </summary>
		[DataMember(Order = 0, IsRequired = true)]
		public String Name { get; set; }
		/// <summary>
		/// 是否为用户数据，true为用户数据，false为系统协商包
		/// </summary>
		[DataMember(Order = 1, IsRequired = true)]
		public Boolean IsUserMsg { get; set; }
		/// <summary>
		/// 数据发送时间
		/// </summary>
		[DataMember(Order = 2)]
		public long DispatchTicks { get; set; }
		public DateTime Dispatch
		{
			get
			{
				return new DateTime(DispatchTicks);
			}
			set
			{
				DispatchTicks = value.Ticks;
			}
		}
		/// <summary>
		/// 用户字符串，用于提供简单标记
		/// </summary>
		[DataMember(Order = 3)]
		public String UserString { get; set; }
		/// <summary>
		/// 数据内容长度
		/// </summary>
		[DataMember(Order = 4)]
		public Int64 DataLength { get; set; }
	}
}

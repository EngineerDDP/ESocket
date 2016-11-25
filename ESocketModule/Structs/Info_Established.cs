using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESocket.Structs
{
	/// <summary>
	/// 连接建立所需的信息
	/// </summary>
	class Info_Established
	{
		/// <summary>
		/// 所创建的链接是否为新链接
		/// </summary>
		public Boolean IsNewConnection { get; set; }
		/// <summary>
		/// 如果不是新连接，则该值表示其属于哪一个父连接
		/// </summary>
		public Int64 ConnectionID { get; set; }
		/// <summary>
		/// 远端上行速度
		/// </summary>
		public Int32 UploadSpeed { get; set; }
		/// <summary>
		/// 远端下行速度
		/// </summary>
		public Int32 DownLoadSpeed { get; set; }
	}
}

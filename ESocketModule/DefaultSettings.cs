using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESocket
{
	/// <summary>
	/// 模块默认设定,使用模块功能时先初始化设置,否则将使用常量初始化这些设置
	/// </summary>
	class DefaultSettings
	{
		#region StaticAccess
		static private DefaultSettings value;
		/// <summary>
		/// 获取默认设定，如果不存在则自动初始化
		/// </summary>
		static public DefaultSettings Value
		{
			get
			{
				if (value == null)
					value = new DefaultSettings(204800, TimeSpan.FromSeconds(6));
				return value;
			}
		}
		/// <summary>
		/// 手动设置默认设定
		/// </summary>
		/// <param name="setting"></param>
		static public void SetDefault(DefaultSettings setting)
		{
			if (value == null)
				value = setting;
			else
				throw new NotImplementedException("默认设置已设定，无法重复设定");
		}
		#endregion

		#region Settings
		/// <summary>
		/// 上行速度,字节每秒
		/// </summary>
		public Int32 UploadSpeed;
		/// <summary>
		/// 链接最长闲置时间,超出时间则会被关闭
		/// </summary>
		public TimeSpan MaxmumIdleTime;
		/// <summary>
		/// 预留的无效包序列号，当检测到此包序列号时，不解析该数据包
		/// </summary>
		public Byte NonSequence;
		/// <summary>
		/// 构造函数
		/// </summary>
		public DefaultSettings(int uploadSpeed, TimeSpan maxmumIdleTime)
		{
			UploadSpeed = uploadSpeed;
			MaxmumIdleTime = maxmumIdleTime;
			this.NonSequence = 0xff;
		}
		#endregion
	}
}

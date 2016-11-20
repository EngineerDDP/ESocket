using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESocket.Pack
{
	/// <summary>
	/// 用于存储的Package类，包含元数据序列号，包字长
	/// </summary>
	class Package
	{
		/// <summary>
		/// 序列号
		/// </summary>
		private Byte sequence;
		/// <summary>
		/// 包大小
		/// </summary>
		private UInt16 size;
		/// <summary>
		/// 数据
		/// </summary>
		private Byte[] data;
		/// <summary>
		/// 构造函数
		/// </summary>
		/// <param name="sequence">包序列号，用于包拼接</param>
		/// <param name="size">包内数据块大小</param>
		/// <param name="data">数据</param>
		public Package(byte sequence, ushort size, byte[] data)
		{
			this.sequence = sequence;
			this.size = size;
			this.data = data;
		}

		public byte Sequence
		{
			get
			{
				return sequence;
			}
		}

		public ushort Size
		{
			get
			{
				return size;
			}
		}

		public byte[] Data
		{
			get
			{
				return data;
			}
		}
	}
}

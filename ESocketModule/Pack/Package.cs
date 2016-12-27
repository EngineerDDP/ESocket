using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ESocket.Convert;

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
		private Byte[] sequence;
		/// <summary>
		/// 包大小
		/// </summary>
		private Byte[] size;
		/// <summary>
		/// 数据
		/// </summary>
		private Byte[] data;
		/// <summary>
		/// Package的实际长度
		/// </summary>
		private uint packageLength;
		public ulong Sequence
		{
			get
			{
				return Convert.Serialization.GetUInt64(sequence, 0, sequence.Length);
			}
		}
		public byte[] Data
		{
			get
			{
				return data;
			}
		}
		/// <summary>
		/// 构造函数
		/// </summary>
		/// <param name="sequence">包序列号，用于包拼接</param>
		/// <param name="size">包内数据块大小</param>
		/// <param name="data">数据</param>
		public Package(ulong sequence, uint size, byte[] data)
		{
			this.sequence = Convert.Serialization.GetBytes(sequence, DefaultSettings.LengthofSeqTag);
			this.size = Convert.Serialization.GetBytes(size, DefaultSettings.LengthofSizeTag);
			this.data = data;
			this.packageLength = (uint)(this.sequence.Length + this.size.Length + data.Length);
		}
		/// <summary>
		/// 创建空包
		/// </summary>
		public Package()
		{
			this.sequence = Serialization.GetBytes(DefaultSettings.NonSequence, DefaultSettings.LengthofSeqTag);
			this.size = null;
			this.data = null;
			this.packageLength = (uint)sequence.Length;
		}
		/// <summary>
		/// 从可读取的流构造新对象
		/// </summary>
		/// <param name="s"></param>
		public Package(System.IO.Stream s)
		{
			//设置序列号
			sequence = new byte[DefaultSettings.LengthofSeqTag];
			//读取序列号
			s.Read(sequence, 0, sequence.Length);
			//计数
			packageLength = (uint)sequence.Length;
			if(Convert.Serialization.GetUInt64(sequence,0,sequence.Length) != DefaultSettings.NonSequence)
			{
				//设置大小
				size = new byte[DefaultSettings.LengthofSizeTag];
				//读取大小
				s.Read(size, 0, size.Length);
				//设置数据
				data = new byte[Convert.Serialization.GetUInt64(size, 0, size.Length)];
				//读取数据
				s.Read(data, 0, data.Length);
				//计数
				packageLength += (uint)(size.Length + data.Length);
			}
		}
		/// <summary>
		/// 序列化对象到流中
		/// </summary>
		/// <param name="s"></param>
		public void Serialize(System.IO.Stream s)
		{
			//写序列号
			s.Write(sequence, 0, sequence.Length);
			if(data != null)
			{
				//写大小
				s.Write(size, 0, size.Length);
				//写数据
				s.Write(data, 0, data.Length);
			}
		}
		public uint PackageLength
		{
			get
			{
				return packageLength;
			}
		}
	}
}

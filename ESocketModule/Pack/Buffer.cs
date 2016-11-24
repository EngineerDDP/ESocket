using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace ESocket.Pack
{
	/// <summary>
	/// 用于缓存发送数据
	/// </summary>
	class Buffer : IDisposable
	{
		private Stream data;
		/// <summary>
		/// 数据内容
		/// </summary>
		public Stream Data
		{
			get
			{
				//标记检查点
				CheckPoint = new DateTime(DateTime.Now.Ticks);
				return data;
			}
			private set
			{
				//标记检查点
				CheckPoint = new DateTime(DateTime.Now.Ticks);
				data = value;
			}
		}
		/// <summary>
		/// 数据标记
		/// </summary>
		public BufferTag Tag { get; set; }
		/// <summary>
		/// 发送优先级
		/// </summary>
		public Int32 Priority { get; private set; }
		/// <summary>
		/// 数据检查点
		/// </summary>
		public DateTime CheckPoint { get; private set; }
		/// <summary>
		/// 数据长度
		/// </summary>
		public Int64 DataLength {
			get
			{
				return Tag.DataLength;
			}
		}
		///// <summary>
		///// 从数据包中反序列化创建新Buffer
		///// </summary>
		///// <param name="pack"></param>
		///// <returns></returns>
		//public static Buffer CreateFromPackage(Package pack)
		//{
		//	var s = new MemoryStream(pack.Data);
		//	var json = new DataContractJsonSerializer(typeof(BufferTag));
		//	var tag = json.ReadObject(s);
		//	return new Buffer(tag as BufferTag);
		//}

		/// <summary>
		/// 使用特定Tag创建缓冲区，用于数据接收
		/// </summary>
		public Buffer(BufferTag tag)
		{
			Data = new MemoryStream();
			Tag = tag;
			Priority = 0;
		}
		/// <summary>
		/// 使用特定的Tag和指定优先级创建缓冲区
		/// </summary>
		/// <param name="s"></param>
		/// <param name="tag"></param>
		/// <param name="priority"></param>
		public Buffer(Stream s, BufferTag tag, int priority)
		{
			Data = s;
			Tag = tag;
			Priority = priority;
		}
		///// <summary>
		///// 读取一个数据包，没有数据包可读时返回null
		///// </summary>
		///// <param name="seq">数据序列号</param>
		///// <returns></returns>
		//public Package ReadPackage(int seq)
		//{
		//	if (Status == SendStatus.Sendingtag)
		//	{
		//		var s = new MemoryStream();
		//		var json = new DataContractJsonSerializer(typeof(BufferTag));
		//		json.WriteObject(s, Tag);
		//		return new Package((byte)seq, (ushort)s.Length, s.ToArray());
		//	}
		//	else if (Status == SendStatus.SendingData)
		//	{
		//		byte[] buffer = new byte[Math.Min(Data.Length - Data.Position, DefaultSettings.Value.PackageSize)];
		//		Data.Read(buffer, 0, buffer.Length);
		//		return new Package((byte)seq, (ushort)buffer.Length, buffer);
		//	}
		//	else
		//		return null;
		//}
		///// <summary>
		///// 写入指定的数据包，返回写入完成的标志
		///// </summary>
		///// <param name="pack"></param>
		///// <returns></returns>
		//public Boolean WritePackage(Package pack)
		//{
		//	Data.Write(pack.Data, 0, pack.Size);
		//	if (Data.Length >= Tag.DataLength)
		//		return true;
		//	else
		//		return false;
		//}
		public void Dispose()
		{
			((IDisposable)Data).Dispose();
		}
	}
}

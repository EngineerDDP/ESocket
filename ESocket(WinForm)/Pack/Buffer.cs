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
		public Boolean TagSended { get; set; }
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
			TagSended = false;
			Priority = 0;
		}
		/// <summary>
		/// 使用特定Tag创建缓冲区，用于数据发送
		/// </summary>
		public Buffer(BufferTag tag, int priority)
		{
			Data = new MemoryStream();
			Tag = tag;
			TagSended = false;
			Priority = priority;
		}
		/// <summary>
		/// 使用特定的Tag和指定优先级创建缓冲区
		/// </summary>
		/// <param name="s"></param>
		/// <param name="tag"></param>
		/// <param name="priority"></param>
		public Buffer(BufferTag tag, Stream s, int priority)
		{
			Data = s;
			Data.Position = 0;
			Tag = tag;
			Priority = priority;
		}

		#region IDisposable Support
		private bool disposedValue = false; // 要检测冗余调用

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					// TODO: 释放托管状态(托管对象)。
					data.Dispose();
					data = null;
				}

				// TODO: 释放未托管的资源(未托管的对象)并在以下内容中替代终结器。
				// TODO: 将大型字段设置为 null。

				disposedValue = true;
			}
		}

		// TODO: 仅当以上 Dispose(bool disposing) 拥有用于释放未托管资源的代码时才替代终结器。
		// ~Buffer() {
		//   // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
		//   Dispose(false);
		// }

		// 添加此代码以正确实现可处置模式。
		public void Dispose()
		{
			// 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
			Dispose(true);
			// TODO: 如果在以上内容中替代了终结器，则取消注释以下行。
			// GC.SuppressFinalize(this);
		}
		#endregion
	}
}

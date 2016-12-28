using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ESocket.Convert;

namespace ESocket.Pack
{
	class BufferSerializer
	{
		/// <summary>
		/// 序列化方案
		/// </summary>
		IDataContractSerializer Serializer;
		/// <summary>
		/// 编码器
		/// </summary>
		IPackageEncoder Encoder;
		public BufferSerializer()
		{
			Serializer = new DataContractSerializer();
			Encoder = new PackageEncoder();
		}
		/// <summary>
		/// 读数据包,返回读取是否完成的标记
		/// </summary>
		/// <param name="tag">序列号</param>
		/// <param name="b">数据缓冲区</param>
		/// <param name="result">返回读取的结果</param>
		/// <returns></returns>
		public Boolean ReadPackage(ulong tag, Buffer b, out Pack.Package[] result)
		{
			Pack.Package p;
			List<Pack.Package> temp = new List<Package>();
			byte[] buffer;
			for (int j = 0; j < b.Priority; ++j)
			{
				//判断
				if (!b.TagSended)
				{
					buffer = Serializer.WriteObject(b.Tag);
					b.TagSended = !b.TagSended;
				}
				else if (b.Data.CanRead && b.Data.Position != b.DataLength)
				{
					buffer = new byte[(int)Math.Min(b.DataLength - b.Data.Position, DefaultSettings.Value.PackageSize)];
					b.Data.Read(buffer, 0, buffer.Length);
				}
				else
					break;
				//加密
				p = Encoder.Encode(new Package(tag, (uint)buffer.Length, buffer));
				temp.Add(p);
			}
			result = temp.ToArray();
			return b.Data.Position == b.DataLength;
		}
		/// <summary>
		/// 写数据包
		/// </summary>
		/// <param name="p">数据包</param>
		/// <param name="b">缓冲区</param>
		/// <returns>返回创建的缓冲区</returns>
		public Boolean WritePackage(Package p,ref Buffer b)
		{
			Pack.Package pack = Encoder.Decode(p);
			if (b == null)
			{
				BufferTag tag = Serializer.ReadObject(p.Data);
				b = new Pack.Buffer(tag);
			}
			else
				b.Data.Write(p.Data, 0, p.Data.Length);
			return b.Data.Length >= b.DataLength;
		}
	}
}

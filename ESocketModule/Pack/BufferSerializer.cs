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
		IDataContractSerializer Serializer;
		public BufferSerializer()
		{
			Serializer = 
		}
		public Package[] ReadPackage(Buffer b)
		{
			Pack.Package[] p;
			for (int j = 0; j < b.Priority; ++j)
			{
				//判断
				if (!b.TagSended)
				{
					buffer = Json.WriteObject(b.Tag);
					b.TagSended = !b.TagSended;
				}
				else if (b.Data.CanRead && b.Data.Position != b.DataLength)
				{
					buffer = new byte[(int)Math.Min(b.DataLength - b.Data.Position, DefaultSettings.Value.PackageSize)];
					b.Data.Read(buffer, 0, buffer.Length);
				}
				else
				{
					SendBuffer[i].Dispose();
					SendBuffer[i] = null;
					break;
				}
				//加密
				p = Encoder.Encode(new Package((byte)i, (ushort)buffer.Length, buffer));
				//发送
				try
				{
					Transmitter?.SendPackage(p);
				}
				catch (Exception e)
				{
					OnExcepetionOccurred?.Invoke(this, new Args.SocketExceptionEventArgs(this.GetType(), e, true));
				}
				buffer = null;
			}
		}
	}
}

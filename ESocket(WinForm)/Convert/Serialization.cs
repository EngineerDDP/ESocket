using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESocket.Convert
{
	class Serialization
	{
		/// <summary>
		/// 将指定的UInt16序列化为字节流
		/// </summary>
		/// <param name="buffer">数据</param>
		/// <returns></returns>
		public static Byte[] GetBytes(UInt16 buffer)
		{
			byte[] temp = new byte[2];
			temp[0] = (byte)(buffer >> 8);
			temp[1] = (byte)(buffer & 0xff);
			return temp;
		}
		/// <summary>
		/// 将指定的字节串反序列化到UInt16
		/// </summary>
		/// <param name="buffer">缓冲区</param>
		/// <returns></returns>
		public static UInt16 GetUInt16(byte[] buffer)
		{
			UInt16 temp = 0;
			temp |= buffer[1];
			temp |= (UInt16)(buffer[0] << 8);
			return temp;
		}
		/// <summary>
		/// 将指定的UInt64转换为Byte[]
		/// </summary>
		/// <param name="buffer"></param>
		/// <param name="length">数据长度，按字节算</param>
		/// <returns></returns>
		public static Byte[] GetBytes(UInt64 buffer, int length)
		{
			byte[] temp = new byte[length];
			int i = length - 1, j = 0;
			while (j < length)
			{
				temp[j] = (byte)(buffer >> (i * 8));
				++j;
				--i;
			}
			return temp;
		}
		/// <summary>
		/// 从指定缓冲区读取无符号64位整形
		/// </summary>
		/// <param name="buffer">字节数组</param>
		/// <param name="offset">起始下标</param>
		/// <param name="length">包含数据的字节长度</param>
		/// <returns>返回得到的64位无符号整形</returns>
		public static UInt64 GetUInt64(byte[] buffer, int offset, int length)
		{
			UInt64 temp = 0;
			int i = length - 1, j = offset;
			while (j < length + offset)
			{
				temp |= ((ulong)buffer[j] << (i * 8));
				++j;
				--i;
			}
			return temp;
		}
	}
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ESocket.Controller
{
	/// <summary>
	/// 缓冲创建
	/// </summary>
	class BufferCreator
	{
		/// <summary>
		/// 通过指定的流创建缓冲区
		/// </summary>
		/// <param name="type">数据类型标记</param>
		/// <param name="msg">附加字符串</param>
		/// <param name="priority">发送优先级</param>
		/// <param name="s">数据流</param>
		/// <returns></returns>
		public static Pack.Buffer CreateBuffer(String type, String msg, int priority, Stream s)
		{
			Pack.BufferTag tag = new Pack.BufferTag()
			{
				Name = type,
				IsUserMsg = true,
				Dispatch = DateTime.Now,
				UserString = msg,
				DataLength = s.Length
			};
			Pack.Buffer b = new Pack.Buffer(tag, s, priority);
			return b;
		}
		/// <summary>
		/// 通过指定的短字符串创建缓冲区
		/// </summary>
		/// <param name="type">数据类型标记</param>
		/// <param name="msg">附加字符串</param>
		/// <returns></returns>
		public static Pack.Buffer CreateBuffer(String type, String msg)
		{
			Pack.BufferTag tag = new Pack.BufferTag()
			{
				Name = type,
				IsUserMsg = true,
				Dispatch = DateTime.Now,
				UserString = msg,
				DataLength = 0
			};
			Pack.Buffer b = new Pack.Buffer(tag, 1);
			return b;
		}
		/// <summary>
		/// 创建仅标记缓冲区
		/// </summary>
		/// <param name="type">数据类型标记</param>
		/// <param name="msg">附加字符串</param>
		/// <returns></returns>
		public static Pack.Buffer CreateBuffer(String type, bool IsUser = true)
		{
			Pack.BufferTag tag = new Pack.BufferTag()
			{
				Name = type,
				IsUserMsg = IsUser,
				Dispatch = DateTime.Now,
				UserString = "None",
				DataLength = 0
			};
			Pack.Buffer b = new Pack.Buffer(tag, 1);
			return b;
		}
		/// <summary>
		/// 通过可序列化的短对象创建缓冲区
		/// </summary>
		/// <param name="type">数据类型标记</param>
		/// <param name="o">对象</param>
		/// <returns></returns>
		public static Pack.Buffer CreateBuffer(String type, object o, bool IsUser = true)
		{
			Pack.BufferTag tag = new Pack.BufferTag()
			{
				Name = type,
				IsUserMsg = IsUser,
				Dispatch = DateTime.Now,
				UserString = JsonConvert.SerializeObject(o),
				DataLength = 0
			};
			Pack.Buffer b = new Pack.Buffer(tag, 1);
			return b;
		}
	}
}

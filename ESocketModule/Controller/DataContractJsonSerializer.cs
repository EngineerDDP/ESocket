using System;
using System.IO;
using ESocket.Pack;
using Newtonsoft.Json;

namespace ESocket.Controller
{
	internal class DataContractSerializer
	{
		public byte[] WriteObject(BufferTag tag)
		{
			using (MemoryStream s = new MemoryStream())
			{
				StreamWriter sw = new StreamWriter(s);
				sw.WriteLine(tag.DataLength);
				sw.WriteLine(tag.Dispatch);
				sw.WriteLine(tag.IsUserMsg);
				sw.WriteLine(tag.Name);
				sw.Write(tag.UserString);
				sw.Flush();
				return s.ToArray();
			}
		}

		public BufferTag ReadObject(byte[] b)
		{
			using (MemoryStream s = new MemoryStream(b))
			{
				StreamReader sr = new StreamReader(s);
				BufferTag tag = new BufferTag();
				tag.DataLength = System.Convert.ToInt64(sr.ReadLine());
				tag.Dispatch = System.Convert.ToDateTime(sr.ReadLine());
				tag.IsUserMsg = System.Convert.ToBoolean(sr.ReadLine());
				tag.Name = sr.ReadLine();
				tag.UserString = sr.ReadToEnd();
				return tag;
			}
		}
	}
}
using System;
using System.IO;
using ESocket.Pack;
using Newtonsoft.Json;

namespace ESocket.Controller
{
	internal class DataContractJsonSerializer
	{
		internal void WriteObject(MemoryStream s, BufferTag tag)
		{
			StreamWriter sw = new StreamWriter(s);
			sw.Write(JsonConvert.SerializeObject(tag));
			sw.Flush();
		}

		internal BufferTag ReadObject(MemoryStream memoryStream)
		{
			StreamReader sr = new StreamReader(memoryStream);
			return JsonConvert.DeserializeObject<BufferTag>(sr.ReadToEnd());
		}
	}
}
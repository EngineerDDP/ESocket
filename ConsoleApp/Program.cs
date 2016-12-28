using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ESocket;

namespace ConsoleApp
{
	class Program
	{
		static void Main(string[] args)
		{
			ISingleClient client = ESocket.Controller.DEBUG.GetDebugClient();

			client.OnMessageReceived += Client_OnMessageReceived;

			Stream s1 = new MemoryStream();

			for(int i = 0;i < short.MaxValue;++i)
			{
				s1.WriteByte(1);
			}

			Stream s2 = new MemoryStream();
			s1.CopyTo(s2);

			client.Send("System Test", "Debug transmitter test 1", 2, s1);
			client.Send("System Test", "Debug transmitter test 1", 2, s2);

			client.Init();

			Thread.Sleep(1000000);

			Console.WriteLine(client.Ping);
		}

		private static void Client_OnMessageReceived(object sender, ESocket.Args.MessageReceivedEventArgs e)
		{
			Console.WriteLine(e.Type);
			if (e.Data.Length == short.MaxValue)
				Console.WriteLine("Data validated");
		}
	}
	public class MyClass
	{
		public int Key { get; set; }
	}
}

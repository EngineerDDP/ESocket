using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESocket.Args
{
	public class MessageStartReceivingEventArgs
	{
		public MessageStartReceivingEventArgs(string type, string msg, long length, ulong iD)
		{
			Type = type;
			Msg = msg;
			Length = length;
			ID = iD;
		}

		public String Type { get; private set; }
		public String Msg { get; private set; }
		public Int64 Length { get; private set; }
		public ulong ID { get; private set; }
	}
}

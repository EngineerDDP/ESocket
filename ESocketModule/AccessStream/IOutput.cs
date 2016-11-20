using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESocket.AccessStream
{
	interface IOutput
	{
		Int32 Length { get; }
		void WriteBytes(byte[] buffer, int offset, int length);
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESocket.AccessStream
{
	interface IInput
	{
		Int32 Length { get; }
		void ReadBytes(byte[] buffer, int offset, int length);
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ESocket
{
	interface IEClient
	{
		bool GetAvailableOutputStream(out Stream s);
		
	}
}

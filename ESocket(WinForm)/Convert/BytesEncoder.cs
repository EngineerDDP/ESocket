using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESocket.Convert
{
	/// <summary>
	/// 包加密，暂无
	/// </summary>
	class PackageEncoder : IPackageEncoder
	{
		public PackageEncoder()
		{

		}
		public Pack.Package Encode(Pack.Package pack)
		{
			return pack;
		}
		public Pack.Package Decode(Pack.Package pack)
		{
			return pack;
		}
	}
}

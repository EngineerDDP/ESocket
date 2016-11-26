using ESocket.Pack;

namespace ESocket.Convert
{
	interface IPackageEncoder
	{
		Package Decode(Package pack);
		Package Encode(Package pack);
	}
}
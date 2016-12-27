using ESocket.Pack;

namespace ESocket.Convert
{
	internal interface IDataContractSerializer
	{
		BufferTag ReadObject(byte[] b);
		byte[] WriteObject(BufferTag tag);
	}
}
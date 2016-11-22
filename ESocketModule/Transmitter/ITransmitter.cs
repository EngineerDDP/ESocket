using System;
using System.Threading.Tasks;
using ESocket.Args;
using ESocket.Pack;
using Windows.Networking;

namespace ESocket.Transmitter
{
	interface ITransmitter
	{
		HostName LocalHostName { get; }
		string LocalServiceName { get; }
		HostName RemoteHostName { get; }
		string RemoteServiceName { get; }
		ulong TotalDownload { get; }
		uint TotalRunningTime { get; }
		ulong TotalUpload { get; }

		event EventHandler<ConnectionTimeoutEventArgs> OnConnectionTimeout;
		event EventHandler<PackageReceivedEventArgs> OnPackageReceived;

		Package RecvPackage();
		void SendPackage(Package pack);
		Task StartAutoRecvAsync();
		void StopAutoRecv();
	}
}
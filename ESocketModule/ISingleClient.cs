using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ESocket.Args;
using Windows.Networking;

namespace ESocket
{
	public interface ISingleClient
	{
		uint DownloadSpeed { get; }
		uint DownloadSpeedLimit { get; }
		List<SocketExceptionEventArgs> Errors { get; }
		long ID { get; }
		String LocalHostName { get; }
		string LocalServiceName { get; }
		long OwnerID { get; }
		TimeSpan Ping { get; }
		String RemoteHostName { get; }
		string RemoteServiceName { get; }
		TimeSpan RunningTime { get; }
		ulong TotalDownload { get; }
		ulong TotalUpload { get; }
		uint UploadSpeed { get; }
		uint UploadSpeedLimit { get; }

		event EventHandler<SocketExceptionEventArgs> OnExceptionOccurred;
		event EventHandler<ConnectionTimeoutEventArgs> OnConnectionTimeout;
		event EventHandler<MessageReceivedEventArgs> OnMessageReceived;
		event EventHandler<MessageStartReceivingEventArgs> OnStartReceiving;

		void Close();
		Task Init();
		long MessageRecvStatus(int id);
		bool Send(string type, string msg);
		bool Send(string type, object o);
		bool Send(string type, string msg, int priority, Stream s, int port = 0);
		void SetInfo();
		void SetSpeedLimit(uint upload, uint download);
	}
}
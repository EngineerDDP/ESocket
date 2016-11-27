﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ESocket.Args;

namespace ESocket.Controller
{
	public interface ISingleClient
	{
		uint DownloadSpeed { get; }
		uint DownloadSpeedLimit { get; }
		List<SocketExceptionEventArgs> Errors { get; }
		long ID { get; }
		long OwnerID { get; }
		TimeSpan Ping { get; }
		TimeSpan RunningTime { get; }
		ulong TotalDownload { get; }
		ulong TotalUpload { get; }
		uint UploadSpeed { get; }
		uint UploadSpeedLimit { get; }

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
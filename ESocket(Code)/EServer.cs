﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;

namespace ESocket
{
	public class EServer : IDisposable
	{
		/// <summary>
		/// 监听端口
		/// </summary>
		private StreamSocketListener Listener;
		public event EventHandler<Args.ConnectionReceivedEventArgs> OnConnectionReceived;
		public EServer()
		{
			Listener = new StreamSocketListener();
		}
		public async Task Init(String port)
		{
			Listener.add_ConnectionReceived(Listener_ConnectionReceived);
			
			await Listener.BindServiceNameAsync(port);
		}

		private async void Listener_ConnectionReceived(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
		{
			Controller.ITransmitter trans = new Controller.MainTransmitter(args.Socket);
			Controller.SingleClient c = new Controller.SingleClient(trans, true);
			await c.Init();
			OnConnectionReceived?.Invoke(this, new Args.ConnectionReceivedEventArgs(c));
		}

		public void Dispose()
		{
			Listener.Dispose();
		}
	}
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ESocket.Controller;

namespace ESocket
{
    class EClient
    {
		/// <summary>
		/// 客户端链接集
		/// </summary>
		private List<SingleClient> Client;
		/// <summary>
		/// 上行速度
		/// </summary>
		public UInt32 UploadSpeed
		{
			get
			{
				uint r = 0;
				foreach (SingleClient s in Client)
					r += s.UploadSpeed;
				return r;
			}
		}
		/// <summary>
		/// 下行速度
		/// </summary>
		public UInt32 DownloadSpeed
		{
			get
			{
				uint r = 0;
				foreach (SingleClient s in Client)
					r += s.DownloadSpeed;
				return r;
			}
		}
		/// <summary>
		/// 接收消息
		/// </summary>
		public event EventHandler<Args.MessageReceivedEventArgs> OnMessageReceived;
		/// <summary>
		/// 开始接收
		/// </summary>
		public event EventHandler<Args.MessageStartReceivingEventArgs> OnStartReceiving;

		public EClient()
		{
			Client = new List<SingleClient>();
		}
		public async void AddClient(SingleClient c)
		{
			c.OnMessageReceived += C_OnMessageReceived;
			c.OnStartReceiving += C_OnStartReceiving;
			c.OnConnectionTimeout += C_OnConnectionTimeout;

			Client.Add(c);
			await c.Init();
		}

		private void C_OnConnectionTimeout(object sender, Args.ConnectionTimeoutEventArgs e)
		{
			(sender as ITransmitter).Dispose();
		}

		private void C_OnStartReceiving(object sender, Args.MessageStartReceivingEventArgs e)
		{
			OnStartReceiving?.Invoke(sender, e);
		}

		private void C_OnMessageReceived(object sender, Args.MessageReceivedEventArgs e)
		{
			OnMessageReceived?.Invoke(sender, e);
		}
	}
}

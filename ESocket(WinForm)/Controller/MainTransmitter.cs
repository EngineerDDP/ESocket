using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ESocket.Pack;
using Windows.Networking.Sockets;
using Windows.System.Threading;
using ESocket.Args;
using System.IO;
using System.Threading;
using Windows.Networking;

namespace ESocket.Controller
{
	/// <summary>
	/// 主收发器，使用TCP协议，本类不使用线程锁，应当保证仅有一个线程在运行
	/// </summary>
	class MainTransmitter : ITransmitter
	{
		/// <summary>
		/// 最大上行速度
		/// </summary>
		private Int32 UploadSpeedLimit;
		/// <summary>
		/// 闲置时间
		/// </summary>
		private Stopwatch IdleTime;
		/// <summary>
		/// 用于计算上下行速度定时器
		/// </summary>
		private ThreadPoolTimer Clock;
		/// <summary>
		/// 用于限制上行速度的信号量
		/// </summary>
		private AutoResetEvent Wait;
		/// <summary>
		/// 当前上行速度
		/// </summary>
		public UInt32 UploadSpeed { get; private set; }
		/// <summary>
		/// 当前下行速度
		/// </summary>
		public UInt32 DownloadSpeed { get; private set; }
		/// <summary>
		/// 总下载,单位为KB
		/// </summary>
		public UInt64 TotalDownload { get; private set; }
		/// <summary>
		/// 总上传,单位为KB
		/// </summary>
		public UInt64 TotalUpload { get; private set; }
		/// <summary>
		/// 总运行时间，以秒计
		/// </summary>
		public UInt32 TotalRunningTime { get; private set; }
		/// <summary>
		/// 用于上行计数,限制上行速度
		/// </summary>
		private UInt32 UploadCount;
		/// <summary>
		/// 用于下行计数
		/// </summary>
		private UInt32 DownloadCount;
		/// <summary>
		/// 是否生成接收事件
		/// </summary>
		private bool TrigEvent;
		/// <summary>
		/// 数据控制协议，传输流客户端
		/// </summary>
		private StreamSocket Client;
		/// <summary>
		/// 链接闲置超时事件
		/// </summary>
		public event EventHandler<ConnectionTimeoutEventArgs> OnConnectionTimeout;
		/// <summary>
		/// 收到数据包事件
		/// </summary>
		public event EventHandler<PackageReceivedEventArgs> OnPackageReceived;
		public event EventHandler<SocketExceptionEventArgs> OnSocketException;


		#region SocketInformation
		public HostName RemoteHostName
		{
			get
			{
				return Client.Information.RemoteHostName;
			}
		}
		public String RemoteServiceName
		{
			get
			{
				return Client.Information.RemoteServiceName;
			}
		}
		public HostName LocalHostName
		{
			get
			{
				return Client.Information.LocalAddress;
			}
		}
		public String LocalServiceName
		{
			get
			{
				return Client.Information.LocalPort;
			}
		}
		#endregion


		/// <summary>
		/// 使用默认的上行速度创建新数据收发器
		/// </summary>
		/// <param name="client"></param>
		public MainTransmitter(StreamSocket client)
		{
			UploadSpeedLimit = DefaultSettings.Value.UploadSpeed;
			IdleTime = new Stopwatch();
			Clock = ThreadPoolTimer.CreatePeriodicTimer(ScheduleCheck, TimeSpan.FromSeconds(1));
			Wait = new AutoResetEvent(false);
			TotalDownload = 0;
			TotalUpload = 0;
			TotalRunningTime = 0;
			UploadCount = 0;

			Client = client;
		}

		/// <summary>
		/// 开始自动接收数据并生成事件
		/// </summary>
		/// <returns></returns>
		public async Task StartAutoRecvAsync()
		{
			TrigEvent = true;
			await Task.Run(() =>
			{
				while (TrigEvent)
				{
					Package p = null;
					try
					{
						p = RecvPackage();
					}
					catch (Exception e)
					{
						OnSocketException?.Invoke(this, new SocketExceptionEventArgs(this.GetType(), e, false));
						StopAutoRecv();
					}

					if (p != null)
					{
						OnPackageReceived?.Invoke(this, new PackageReceivedEventArgs(new DateTime(DateTime.Now.Ticks), p, Client.Information.RemoteHostName, Client.Information.RemotePort, Client.Information.LocalPort));
					}
				}
			});
		}
		/// <summary>
		/// 停止自动接收数据
		/// </summary>
		public void StopAutoRecv()
		{
			TrigEvent = false;
		}
		/// <summary>
		/// 每秒一次定期检查,用于清零计数器,检查远端可用性,检查闲置时间,发送闲置数据包
		/// </summary>
		private void ScheduleCheck(ThreadPoolTimer Timer)
		{
			try
			{
				//检查发送计数器，发送闲置包
				if (UploadCount == 0)
					SendPackage(new Package());
			}
			catch (Exception e)
			{
				OnSocketException?.Invoke(this, new SocketExceptionEventArgs(this.GetType(), e, true));
				Timer.Cancel();
			}
			//刷新速度
			UploadSpeed = UploadCount;
			DownloadSpeed = DownloadCount;
			//清零计数器,唤醒线程
			UploadCount = 0;
			Wait?.Set();
			DownloadCount = 0;
			//检查闲置时间，生成超时事件
			if (IdleTime.Elapsed > DefaultSettings.Value.MaxmumIdleTime)
				OnConnectionTimeout?.Invoke(this, new ConnectionTimeoutEventArgs(Client.Information.RemoteHostName, Client.Information.RemotePort, Client.Information.LocalPort));      //传说中又臭又长的代码
																																														//增加计数器
			TotalRunningTime++;
		}
		/// <summary>
		/// 发送指定的数据包
		/// </summary>
		/// <param name="pack"></param>
		public void SendPackage(Package pack)
		{
			//检查上行量
			if (UploadCount >= UploadSpeedLimit)
				Wait.WaitOne();

			Stream s = Client.OutputStream.AsStreamForWrite();
			pack.Serialize(s);

			UploadCount += pack.PackageLength;
			TotalUpload += pack.PackageLength;

			s.Flush();
		}
		/// <summary>
		/// 接收一个数据包
		/// </summary>
		public Package RecvPackage()
		{
			//重设计时器
			IdleTime.Restart();
			
			Stream s = Client.InputStream.AsStreamForRead();
			Package p = new Package(s);

			TotalDownload += p.PackageLength;
			DownloadCount += p.PackageLength;

			return p;
		}

		public void Dispose()
		{
			Clock.Cancel();
			Client.Dispose();
			Wait.Dispose();
			Wait = null;
		}
	}
}

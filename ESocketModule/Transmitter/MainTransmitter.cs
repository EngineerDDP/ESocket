using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ESocket.Pack;
using Windows.Networking.Sockets;
using Windows.System.Threading;
using ESocket.Exceptions;
using System.IO;
using System.Threading;

namespace ESocket.Transmitter
{
	/// <summary>
	/// 主收发器，使用TCP协议，本类不使用线程锁，应当保证仅有一个线程在运行
	/// </summary>
	class MainTransmitter
	{
		/// <summary>
		/// 最大上行速度
		/// </summary>
		private Int32 UploadSpeed;
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
		/// 总下载,单位为KB
		/// </summary>
		private UInt32 TotalDownload;
		/// <summary>
		/// 总上传,单位为KB
		/// </summary>
		private UInt32 TotalUpload;
		/// <summary>
		/// 总运行时间，以秒计
		/// </summary>
		private UInt32 TotalRunningTime;
		/// <summary>
		/// 用于上行计数,限制上行速度
		/// </summary>
		private UInt32 UploadCount;
		/// <summary>
		/// 数据控制协议，传输流客户端
		/// </summary>
		private StreamSocket Client;
		/// <summary>
		/// 链接闲置超时事件
		/// </summary>
		public event EventHandler<ConnectionTimeoutEventArgs> OnConnectionTimeout;
		/// <summary>
		/// 使用默认的上行速度创建新数据收发器
		/// </summary>
		/// <param name="client"></param>
		private MainTransmitter(StreamSocket client)
		{
			UploadSpeed = DefaultSettings.Value.UploadSpeed;
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
		/// 每秒一次定期检查,用于清零计数器,检查远端可用性,检查闲置时间,发送闲置数据包
		/// </summary>
		private void ScheduleCheck(ThreadPoolTimer Timer)
		{
			//检查发送计数器，发送闲置包
			if (UploadCount == 0)
				;
			//清零计数器,唤醒线程
			UploadCount = 0;
			Wait.Set();
			//检查闲置时间，生成超时事件
			if (IdleTime.Elapsed > DefaultSettings.Value.MaxmumIdleTime)
				OnConnectionTimeout?.Invoke(this, new ConnectionTimeoutEventArgs(Client.Information.RemoteHostName.CanonicalName, Client.Information.RemotePort, Client.Information.LocalPort));		//传说中又臭又长的代码
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
			if(UploadCount >= UploadSpeed)
				Wait.WaitOne();

			Stream s = Client.OutputStream.AsStreamForWrite();
			s.WriteByte(pack.Sequence);

			UploadCount += 1;
			TotalUpload += 1;
			//检查序列号并写入剩余数据
			if (pack.Sequence != DefaultSettings.Value.NonSequence)
			{
				s.Write(Convert.Serialization.GetBytes(pack.Size), 0, 2);
				s.Write(pack.Data, 0, pack.Size);

				//设置计数器
				UploadCount += pack.Size + 2u;
				TotalUpload += pack.Size + 2u;
			}
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
			byte b = (byte)s.ReadByte();

			TotalDownload += 1;
			//检查序列号并读取剩余数据
			if (b != DefaultSettings.Value.NonSequence)
			{
				byte[] buffer = new byte[2];
				s.Read(buffer, 0, 2);
				UInt16 size = Convert.Serialization.GetUInt16(buffer);
				buffer = new byte[size];
				s.Read(buffer, 0, size);

				//设置计数器
				TotalDownload += size + 2u;
				return new Package(b, size, buffer);
			}
			return null;
		}
	}
}

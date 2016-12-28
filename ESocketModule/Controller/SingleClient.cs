using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Windows.Networking;

namespace ESocket.Controller
{
#if DEBUG
	public class DEBUG
	{
		public static ISingleClient GetDebugClient()
		{
			return new SingleClient(new DebugTransmitter(), true, 0);
		}
	}
#endif
	/// <summary>
	/// 子链接，为访问Transmitter提供统一的接口
	/// </summary>
	class SingleClient : ISingleClient, IDisposable
	{
		/// <summary>
		/// 标识该链接是否为服务器端
		/// </summary>
		private Boolean IsServerPort;
		/// <summary>
		/// 连接管理器
		/// </summary>
		private IManager ConnectionManager;
		/// <summary>
		/// 远端链接
		/// </summary>
		private ITransmitter Transmitter;
		/// <summary>
		/// 收集到的错误
		/// </summary>
		public List<Args.SocketExceptionEventArgs> Errors { get; private set; }
		/// <summary>
		/// 对一个链接的唯一标识符
		/// </summary>
		public Int64 ID { get; private set; }
		/// <summary>
		/// 对其父连接的标识符
		/// </summary>
		public Int64 OwnerID { get; private set; }
		/// <summary>
		/// 延迟
		/// </summary>
		public TimeSpan Ping { get; private set; }
		/// <summary>
		/// 上行速度
		/// </summary>
		public UInt32 UploadSpeed
		{
			get
			{
				return Transmitter.UploadSpeed;
			}
		}
		/// <summary>
		/// 下行速度
		/// </summary>
		public UInt32 DownloadSpeed
		{
			get
			{
				return Transmitter.DownloadSpeed;
			}
		}

		/// <summary>
		/// 最大上行速度
		/// </summary>
		public UInt32 UploadSpeedLimit { get; private set; }
		/// <summary>
		/// 最大下行速度
		/// </summary>
		public UInt32 DownloadSpeedLimit { get; private set; }
		/// <summary>
		/// 总下行数据量
		/// </summary>
		public UInt64 TotalDownload
		{
			get
			{
				return Transmitter.TotalDownload;
			}
		}
		/// <summary>
		/// 总上行数据量
		/// </summary>
		public UInt64 TotalUpload
		{
			get
			{
				return Transmitter.TotalUpload;
			}
		}
		/// <summary>
		/// 总运行时间
		/// </summary>
		public TimeSpan RunningTime
		{
			get
			{
				return TimeSpan.FromSeconds(Transmitter.TotalRunningTime);
			}
		}
		public String RemoteHostName
		{
			get
			{
				return Transmitter.RemoteHostName.CanonicalName;
			}
		}
		public String LocalHostName
		{
			get
			{
				return Transmitter.LocalHostName.CanonicalName;
			}
		}
		public String RemoteServiceName
		{
			get
			{
				return Transmitter.RemoteServiceName;
			}
		}
		public String LocalServiceName
		{
			get
			{
				return Transmitter.LocalServiceName;
			}
		}
		/// <summary>
		/// 传递消息
		/// </summary>
		public event EventHandler<Args.MessageReceivedEventArgs> OnMessageReceived;
		public event EventHandler<Args.SocketExceptionEventArgs> OnExceptionOccurred;
		public event EventHandler<Args.MessageStartReceivingEventArgs> OnStartReceiving
		{
			add
			{
				ConnectionManager.OnStartReceiving += value;
			}
			remove
			{
				ConnectionManager.OnStartReceiving -= value;
			}
		}
		/// <summary>
		/// 递交超时事件
		/// </summary>
		public event EventHandler<Args.ConnectionTimeoutEventArgs> OnConnectionTimeout
		{
			add
			{
				Transmitter.OnConnectionTimeout += value;
			}
			remove
			{
				Transmitter.OnConnectionTimeout -= value;
			}
		}
		/// <summary>
		/// 初始化
		/// </summary>
		/// <param name="transmitter"></param>
		public SingleClient(ITransmitter transmitter, bool isServer, long owner = -1)
		{
			IsServerPort = isServer;
			OwnerID = owner;
			Ping = new TimeSpan();
			ConnectionManager = new Manager();
			Transmitter = transmitter;
			ID = GetID();
			Errors = new List<Args.SocketExceptionEventArgs>();
			Transmitter.OnSocketException += new EventHandler<Args.SocketExceptionEventArgs>(ConnectionManager_OnExcepetionOccurred);
		}
		/// <summary>
		/// 设置速度限制
		/// </summary>
		/// <param name="upload">上行速度限制</param>
		/// <param name="download">下行速度限制</param>
		public void SetSpeedLimit(UInt32 upload, UInt32 download)
		{
			UploadSpeedLimit = upload;
			DownloadSpeedLimit = download;
		}
		/// <summary>
		/// 获取ID
		/// </summary>
		/// <returns></returns>
		private Int64 GetID()
		{
			long id = DateTime.Now.Ticks;
			id += Transmitter.RemoteHostName.GetHashCode() << 32;
			id += System.Convert.ToInt32(Transmitter.RemoteServiceName);
			return id;
		}
		/// <summary>
		/// 初始化
		/// </summary>
		public async Task Init()
		{
			if (ConnectionManager.SetTransmitter(Transmitter))
			{
				ConnectionManager.OnBufferReceived += ConnectionManager_OnBufferReceived;
				ConnectionManager.OnExcepetionOccurred += ConnectionManager_OnExcepetionOccurred;

				await ConnectionManager.Init();
			}
		}

		public void Send(String type, String msg, int priority, Stream s, int port = 0)
		{
			ConnectionManager.AddBuffer(BufferCreator.CreateBuffer(type, msg, priority, s));
		}
		public void Send(String type, String msg)
		{
			ConnectionManager.AddBuffer(BufferCreator.CreateBuffer(type, msg));
		}
		public void Send(String type, object o)
		{
			ConnectionManager.AddBuffer(BufferCreator.CreateBuffer(type, o));
		}
		/// <summary>
		/// 获取指定ID的接受情况
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public long MessageRecvStatus(ulong id)
		{
			return ConnectionManager.GetLength(id);
		}
		/// <summary>
		/// 错误收集
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ConnectionManager_OnExcepetionOccurred(object sender, Args.SocketExceptionEventArgs e)
		{
			if (e.CanIgnore)
				Errors.Add(e);
			else
				OnExceptionOccurred?.Invoke(this, e);
		}
		/// <summary>
		/// 响应Buffer，整理并提交
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ConnectionManager_OnBufferReceived(object sender, Args.BufferReceivedEventArgs e)
		{
			//记录延迟
			Ping = e.RecvTime - e.Value.Tag.Dispatch;
			object o = null;
			if (!e.Value.Tag.IsUserMsg)
			{
				try
				{
					o = JsonConvert.DeserializeObject<System_Info>(e.Value.Tag.UserString);
				}
				catch (Exception error)
				{
					Errors.Add(new Args.SocketExceptionEventArgs(this.GetType(), error, true));
				}
				(o as System_Info).Execute(this);
			}
			else
			{
				e.Value.Data.Position = 0;
				Args.MessageReceivedEventArgs args = new Args.MessageReceivedEventArgs(
					e.RemoteHostName,
					Transmitter.LocalHostName,
					e.RemoteServiceName,
					e.Value.Tag.Name,
					e.Value.Tag.UserString,
					e.Value.Data,
					o, 
					e.RecvTime,
					e.Value.Tag.Dispatch);
				OnMessageReceived?.Invoke(this, args);
			}
		}
		/// <summary>
		/// 协商远端配置
		/// </summary>
		public void SetInfo()
		{
			System_Info info = new System_Info()
			{
				OwnerID = OwnerID,
				DownLoadSpeed = DownloadSpeedLimit,
				UploadSpeed = UploadSpeedLimit
			};
			Pack.Buffer b = BufferCreator.CreateBuffer("Init", info, false);
			ConnectionManager.AddBuffer(b);
		}

		public void Close()
		{
			ConnectionManager.RemoveTransmitter();
			Transmitter.Dispose();
		}

		#region Communicate
		/// <summary>
		/// 连接建立所需的信息
		/// </summary>
		class System_Info
		{
			/// <summary>
			/// 该值表示其属于哪一个父连接
			/// </summary>
			public Int64 OwnerID { get; set; }
			/// <summary>
			/// 远端上行速度
			/// </summary>
			public UInt32 UploadSpeed { get; set; }
			/// <summary>
			/// 远端下行速度
			/// </summary>
			public UInt32 DownLoadSpeed { get; set; }
			public void Execute(SingleClient c)
			{
				c.OwnerID = OwnerID;
				c.UploadSpeedLimit = UploadSpeed;
				c.DownloadSpeedLimit = DownLoadSpeed;
			}
		}

		#endregion

		#region IDisposable Support
		private bool disposedValue = false; // 要检测冗余调用

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					// TODO: 释放托管状态(托管对象)。
					Transmitter.Dispose();
					ConnectionManager.Dispose();
				}

				// TODO: 释放未托管的资源(未托管的对象)并在以下内容中替代终结器。
				// TODO: 将大型字段设置为 null。

				disposedValue = true;
			}
		}

		// TODO: 仅当以上 Dispose(bool disposing) 拥有用于释放未托管资源的代码时才替代终结器。
		// ~SingleClient() {
		//   // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
		//   Dispose(false);
		// }

		// 添加此代码以正确实现可处置模式。
		public void Dispose()
		{
			// 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
			Dispose(true);
			// TODO: 如果在以上内容中替代了终结器，则取消注释以下行。
			// GC.SuppressFinalize(this);
		}
		#endregion
	}
}

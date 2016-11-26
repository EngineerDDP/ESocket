using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ESocket.Controller
{
	/// <summary>
	/// 子链接，为访问Transmitter提供统一的接口
	/// </summary>
	class SingleClient
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
		/// 上行速度
		/// </summary>
		public Int32 UploadSpeed { get; private set; }
		/// <summary>
		/// 下行速度
		/// </summary>
		public Int32 DownloadSpeed { get; private set; }
		/// <summary>
		/// 最大上行速度
		/// </summary>
		public Int32 UploadSpeedLimit { get; private set; }
		/// <summary>
		/// 最大下行速度
		/// </summary>
		public Int32 DownloadSpeedLimit { get; private set; }
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
		/// <summary>
		/// 传递消息
		/// </summary>
		public event EventHandler<Args.MessageReceivedEventArgs> OnMessageReceived;
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
		public SingleClient(ITransmitter transmitter, bool isServer, long owner = DefaultSettings.NewConnectionID)
		{
			IsServerPort = isServer;
			OwnerID = owner;
			ConnectionManager = new Manager();
			Transmitter = transmitter;
			ID = GetID();
		}
		/// <summary>
		/// 设置速度限制
		/// </summary>
		/// <param name="upload">上行速度限制</param>
		/// <param name="download">下行速度限制</param>
		public void SetSpeedLimit(Int32 upload,Int32 download)
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
			ConnectionManager.SetTransmitter(Transmitter);
			ConnectionManager.OnBufferReceived += ConnectionManager_OnBufferReceived;
			ConnectionManager.OnExcepetionOccurred += ConnectionManager_OnExcepetionOccurred;
			
			await ConnectionManager.Init();
		}
		/// <summary>
		/// 错误收集
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ConnectionManager_OnExcepetionOccurred(object sender, Args.SocketExceptionEventArgs e)
		{
			Errors.Add(e);
		}
		/// <summary>
		/// 响应Buffer，整理并提交
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ConnectionManager_OnBufferReceived(object sender, Args.BufferReceivedEventArgs e)
		{
			object o = null;
			try
			{
				o = JsonConvert.DeserializeObject(e.Value.Tag.UserString);
			}
			catch { }
			if (!e.Value.Tag.IsUserMsg)
			{
				(o as System_Info).Execute(this);
			}
			else
			{
				Args.MessageReceivedEventArgs args = new Args.MessageReceivedEventArgs(
					e.RemoteHostName,
					Transmitter.LocalHostName,
					e.RemoteServiceName,
					e.Value.Tag.Name,
					e.Value.Tag.UserString,
					e.Value.Data,
					o, e.RecvTime);
				OnMessageReceived?.Invoke(this, args);
			}
		}
		/// <summary>
		/// 协商远端配置
		/// </summary>
		public void SetInfo()
		{
			System_Info info = new ConnectionEstablish()
			{
				OwnerID = OwnerID,
				DownLoadSpeed = DownloadSpeedLimit,
				UploadSpeed = UploadSpeedLimit
			};
			Pack.Buffer b = BufferCreator.CreateBuffer("Init", info, false);
			b.Tag.IsUserMsg = false;
			ConnectionManager.AddBuffer(b);
		}

		#region Communicate
		/// <summary>
		/// 连接建立所需的信息
		/// </summary>
		abstract class System_Info
		{
			/// <summary>
			/// 该值表示其属于哪一个父连接
			/// </summary>
			public Int64 OwnerID { get; set; }
			/// <summary>
			/// 远端上行速度
			/// </summary>
			public Int32 UploadSpeed { get; set; }
			/// <summary>
			/// 远端下行速度
			/// </summary>
			public Int32 DownLoadSpeed { get; set; }
			public abstract void Execute(SingleClient c);
		}
		class ConnectionEstablish : System_Info
		{
			public override void Execute(SingleClient c)
			{
				c.OwnerID = OwnerID;
				c.UploadSpeedLimit = UploadSpeed;
				c.DownloadSpeedLimit = DownLoadSpeed;
			}
		}
		class ResetUploadSpeed : System_Info
		{
			public override void Execute(SingleClient c)
			{
				if(c.IsServerPort)
				{
					c.UploadSpeedLimit = UploadSpeed;
					c.DownloadSpeedLimit = DownLoadSpeed;
				}
			}
		}
		#endregion
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ESocket.Pack;
using System.Runtime.Serialization.Json;
using System.IO;
using Windows.System.Threading;
using System.Threading;

namespace ESocket.Controller
{
	/// <summary>
	/// 管理器负责操作Transmitter，实现并行收发，优先级控制
	/// 本类需要严格线程同步管理，保证主线程和异步发送线程无冲突
	/// </summary>
	class Manager : IManager
	{
		/// <summary>
		/// 用于操作的收发器
		/// </summary>
		private ITransmitter Transmitter;
		/// <summary>
		/// 对象序列化方法
		/// </summary>
		private DataContractJsonSerializer Json;
		/// <summary>
		/// 数据加密算法
		/// </summary>
		private Convert.IPackageEncoder Encoder;
		/// <summary>
		/// 检查计时器
		/// </summary>
		private ThreadPoolTimer Check;
		/// <summary>
		/// 等待填充发送区的信号量
		/// </summary>
		private AutoResetEvent Wait;
		/// <summary>
		/// 存储缓存的发送队列
		/// </summary>
		private Dictionary<int, ESocket.Pack.Buffer> SendBuffer;
		/// <summary>
		/// 存储缓存的接收队列
		/// </summary>
		private Dictionary<int, ESocket.Pack.Buffer> RecvBuffer;
		/// <summary>
		/// 当触发异常时生成
		/// </summary>
		public event EventHandler<Args.SocketExceptionEventArgs> OnExcepetionOccurred;
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
		/// 当收完消息后发生
		/// </summary>
		public event EventHandler<Args.BufferReceivedEventArgs> OnBufferReceived;
		/// <summary>
		/// 创建空的管理器
		/// </summary>
		public Manager()
		{
			Transmitter = null;
			Encoder = new Convert.PackageEncoder();
			Json = new DataContractJsonSerializer(typeof(BufferTag));
			//初始化
			SendBuffer = new Dictionary<int, Pack.Buffer>();
			RecvBuffer = new Dictionary<int, Pack.Buffer>();
			for(int i = 0;i < DefaultSettings.MaxmumSequence;++i)
			{
				SendBuffer.Add(i, null);
				RecvBuffer.Add(i, null);
			}
		}
		/// <summary>
		/// 添加Buffer
		/// </summary>
		/// <param name="buffer"></param>
		/// <returns></returns>
		public Boolean AddBuffer(Pack.Buffer buffer)
		{
			int id = 0;
			if (FindAvailableID(ref id))
			{
				SendBuffer[id] = buffer;
				Wait.Set();
				return true;
			}
			else
				return false;
		}
		/// <summary>
		/// 寻找一个可用的ID
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		private Boolean FindAvailableID(ref int id)
		{
			foreach (int i in SendBuffer.Keys)
				if (SendBuffer[i] == null)
				{
					id = i;
					return true;
				}
			return false;
		}
		/// <summary>
		/// 设置收发器，返回成功与否的标志
		/// </summary>
		/// <param name="transmitter"></param>
		/// <returns></returns>
		public Boolean SetTransmitter(ITransmitter transmitter)
		{
			if (Transmitter == null)
			{
				Transmitter = transmitter;
				Transmitter.OnPackageReceived += Transmitter_OnPackageReceived;
				return true;
			}
			else
				return false;
		}
		/// <summary>
		/// 开始接受数据
		/// </summary>
		/// <returns></returns>
		public async Task Init()
		{
			if (Transmitter != null)
			{
				//创建任务组
				Task[] t = new Task[2];
				//设置检查点
				Check = ThreadPoolTimer.CreatePeriodicTimer(CheckForBadBuffer, DefaultSettings.Value.MaxmumPackageDelay);
				//设置异步接收
				t[0] = Transmitter.StartAutoRecvAsync();
				//设置异步发送
				t[1] = Task.Run(new Action(SendingThread));
				//开始
				await Task.WhenAll(t);
				//结束
				Check.Cancel();
			}
		}
		/// <summary>
		/// 用于发送数据的线程
		/// </summary>
		private void SendingThread()
		{
			using (MemoryStream s = new MemoryStream())
			{
				while (Transmitter != null)	//发送序列全空等待
				{
					int count = 0;
					foreach (int i in SendBuffer.Keys)
					{
						if (SendBuffer[i] == null)
						{
							++count;
							continue;
						}
						Pack.Buffer b = SendBuffer[i];
						Pack.Package p;
						for (int j = 0;j < b.Priority; ++j) 
						{
							//判断
							if (b.Tag != null)
								Json.WriteObject(s, b.Tag);
							else if (b.Data.Position != b.DataLength)
								b.Data.CopyTo(s, (int)Math.Min(b.DataLength - b.Data.Position, DefaultSettings.Value.PackageSize));
							else
							{
								SendBuffer[i] = null;
								break;
							}
							//加密
							p = Encoder.Encode(new Package((byte)i, (ushort)s.Length, s.ToArray()));
							//发送
							try
							{
								Transmitter?.SendPackage(p);
							}
							catch(Exception e)
							{
								OnExcepetionOccurred?.Invoke(this, new Args.SocketExceptionEventArgs(this.GetType(), e));
							}
							//重设
							s.SetLength(0);
						}
					}
					if (count == SendBuffer.Keys.Count)
						Wait.WaitOne();
				}
			}
		}
		
		/// <summary>
		/// 响应接收信息的事件 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Transmitter_OnPackageReceived(object sender, Args.PackageReceivedEventArgs e)
		{
			//解密
			Pack.Package p = Encoder.Decode(e.Value);
			if(RecvBuffer[p.Sequence] == null)
			{
				try
				{
					BufferTag tag = Json.ReadObject(new MemoryStream(p.Data)) as BufferTag;
					RecvBuffer[p.Sequence] = new Pack.Buffer(tag);
				}
				catch { }
			}
			else
			{
				var buffer = RecvBuffer[p.Sequence];
				buffer.Data.Write(p.Data, 0, p.Size);
			}
			if (RecvBuffer[p.Sequence].Data.Length == RecvBuffer[p.Sequence].DataLength)
			{
				OnBufferReceived?.Invoke(this, new Args.BufferReceivedEventArgs(RecvBuffer[p.Sequence].CheckPoint, RecvBuffer[p.Sequence], e.RemoteHostName, e.RemoteServiceName, e.LocalServiceName));
				RecvBuffer[p.Sequence] = null;
				RecvBuffer[p.Sequence].Dispose();
			}
		}
		/// <summary>
		/// 检查Buffer是否已过期
		/// </summary>
		/// <param name="Timer"></param>
		private void CheckForBadBuffer(ThreadPoolTimer Timer)
		{
			DateTime oldestTime = DateTime.Now - DefaultSettings.Value.MaxmumPackageDelay;
			lock(RecvBuffer)
			{
				foreach(int i in RecvBuffer.Keys)
				{
					if (RecvBuffer[i] != null && RecvBuffer[i].CheckPoint < oldestTime)
					{
						RecvBuffer[i].Dispose();
						RecvBuffer[i] = null;
					}
				}
			}
		}
		/// <summary>
		/// 移除收发器，返回成功与否的标志
		/// </summary>
		/// <param name="transmitter"></param>
		/// <returns></returns>
		public Boolean RemoveTransmitter()
		{
			if (Transmitter != null)
			{
				Transmitter.StopAutoRecv();
				Transmitter.OnPackageReceived -= Transmitter_OnPackageReceived;
				Transmitter = null;
				return true;
			}
			else
				return false;
		}
	}
}

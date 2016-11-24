using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ESocket.Pack;
using System.Runtime.Serialization.Json;
using System.IO;

namespace ESocket.Transmitter
{
	/// <summary>
	/// 管理器负责操作Transmitter，实现并行收发，优先级控制
	/// 本类需要严格线程同步管理，保证主线程和异步发送线程无冲突
	/// </summary>
	class Manager
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
		/// 存储缓存的发送队列
		/// </summary>
		private Dictionary<int, ESocket.Pack.Buffer> SendBuffer;
		/// <summary>
		/// 存储缓存的接收队列
		/// </summary>
		private Dictionary<int, ESocket.Pack.Buffer> RecvBuffer;
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
				//设置异步接收
				t[0] = Transmitter.StartAutoRecvAsync();
				//设置异步发送
				t[1] = Task.Run(new Action(SendingThread));
				//开始
				await Task.WhenAll(t);
			}
		}
		/// <summary>
		/// 用于发送数据的线程
		/// </summary>
		private void SendingThread()
		{
			using (MemoryStream s = new MemoryStream())
			{
				while (Transmitter != null)
				{
					foreach (int i in SendBuffer.Keys)
					{
						Pack.Buffer b = SendBuffer[i];
						Pack.Package p;
						for (int j = 0; j < b.Priority; ++j)
						{
							//判断
							if (b.Tag != null)
								Json.WriteObject(s, b.Tag);
							else if (b.Data.Position != b.DataLength)
								b.Data.CopyTo(s, (int)Math.Min(b.DataLength - b.Data.Position, DefaultSettings.Value.PackageSize));
							else
								break;
							//发送
							p = new Package((byte)i, (ushort)s.Length, s.ToArray());
							Transmitter?.SendPackage(p);
							//重设
							s.SetLength(0);
						}
					}
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
			if(RecvBuffer[e.Value.Sequence] == null)
			{
				try
				{
					BufferTag tag = Json.ReadObject(new MemoryStream(e.Value.Data)) as BufferTag;
					RecvBuffer[e.Value.Sequence] = new Pack.Buffer(tag);
				}
				catch { }
			}
			else
			{
				var buffer = RecvBuffer[e.Value.Sequence];
				buffer.Data.Write(e.Value.Data, 0, e.Value.Size);
				if (buffer.Data.Length == buffer.DataLength)
					OnBufferReceived?.Invoke(this, new Args.BufferReceivedEventArgs(buffer.CheckPoint,buffer,e.RemoteHostName,e.RemoteServiceName,e.LocalServiceName));
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

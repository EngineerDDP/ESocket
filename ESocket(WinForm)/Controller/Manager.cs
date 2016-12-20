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
using Newtonsoft.Json;
using System.Net.Sockets;

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
		private DataContractSerializer Json;
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
		private Pack.Buffer[] SendBuffer;
		/// <summary>
		/// 存储缓存的接收队列
		/// </summary>
		private Pack.Buffer[] RecvBuffer;
		/// <summary>
		/// 当触发异常时生成
		/// </summary>
		public event EventHandler<Args.SocketExceptionEventArgs> OnExcepetionOccurred;
		/// <summary>
		/// 开始接收新数据
		/// </summary>
		public event EventHandler<Args.MessageStartReceivingEventArgs> OnStartReceiving;
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
			Wait = new AutoResetEvent(false);
			Transmitter = null;
			Encoder = new Convert.PackageEncoder();
			Json = new DataContractSerializer();
			//初始化
			SendBuffer = new Pack.Buffer[DefaultSettings.MaxmumSequence];
			RecvBuffer = new Pack.Buffer[DefaultSettings.MaxmumSequence];
			for (int i = 0; i < DefaultSettings.MaxmumSequence; ++i)
			{
				SendBuffer[i] = null;
				RecvBuffer[i] = null;
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
			for (int i = 0; i < SendBuffer.Length; ++i)
				if (SendBuffer[i] == null)
				{
					id = i;
					return true;
				}
			return false;
		}
		/// <summary>
		/// 获取数据当前长度
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public long GetLength(int id)
		{
			return RecvBuffer[id].Data.Length;
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
			byte[] buffer;
			int count;
			int i;
			while (Transmitter != null) //发送序列全空等待
			{
				count = 0;
				for (i = 0; i < SendBuffer.Length; ++i)
				{
					if (SendBuffer[i] == null)
					{
						++count;
						continue;
					}
					Pack.Buffer b = SendBuffer[i];
					Pack.Package p;
					for (int j = 0; j < b.Priority; ++j)
					{
						//判断
						if (!b.TagSended)
						{
							buffer = Json.WriteObject(b.Tag);
							b.TagSended = !b.TagSended;
						}
						else if (b.Data.CanRead && b.Data.Position != b.DataLength)
						{
							buffer = new byte[(int)Math.Min(b.DataLength - b.Data.Position, DefaultSettings.Value.PackageSize)];
							b.Data.Read(buffer, 0, buffer.Length);
						}
						else
						{
							SendBuffer[i].Dispose();
							SendBuffer[i] = null;
							break;
						}
						//加密
						p = Encoder.Encode(new Package((byte)i, (ushort)buffer.Length, buffer));
						//发送
						try
						{
							Transmitter?.SendPackage(p);
						}
						catch (Exception e)
						{
							OnExcepetionOccurred?.Invoke(this, new Args.SocketExceptionEventArgs(this.GetType(), e, true));
						}
						buffer = null;
					}
				}
				if (count == SendBuffer.Length)
					Wait.WaitOne();
			}

		}

		/// <summary>
		/// 响应接收信息的事件 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		private void Transmitter_OnPackageReceived(object sender, Args.PackageReceivedEventArgs args)
		{
			//解密
			Pack.Package p = Encoder.Decode(args.Value);
			if (RecvBuffer[p.Sequence] == null)
			{
				try
				{
					BufferTag tag = Json.ReadObject(p.Data);
					RecvBuffer[p.Sequence] = new Pack.Buffer(tag);
					OnStartReceiving?.Invoke(this, new Args.MessageStartReceivingEventArgs(tag.Name, tag.UserString, tag.DataLength, p.Sequence));
				}
				catch (Exception e)
				{
					OnExcepetionOccurred?.Invoke(this, new Args.SocketExceptionEventArgs(this.GetType(), e, true));
				}
			}
			else
			{
				var buffer = RecvBuffer[p.Sequence];
				buffer.Data.Write(p.Data, 0, p.Size);
			}
			if (RecvBuffer[p.Sequence] != null && RecvBuffer[p.Sequence].Data.Length == RecvBuffer[p.Sequence].DataLength)
			{
				OnBufferReceived?.Invoke(this, new Args.BufferReceivedEventArgs(RecvBuffer[p.Sequence].CheckPoint, RecvBuffer[p.Sequence], args.RemoteHostName, args.RemoteServiceName, args.LocalServiceName));
				RecvBuffer[p.Sequence] = null;
			}
		}
		/// <summary>
		/// 检查Buffer是否已过期
		/// </summary>
		/// <param name="Timer"></param>
		private void CheckForBadBuffer(ThreadPoolTimer Timer)
		{
			DateTime oldestTime = DateTime.Now - DefaultSettings.Value.MaxmumPackageDelay;
			lock (RecvBuffer)
			{
				for (int i = 0; i < RecvBuffer.Length; ++i)
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

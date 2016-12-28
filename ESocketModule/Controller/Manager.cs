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
using ESocket.Convert;

namespace ESocket.Controller
{
	/// <summary>
	/// 管理器负责操作Transmitter，实现并行收发，优先级控制
	/// 本类需要严格线程同步管理，保证主线程和异步发送线程无冲突
	/// </summary>
	class Manager : IManager
	{
		private object locker = new object();
		/// <summary>
		/// 用于操作的收发器
		/// </summary>
		private ITransmitter Transmitter;
		/// <summary>
		/// 缓冲区切片方法
		/// </summary>
		BufferSerializer Serializer;
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
		private List<KeyValuePair<UInt64, Pack.Buffer>> SendBuffer;
		/// <summary>
		/// 发送序列号
		/// </summary>
		private UInt64 SendSequence;
		/// <summary>
		/// 存储缓存的接收队列
		/// </summary>
		private List<KeyValuePair<UInt64, Pack.Buffer>> RecvBuffer;
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
			Serializer = new BufferSerializer();
			//初始化
			SendBuffer = new List<KeyValuePair<ulong, Pack.Buffer>>();
			RecvBuffer = new List<KeyValuePair<ulong, Pack.Buffer>>();
		}
		/// <summary>
		/// 添加Buffer
		/// </summary>
		public void AddBuffer(Pack.Buffer buffer)
		{
			SendBuffer.Add(new KeyValuePair<ulong, Pack.Buffer>(FindAvailableID(), buffer));
			Wait.Set();
		}
		
		/// <summary>
		/// 寻找一个可用的ID
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		private UInt64 FindAvailableID()
		{
			lock(locker)
			{
				SendSequence = (SendSequence + 1) % (1 << (DefaultSettings.LengthofSeqTag * 8));
			}
			if (SendBuffer.Exists(q => q.Key == SendSequence))
				return SendSequence;
			else
				return FindAvailableID();
		}
		/// <summary>
		/// 获取数据当前长度
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public long GetLength(ulong id)
		{
			return RecvBuffer.Find(q => q.Key == id).Value.Data.Length;
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
			int i;
			while (Transmitter != null) //发送序列全空等待
			{
				for (i = 0; i < SendBuffer.Count; ++i)
				{
					Pack.Package[] packs;
					bool complete = Serializer.ReadPackage(SendBuffer[i].Key, SendBuffer[i].Value, out packs);
					try
					{
						foreach (Pack.Package p in packs)
							Transmitter.SendPackage(p);
					}
					catch (Exception e)
					{
						OnExcepetionOccurred?.Invoke(this, new Args.SocketExceptionEventArgs(this.GetType(), e, true));
					}
					if (complete)
						SendBuffer.RemoveAt(i);
				}
				if (SendBuffer.Count == 0)
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
			bool complete = false;
			var val = RecvBuffer.FirstOrDefault(q => q.Key == args.Value.Sequence).Value;
			try
			{
				complete = Serializer.WritePackage(args.Value, ref val);
			}
			catch (Exception e)
			{
				OnExcepetionOccurred?.Invoke(this, new Args.SocketExceptionEventArgs(this.GetType(), e, true));
			}
			if (val == null)
			{
				RecvBuffer.Add(new KeyValuePair<ulong, Pack.Buffer>(args.Value.Sequence, val));
				OnStartReceiving?.Invoke(this, new Args.MessageStartReceivingEventArgs(val.Tag.Name, val.Tag.UserString, val.Tag.DataLength, args.Value.Sequence));
			}
			if(complete)
			{
				OnBufferReceived?.Invoke(this, new Args.BufferReceivedEventArgs(val.CheckPoint, val, args.RemoteHostName, args.RemoteServiceName, args.LocalServiceName));
				RecvBuffer.RemoveAt(RecvBuffer.FindIndex(q => q.Key == args.Value.Sequence));
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
				for (int i = 0; i < RecvBuffer.Count; ++i) 
				{
					if (RecvBuffer[i].Value.CheckPoint < oldestTime)
					{
						RecvBuffer[i].Value.Dispose();
						RecvBuffer.RemoveAt(i);
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

		public void Dispose()
		{
			Wait.Dispose();
		}
	}
}

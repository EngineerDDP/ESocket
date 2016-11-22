using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ESocket.Pack;

namespace ESocket.Transmitter
{
	/// <summary>
	/// 管理器负责操作Transmitter，实现并行收发，优先级控制
	/// </summary>
	class Manager
	{
		/// <summary>
		/// 用于操作的收发器
		/// </summary>
		private ITransmitter Transmitter;
		/// <summary>
		/// 存储缓存的发送队列
		/// </summary>
		private Dictionary<int, ESocket.Pack.Buffer> SendBuffer;
		private Dictionary<int, ESocket.Pack.Buffer> RecvBuffer;

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

		public event EventHandler<Args.BufferReceivedEventArgs> OnBufferReceived;
		/// <summary>
		/// 创建空的管理器
		/// </summary>
		public Manager()
		{
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
		/// 接收数据并填入缓冲
		/// </summary>
		private void Receive()
		{
			
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
		public async Task<Boolean> SetTransmitter(ITransmitter transmitter)
		{
			if (Transmitter == null)
			{
				Transmitter = transmitter;
				Transmitter.OnPackageReceived += Transmitter_OnPackageReceived;
				await Transmitter.StartAutoRecvAsync();
				return true;
			}
			else
				return false;
		}

		private void Transmitter_OnPackageReceived(object sender, Args.PackageReceivedEventArgs e)
		{
			if(RecvBuffer[e.Value.Sequence] == null)
			{
				RecvBuffer[e.Value.Sequence] = Pack.Buffer.CreateFromPackage(e.Value);
			}
			else
			{
				RecvBuffer[e.Value.Sequence].WritePackage(e.Value);
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

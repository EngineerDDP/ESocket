using System;
using System.Threading.Tasks;
using ESocket.Args;
using ESocket.Pack;

namespace ESocket.Transmitter
{
	interface IManager
	{
		/// <summary>
		/// 当收完消息后发生
		/// </summary>
		event EventHandler<BufferReceivedEventArgs> OnBufferReceived;
		/// <summary>
		/// 递交超时事件
		/// </summary>
		event EventHandler<ConnectionTimeoutEventArgs> OnConnectionTimeout;
		/// <summary>
		/// 当触发异常时生成
		/// </summary>
		event EventHandler<Args.SocketExceptionEventArgs> OnExcepetionOccurred;
		/// <summary>
		/// 添加Buffer
		/// </summary>
		/// <param name="buffer"></param>
		/// <returns></returns>
		bool AddBuffer(Pack.Buffer buffer);
		/// <summary>
		/// 开始接受数据
		/// </summary>
		/// <returns></returns>
		Task Init();
		/// <summary>
		/// 移除收发器，返回成功与否的标志
		/// </summary>
		/// <param name="transmitter"></param>
		/// <returns></returns>
		bool RemoveTransmitter();
		/// <summary>
		/// 设置收发器，返回成功与否的标志
		/// </summary>
		/// <param name="transmitter"></param>
		/// <returns></returns>
		bool SetTransmitter(ITransmitter transmitter);
	}
}
﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestProject1
{
	[TestClass]
	/// <summary>
	/// 用于交换数据的管道，负责即时写入即时读取的单向通讯
	/// </summary>
	class PipeStream
	{
		/// <summary>
		/// 线程锁
		/// </summary>
		private object locker;
		/// <summary>
		/// 用于存储管道数据
		/// </summary>
		private Byte[] Data;
		/// <summary>
		/// 写入点位置
		/// </summary>
		private Int32 ReadPos;
		/// <summary>
		/// 读取点位置
		/// </summary>
		private Int32 WritePos;
		/// <summary>
		/// 最大容量
		/// </summary>
		private Int32 Capacity;
		/// <summary>
		/// 数据长度
		/// </summary>
		private Int32 Length;
		/// <summary>
		/// 正在写入
		/// </summary>
		private bool IsWriting;
		/// <summary>
		/// 正在读取
		/// </summary>
		private bool IsReading;
		/// <summary>
		/// 使用指定的容量值创建管道流
		/// </summary>
		/// <param name="capacity"></param>
		public PipeStream(Int32 capacity)
		{
			locker = new object();
			ReadPos = 0;
			WritePos = 0;
			Capacity = capacity;
			Length = 0;
			IsWriting = false;
			IsReading = false;
			Data = new byte[Capacity];
		}
		private void SetLength(Int32 length)
		{
			lock (locker)
			{
				Length = length;
			}
		}
		/// <summary>
		/// 写入指定的字节串
		/// </summary>
		/// <param name="buffer">字节缓冲区</param>
		/// <param name="offset">起始位置</param>
		/// <param name="length">写入长度</param>
		public void WriteBytes(Byte[] buffer, int offset, int length)
		{
			if (length <= Capacity - Length)
			{
				IsWriting = true;
				int i;
				for (i = 0; i < length; ++i)
					Data[(i + WritePos) % Capacity] = buffer[i + offset];
				IsWriting = false;
				WritePos = (WritePos + length) % Capacity;
				SetLength(Length + length);
			}
			else
				throw new NotImplementedException("要写入的长度超出容量");
		}
		/// <summary>
		/// 读出指定长度的字节
		/// </summary>
		/// <param name="buffer">字节缓冲区</param>
		/// <param name="offset">起始位置</param>
		/// <param name="length">写入长度</param>
		public void ReadBytes(Byte[] buffer, int offset, int length)
		{
			if (Length >= length)
			{
				IsReading = true;
				int i;
				for (i = 0; i < length; ++i)
					buffer[i + offset] = Data[(i + ReadPos) % Capacity];
				IsReading = false;
				ReadPos = (ReadPos + length) % Capacity;
				SetLength(Length - length);
			}
			else
				throw new NotImplementedException("要读取的数据超出容量");
		}
	}
}

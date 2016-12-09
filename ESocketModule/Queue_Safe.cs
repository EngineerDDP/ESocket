using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESocket
{
	/// <summary>
	/// 仅支持出队与入队操作的线程安全的队列类
	/// </summary>
	/// <typeparam name="Type">使用指定类型参数实例化泛型</typeparam>
	internal class Queue_Safe<Type>
	{
		/// <summary>
		/// 队列内容
		/// </summary>
		private Queue<Type> que;
		/// <summary>
		/// 线程锁
		/// </summary>
		private object locker;
		private enum Operation
		{
			Enqueue, Dequeue
		}
		public Queue_Safe()
		{
			que = new Queue<Type>();
			locker = new object();
		}
		/// <summary>
		/// 队列操作
		/// </summary>
		/// <param name="o">操作类型枚举</param>
		/// <param name="item">输出值(Dequeue) 参数值(Enqueue)</param>
		private void Method(Operation o, ref Type item)
		{
			lock (locker)
			{
				switch (o)
				{
					case Operation.Enqueue:
						que.Enqueue(item);
						break;
					case Operation.Dequeue:
						item = que.Dequeue();
						break;
					default:
						break;
				}
			}
		}
		/// <summary>
		/// 取队首元素并将其删除
		/// </summary>
		/// <returns></returns>
		public Type Dequeue()
		{
			Type item = default(Type);
			Method(Operation.Dequeue, ref item);
			return item;
		}
		/// <summary>
		/// 将元素入队
		/// </summary>
		/// <param name="item"></param>
		public void Enqueue(Type item)
		{
			Method(Operation.Enqueue, ref item);
		}
		/// <summary>
		/// 查找是否存在满足条件的项
		/// </summary>
		/// <param name="func">条件</param>
		/// <returns></returns>
		public bool Any(Func<Type, bool> func)
		{
			return que.Any(func);
		}
		/// <summary>
		/// 获取队列长度
		/// </summary>
		public int Count
		{
			get
			{
				return que.Count;
			}
		}
	}
}

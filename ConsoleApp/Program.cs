using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp
{
	class Program
	{
		static void Main(string[] args)
		{
			List<MyClass> list = new List<MyClass>();
			MyClass c = list.FirstOrDefault(q => q.Key == 1);

			Console.WriteLine(c);
		}
	}
	public class MyClass
	{
		public int Key { get; set; }
	}
}

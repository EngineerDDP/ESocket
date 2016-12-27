using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestProject2
{
	[TestClass]
	public class UnitTest1
	{
		[TestMethod]
		public void TestMethod1()
		{
			List<MyClass> list = new List<MyClass>();

			Console.WriteLine(list.FirstOrDefault(q => q.Key == 1).GetType());
		}
	}
	public class MyClass
	{
		public int Key { get; set; }
	}
}

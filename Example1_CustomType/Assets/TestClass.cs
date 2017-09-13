using FF;
using UnityEngine;

#pragma warning disable 414

public class TestClass
{
	public static TestClass instance = new TestClass();
	public class MyData
	{
		public int a;
		public int b;
	}

	public ObjectDictionary<int, MyData> _objectDictionary = new ObjectDictionary<int, MyData>();
	public MyColor _color = new MyColor(1, 0, 0);

	public TestClass()
	{
		_objectDictionary.Add(10, new MyData());
		_objectDictionary.Add(20, new MyData());
	}
}

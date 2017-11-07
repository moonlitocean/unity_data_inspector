using System;
using DataInspector;

public class ObjectDictionaryVisualizer : CollectionVisualizerBase
{
	public override int Size(object collection)
	{
		return ((IObjectDictionary)collection).Count;
	}

	public override object[] Keys(object collection)
	{
		object[] keys = new object[((IObjectDictionary)collection).Count];
		int index = 0;
		foreach (var key in ((IObjectDictionary)collection).Keys)
		{
			keys[index++] = key;
		}
		return keys;
	}

	public override object Get(object collection, object key)
	{
		return ((IObjectDictionary)collection).GetElem(key);
	}

	public override void Set(object collection, object key, object value)
	{
		((IObjectDictionary) collection).SetElem(key, value);
	}

	// 可选。
	// 如果没有的话，容器不知道自己的元素类型，就不能直接构造新元素
	public override Type ValueType(object collection)
	{
		return TypeTools.FindGenericParamType(collection.GetType(), typeof(ObjectDictionary<,>), 1);
	}
}

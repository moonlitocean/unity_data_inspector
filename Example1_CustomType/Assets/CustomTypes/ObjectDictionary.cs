using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


public interface IObjectDictionary
{
	bool ContainsObjectKey(object key);
	void SetElem(object key, object value);

	object GetElem(object key);
	void Clear();
	int Count { get; }
	ICollection Keys { get; }
	ICollection Values { get; }
	IDictionaryEnumerator GetEnumerator();
}

/// <summary>
/// object值类型的Dictionary的一个简单封装版本。
/// XCode下，托管的dll在编译过程中的中间文件dll.o有44MB的大小限制，
/// 其中很大一部分是由于Dictionary在AOT编译条件下对具体的泛型类型进行了展开，
/// 为了减小dll的体积，在此封装了一个简单接口版本的Dictionary
/// </summary>

[Serializable]
public class ObjectDictionary<TK, TV> : IObjectDictionary where TV : class
{
	private readonly Dictionary<TK, object> dict;

	public ObjectDictionary()
	{
		dict = new Dictionary<TK, object>();
	}

	public ObjectDictionary(IDictionary<TK, object> src)
	{
		dict = new Dictionary<TK, object>(src);
	}

	public ObjectDictionary(IEqualityComparer<TK> comparer)
	{
		dict = new Dictionary<TK, object>(comparer);
	}

	public ObjectDictionary(int size, IEqualityComparer<TK> comparer)
	{
		dict = new Dictionary<TK, object>(size, comparer);
	}

	public bool ContainsKey(TK key)
	{
		return dict.ContainsKey(key);
	}

	public TV this[TK key]
	{
		get { return dict.ContainsKey(key) ? dict[key] as TV : default(TV); }
	}

	public IEnumerable<TV> Values
	{
		get { return dict.Values.Select(v => v as TV); }
	}

	public IEnumerable<TK> Keys
	{
		get { return dict.Keys; }
	}

	public int Count
	{
		get { return dict.Keys.Count; }
	}

	public void Add(TK key, TV value)
	{
		dict.Add(key, value);
	}

	////////////////////////////////////////////////////////
	/// Implementation of IObjectDictionary
	public bool ContainsObjectKey(object key)
	{
		return GetDictionary().Contains(key);
	}

	public void SetElem(object key, object value)
	{
		GetDictionary()[key] = value;
	}

	public object GetElem(object key)
	{
		return GetDictionary()[key];
	}

	public object GetElem(Predicate<object> pred)
	{
		var dict = GetDictionary();
		var iter = dict.Values.GetEnumerator();
		while (iter.MoveNext())
		{
			var item = iter.Current;
			if (pred(item))
				return item;
		}
		return null;
	}

	public void Clear()
	{
		GetDictionary().Clear();
	}

	ICollection IObjectDictionary.Keys
	{
		get { return GetDictionary().Keys; }
	}

	ICollection IObjectDictionary.Values
	{
		get { return GetDictionary().Values; }
	}

	IDictionaryEnumerator IObjectDictionary.GetEnumerator()
	{
		return (dict as IDictionary).GetEnumerator();
	}

	private IDictionary GetDictionary()
	{
		return dict;
	}
}
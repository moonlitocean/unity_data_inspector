using System;
using System.Collections;

namespace DataInspector
{
	internal class ArrayVisualizer : CollectionVisualizerBase
	{
		public override bool HasCustomCreator(Type type, IMark mark)
		{
			return true;
		}

		public override object CustomCreateInstance(Type type, IMark mark)
		{
			return Array.CreateInstance(type.GetElementType(), new int[type.GetArrayRank()]);
		}

		public override int Size(object collection)
		{
			return ((ICollection)collection).Count;
		}

		public override object[] Keys(object collection)
		{
			object[] keys = new object[((ICollection)collection).Count];
			for (int i = 0; i < keys.Length; ++i)
				keys[i] = i;
			return keys;
		}

		public override object Get(object collection, object key)
		{
			return ((IList)collection)[(int)key];
		}

		public override void Set(object collection, object key, object value)
		{
			((IList)collection)[(int)key] = value;
		}

		public override Type ValueType(object collection)
		{
			return collection.GetType().GetElementType();
		}

		public override bool Resizable(object collection)
		{
			return true;
		}

		public override object Resize(object collection, int size)
		{
			return TypeTools.ResizeArray(collection as Array, size);
		}
	}
}
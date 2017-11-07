using System;
using System.Collections;
using System.Collections.Generic;

namespace DataInspector
{
	internal class ListVisualizer : CollectionVisualizerBase
	{
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
			return ((IList) collection)[(int) key];
		}

		public override void Set(object collection, object key, object value)
		{
			((IList)collection)[(int)key] = value;
		}

		public override Type ValueType(object collection)
		{
			return TypeTools.FindGenericParamType(collection.GetType(), typeof(List<>), 0);
		}

		public override bool Resizable(object collection)
		{
			return true;
		}

		public override object Resize(object collection, int size)
		{
			IList list = (IList)collection;

			var elemType = ValueType(collection);
			while (list.Count > size)
			{
				list.RemoveAt(list.Count - 1);
			}
			while (list.Count < size)
			{
				if (elemType.IsClass)
					list.Add(null);
				else
					list.Add(TypeTools.CreateInstance(elemType));
			}
			return list;
		}
	}
}
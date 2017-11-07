using System;
using System.Collections;

namespace DataInspector
{
	internal class ListVisualizer : BaseContainerVisualizer
	{
		public override bool HasCustomCreator(Type type, IMark mark)
		{
			return type.IsArray;
		}

		public override object CustomCreateInstance(Type type, IMark mark)
		{
			if (type.IsArray)
				return Array.CreateInstance(type.GetElementType(), new int[type.GetArrayRank()]);

			throw new NotImplementedException(type.ToString());
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
			return ((IList) collection)[(int) key];
		}

		public override bool Set(object collection, object key, object value)
		{
			bool changed = ((IList)collection)[(int)key] != value;
			((IList)collection)[(int)key] = value;
			return changed;
		}

		public override Type ValueType(object collection)
		{
			var type = collection.GetType();
			if (TypeTools.IsSubclassOfList(type))
				return TypeTools.GetListValueType(type);
			else if (type.IsArray)
				return type.GetElementType();
			else
				return null;
		}

		public override bool Resizable(object collection)
		{
			return true;
		}

		public override object Resize(object collection, int size)
		{
			IList list = (IList)collection;
			var elemType = ValueType(collection);

			if (collection.GetType().IsArray)
			{
				return TypeTools.ResizeArray(collection as Array, size);
			}
			else
			{
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
}
using System;
using System.Collections;

namespace DataInspector
{
	internal class DictionaryVisualizer : BaseContainerVisualizer
	{
		public override int Size(object collection)
		{
			return ((ICollection)collection).Count;
		}

		public override object[] Keys(object collection)
		{
			object[] keys = new object[((IDictionary)collection).Count];
			int index = 0;
			foreach (var key in ((IDictionary)collection).Keys)
			{
				keys[index++] = key;
			}
			return keys;
		}

		public override object Get(object collection, object key)
		{
			return ((IDictionary)collection)[key];
		}

		public override bool Set(object collection, object key, object value)
		{
			bool changed = false;
			if (!((IDictionary)collection).Contains(key))
			{
				changed = true;
			}
			else
			{
				if (((IDictionary)collection)[key] != value)
					changed = true;
			}

			((IDictionary)collection)[key] = value;
			return changed;
		}

		public override Type ValueType(object collection)
		{
			var type = collection.GetType();
			if (TypeTools.IsSubclassOfDictionary(type))
				return TypeTools.GetDictionaryValueType(type);
			else
				return typeof(object);
		}
	}
}
using System;
using System.Collections;
using System.Collections.Generic;

namespace DataInspector
{
	internal class DictionaryVisualizer : CollectionVisualizerBase
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
			Array.Sort(keys);
			return keys;
		}

		public override object Get(object collection, object key)
		{
			return ((IDictionary)collection)[key];
		}

		public override void Set(object collection, object key, object value)
		{
			((IDictionary)collection)[key] = value;
		}

		public override Type ValueType(object collection)
		{
			return TypeTools.FindGenericParamType(collection.GetType(), typeof(Dictionary<,>), 1);
		}
	}
}
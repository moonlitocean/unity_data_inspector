using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DataInspector
{
	internal class StaticVisualizer : CollectionVisualizerBase
	{
		private class FieldData
		{
			public string[] fieldNames;
			public Dictionary<string, FieldInfo> fields;
		}
		private static readonly Dictionary<Type, FieldData> cachedFields = new Dictionary<Type, FieldData>();

		private static FieldData GetFieldInfos(Type type)
		{
			if (!cachedFields.ContainsKey(type))
			{
				var data = new FieldData();
				var fieldInfos = type.GetFields(BindingFlags.Static | BindingFlags.Public).ToArray();

				data.fields = new Dictionary<string, FieldInfo>(fieldInfos.Length);
				data.fieldNames = fieldInfos.Select(o => o.Name).ToArray();
				foreach (var field in fieldInfos)
					data.fields[field.Name] = field;

				cachedFields.Add(type, data);
			}
			return cachedFields[type];
		}

		public override int Size(object collection)
		{
			return GetFieldInfos((Type)collection).fieldNames.Length;
		}

		public override object[] Keys(object collection)
		{
			return GetFieldInfos((Type)collection).fieldNames;
		}

		public override object Get(object collection, object key)
		{
			return GetFieldInfos((Type)collection).fields[(string)key].GetValue(null);
		}

		public override void Set(object collection, object key, object value)
		{
			GetFieldInfos((Type)collection).fields[(string)key].SetValue(null, value);
		}
	}
}
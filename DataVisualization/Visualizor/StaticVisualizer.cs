using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DataTools;

internal class StaticVisualizer : DataVisualizer
{
	private class FieldData
	{
		public FieldInfo field;
		public IMark mark;
	}
	private static readonly Dictionary<Type, FieldData[]> cachedFields = new Dictionary<Type, FieldData[]>();

	private static FieldData[] GetFieldInfos(Type type)
	{
		if (!cachedFields.ContainsKey(type))
		{
			var fieldInfos = type.GetFields(BindingFlags.Static | BindingFlags.Public).OrderBy(o => o.Name).ToArray();
			var data = new FieldData[fieldInfos.Length];

			for (int i = 0; i < fieldInfos.Length; i++)
			{
				data[i] = new FieldData
				{
					field = fieldInfos[i],
					mark = TypeTools.GetAttribute<IMark>(fieldInfos[i])
				};
			}
			cachedFields.Add(type, data);
		}
		return cachedFields[type];
	}

	public override bool HasChildren()
	{
		return true;
	}

	public override bool InspectChildren(DataVisualization visualization, string path, ref object data, Type type)
	{
		var dataAsType = data as Type;
		if (dataAsType == null)
			return false;

		bool changed = false;
		foreach (FieldData field in GetFieldInfos(dataAsType))
		{
			changed |= InspectField(visualization, path + "." + field.field.Name, ref data, field);
		}
		return changed;
	}

	private static bool InspectField(DataVisualization visualization, string path, ref object data, FieldData fieldInfo)
	{
		bool changed = false;
		object newValue = null;
		visualization.Inspect(fieldInfo.field.Name, path, fieldInfo.field.GetValue(null), fieldInfo.field.FieldType,
			fieldInfo.mark, v =>
			{
				changed = true;
				newValue = v;
			});

		if (changed)
			fieldInfo.field.SetValue(data, newValue);
		return changed;
	}
}
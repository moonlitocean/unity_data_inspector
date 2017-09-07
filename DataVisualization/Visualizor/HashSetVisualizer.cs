using System;
using System.Collections;
using DataTools;
using UnityEditor;

internal class HashSetVisualizer : DataVisualizer
{
	public override bool HasChildren()
	{
		return true;
	}

	public override bool InspectSelf(string name, ref object data, Type type)
	{
		IEnumerable container = data as IEnumerable;
		if (container == null)
			return false;

		EditorGUILayout.LabelField("Count: ", GetCount(data).ToString());
		return false;
	}

	public override bool InspectChildren(DataVisualization visualization, string path, ref object data, Type type)
	{
		var list = data as IEnumerable;
		if (list == null)
			return false;

		Type hashsetValueType = null;
		if (TypeTools.IsSubclassOfHashSet(type))
			hashsetValueType = TypeTools.GetHashSetValueType(type);

		int i = 0;
		bool changed = false;
		foreach (var value in list)
		{
			Type valueType = value != null ? value.GetType() : hashsetValueType;
			changed |= visualization.Inspect(i.ToString(), path + "." + i, value, valueType);
			++i;
		}
		return changed;
	}

	private static int GetCount(object data)
	{
		return data.GetValueEx<int>("Count");
	}
}
using System;
using System.Collections;
using FF;
using UnityEditor;

internal class HashSetVisualizor : DataVisualizor
{
	public override bool IsFoldable()
	{
		return true;
	}

	public override bool HasContent(object data, Type type)
	{
		IEnumerable container = data as IEnumerable;
		if (container == null)
			return false;

		return GetCount(data) > 0;
	}

	public override bool InspectRoot(string name, ref object data, Type type)
	{
		IEnumerable container = data as IEnumerable;
		if (container == null)
			return false;

		EditorGUILayout.LabelField("Count: ", GetCount(data).ToString());
		return false;
	}

	public override bool InspectContent(DataVisualization visualization, string path, ref object data, Type type)
	{
		IEnumerable list = data as IEnumerable;
		if (list == null)
			return false;

		Type hashsetValueType = null;
		if (TypeUtil.IsSubclassOfHashSet(type))
			hashsetValueType = TypeUtil.GetHashSetValueType(type);

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
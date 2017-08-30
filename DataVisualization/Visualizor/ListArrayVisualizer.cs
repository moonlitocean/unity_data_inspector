using System;
using System.Collections;
using FF;
using UnityEditor;

internal class ListArrayVisualizer : DataVisualizer
{
	public override bool HasChildren()
	{
		return true;
	}

	public override bool InspectSelf(string name, ref object data, Type type)
	{
		var list = data as IList;
		if (list == null)
			return false;

		EditorGUILayout.LabelField("Size:" + list.Count);
		return false;
	}

	public override bool InspectChildren(DataVisualization visualization, string path, ref object data, Type type)
	{
		var list = data as IList;
		if (list == null)
			return false;

		int newCount = EditorGUILayout.DelayedIntField("Size", list.Count);
		newCount = Math.Max(0, newCount);
		if (newCount != list.Count)
		{
			data = ResizeList(list, newCount);
			return true;
		}

		Type listValueType = null;
		if (TypeTools.IsSubclassOfList(type))
			listValueType = TypeTools.GetListValueType(type);

		bool changed = false;
		for (int index = 0; index < list.Count; ++index)
		{
			int curindex = index;
			object value = list[index];
			Type valueType = value != null ? value.GetType() : listValueType;
		    string extraTypeInfo = string.Empty;
		    if (valueType != listValueType)
		    {
		        extraTypeInfo = string.Format("   ({0})", valueType.Name);
		    }
            changed |= visualization.Inspect("Element " + index + extraTypeInfo, path + "." + index, value, valueType, null, v => list[curindex] = v);
		}
		return changed;
	}

	private IList ResizeList(IList list, int newCount)
	{
		if (TypeTools.IsSubclassOfList(list.GetType()))
		{
			TypeTools.ResizeGenericList(list, newCount);
			return list;
		}
		else if (list.GetType().IsArray)
		{
			return TypeTools.ResizeArray(list as Array, newCount);
		}
		else
		{
			return list;
		}
	}
}
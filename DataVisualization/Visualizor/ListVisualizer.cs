﻿using System;
using System.Collections;
using DataTools;
using UnityEditor;

internal class ListVisualizer : DataVisualizer
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

		EditorGUILayout.LabelField("Count: " + list.Count);
		return false;
	}

	public override bool InspectChildren(DataVisualization visualization, string path, ref object data, Type type)
	{
		var list = data as IList;
		if (list == null)
			return false;

		Type listValueType = GetElemType(list);
		int newCount = EditorGUILayout.DelayedIntField("Size", list.Count);
		newCount = Math.Max(0, newCount);
		if (newCount != list.Count)
		{
			data = Resize(list, newCount, listValueType);
			return true;
		}

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

	protected IList Resize(IList list, int newCount, Type elemType)
	{
		if (list.GetType().IsArray)
		{
			return TypeTools.ResizeArray(list as Array, newCount);
		}
		else
		{
			while (list.Count > newCount)
			{
				list.RemoveAt(list.Count - 1);
			}
			while (list.Count < newCount)
			{
				list.Add(TypeTools.CreateInstance(elemType));
			}
			return list;
		}
	}

	protected virtual Type GetElemType(IList list)
	{
		var t = list.GetType();
		if (t.IsArray)
			return t.GetElementType();
		else
			return TypeTools.GetListValueType(t);
	}
}
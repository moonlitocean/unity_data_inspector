using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

internal class CallableMethodVisualizor : DataVisualizor
{
	public override bool IsFoldable()
	{
		return true;
	}

	public override bool  HasContent(object data, Type type)
	{
		InspectorMethod method = data as InspectorMethod;
		if (method == null)
			return false;

		return method.method.GetParameters().Length > 0;
	}

	public override bool InspectRoot(string name, ref object data, Type type)
	{
		InspectorMethod method = data as InspectorMethod;
		if (method == null)
			return false;

		EditorGUILayout.LabelField(method.name);
		bool invoke = GUILayout.Button("Send");

		if (invoke)
		{
			method.Invoke();
		}
		return false;
	}

	public override bool InspectContent(DataVisualization visualization, string path, ref object data, Type type)
	{
		InspectorMethod method = data as InspectorMethod;
		if (method == null)
			return false;

		ParameterInfo[] paramList = method.method.GetParameters();
		for (int index = 0; index < paramList.Length; ++index)
		{
			int curIndex = index;
			ParameterInfo paramInfo = paramList[index];
			visualization.Inspect(paramInfo.Name,
				path + "." + paramInfo.Name,
				method.param[index], paramInfo.ParameterType, null, v => method.param[curIndex] = v);
		}

		return false;
	}
}
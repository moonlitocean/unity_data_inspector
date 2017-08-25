using System;
using System.Globalization;
using Unique;
using UnityEditor;
using UnityEngine;

internal class AttributeVariableVisualizor : DataVisualizor
{
	public override bool IsFoldable()
	{
		return true;
	}

	public override bool InspectRoot(string name, ref object data, Type type)
	{
		AttributeVariable av = (AttributeVariable)data;
		GUILayout.Label(av.Value64.ToString(CultureInfo.InvariantCulture), GUILayout.Width(100f));
		av.WatchChanged = GUILayout.Toggle(av.WatchChanged, "Watch", GUILayout.Width(100f));
		return false;
	}

	public override bool InspectContent(DataVisualization visualization, string path, ref object data, Type type)
	{
		var av = data as AttributeVariable;
		bool changed = false;
		if (av != null)
		{
			EditorGUILayout.LabelField("Basic * Relative + Absolute");
			changed |= visualization.Inspect("Basic", path + ".Basic", av.Basic, null, null, v => av.Basic = (float) v);
			changed |= visualization.Inspect("Relative", path + ".Relative", av.Relative, null, null, v => av.Relative = (float)v);
			changed |= visualization.Inspect("Absolute", path + ".Absolute", av.Absolute, null, null, v => av.Absolute = (float)v);
		}
		return changed;
	}
}

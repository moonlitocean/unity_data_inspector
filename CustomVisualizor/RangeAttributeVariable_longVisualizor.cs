using System.Globalization;
using UnityEngine;
using System;
using System.Reflection;
using Unique;

internal class RangeAttributeVariable_longVisualizor : DataVisualizor
{
	public override bool IsFoldable()
	{
		return true;
	}

	public override bool InspectRoot(string name, ref object data, Type type)
	{
		var rav = data as RangeAttributeVariable_long;
		if (rav != null)
		{
			GUILayout.Label(rav.Value.ToString(CultureInfo.InvariantCulture));	
		}
		return false;
	}

	public override bool InspectContent(DataVisualization visualization, string path, ref object data, Type type)
	{
		var rav = data as RangeAttributeVariable_long;
		bool changed = false;
		if (rav != null)
		{
			var min = type.GetField("_min", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
			if (min != null)
			{
				object value = min.GetValue(data);
				changed |= visualization.Inspect("Min", path + ".Min", value, typeof (AttributeVariable), null,
					o => rav.Value = rav.Value);
			}

			var max = type.GetField("_max", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
			if (max != null)
			{
				object value = max.GetValue(data);
				changed |= visualization.Inspect("Max", path + ".Max", value, typeof (AttributeVariable), null,
					o => rav.Value = rav.Value);
			}
		}
		return changed;
	}
}
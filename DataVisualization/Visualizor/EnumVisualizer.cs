using System;
using DataTools;
using UnityEditor;

internal class EnumVisualizer : DataVisualizer
{
	public override bool HasChildren()
	{
		return false;
	}

    public override bool InspectSelf(DataVisualization visualization, string name, ref object data, Type type)
    {
        var e = data as Enum;
        if (e == null)
            return false;
        if (TypeTools.GetAttribute<FlagsAttribute>(type) != null)
            return ApplyValueIfNotEqual(ref data, EditorGUILayout.EnumMaskField(name, e));
        else
	        return ApplyValueIfNotEqual(ref data, EditorGUILayout.EnumPopup(name, e));
    }
}
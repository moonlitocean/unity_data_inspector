using System;
using UnityEditor;
using UnityEngine;

internal class TimeSpanVisualizer : DataVisualizer
{
	public override bool HasChildren()
	{
		return false;
	}

    public override bool InspectSelf(string name, ref object data, Type type)
    {
        var ts = (TimeSpan) data;
        int hour = ts.Hours;
        int min = ts.Minutes;
        int sec = ts.Seconds;
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label(name);
        GUILayout.FlexibleSpace();
        int nh = EditorGUILayout.IntField(hour);
        GUILayout.Label(":");
        int nm = EditorGUILayout.IntField(min);
        GUILayout.Label(":");
        int ns = EditorGUILayout.IntField(sec);
        EditorGUILayout.EndHorizontal();
        if (nh != hour || nm != min || ns != sec)
        {
            return ApplyValueTypeData(ref data, new TimeSpan(nh, nm, ns));
        }
	    return false;
    }
}
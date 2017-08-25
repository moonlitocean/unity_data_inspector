using System;
using FF;
using UnityEditor;
using UnityEngine;

internal class TimeSpanVisualizor : DataVisualizor
{
	public override bool IsFoldable()
	{
		return false;
	}

    public override bool InspectRoot(string name, ref object data, Type type)
    {
        TimeSpan ts = (TimeSpan) data;
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
            return ApplyData(ref data, new TimeSpan(nh, nm, ns));
        }
	    return false;
    }
}
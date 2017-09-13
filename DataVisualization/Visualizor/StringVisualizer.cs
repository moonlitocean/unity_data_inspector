using System;
using UnityEditor;

internal class StringVisualizer : DataVisualizer
{
	public override bool HasCustomCreator(Type type, DataTools.IMark mark)
	{
		return true;
	}

	public override object CustomCreateInstance(Type type, DataTools.IMark mark)
	{
		return "";
	}

	public override bool InspectSelf(DataVisualization visualization, string name, ref object data, Type type)
	{
		if (data == null)
		{
			EditorGUILayout.LabelField(name, "null");
			return false;
		}
		
		return ApplyValueIfNotEqual(ref data, EditorGUILayout.TextField(name, (string)data));
	}
}
using System;
using DataTools;
using UnityEditor;

public class ObjectDictionaryVisualizer : DataVisualizer
{
	public override bool HasChildren()
	{
		return true;
	}

	public override bool InspectSelf(DataVisualization visualization, string name, ref object data, Type type)
	{
		var dictionary = data as IObjectDictionary;
		if (dictionary == null)
			return false;
			
		EditorGUILayout.LabelField("Count: " + dictionary.Count);
		return false;
	}

	// TODO: Implement InspectChildren by refectoring GUIContainerTools to support dictionary-like data
}

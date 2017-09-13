using System;
using DataInspector;
using UnityEditor;

public class ObjectDictionaryVisualizer : VisualizerBase
{
	public override bool HasChildren()
	{
		return true;
	}

	public override bool InspectSelf(Inspector inspector, string name, ref object data, Type type)
	{
		var dictionary = data as IObjectDictionary;
		if (dictionary == null)
			return false;
			
		EditorGUILayout.LabelField("Count: " + dictionary.Count);
		return false;
	}

	// TODO: Implement InspectChildren by refectoring GUIContainerTools to support dictionary-like data
}

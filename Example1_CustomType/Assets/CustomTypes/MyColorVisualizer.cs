using System;
using DataInspector;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

internal class MyColorVisualizer : VisualizerBase
{
	// 1. I want the standard Unity color picker as the main inspector
	//    Note the code must run in both Editor & None editor envionment

	public override bool InspectSelf(Inspector inspector, string name, ref object data, Type type)
	{
		// Check if in editor
		if (GUITools.IsInEditor())
		{
#if UNITY_EDITOR
			MyColor income = EditorGUILayout.ColorField(name, (MyColor) data);
			return ApplyValueIfNotEqual(ref data, income);
#else
			throw new NotImplementedException();
#endif
		}
		else
		{
			Color c = (MyColor) data;
			GUITools.LabelField(name, c.ToString());
			return false;
		}
	}

	// 2. I also want to display the detailed values of color (r,g,b,a), so I set HasChildren() as true
	public override bool HasChildren()
	{
		return true;
	}

	// 3. And I dont like the foldout on root. I want children always displayed.
	public override bool AlwaysShowChildren()
	{
		return true;
	}

	// 4. Finally, I dont want to implement (r,g,b,a) display by myself.
	//  The default implementation of compositeVisualizer is good, so I get the visualizer and called it.
	public override bool InspectChildren(Inspector inspector, string path, ref object data, Type type)
	{
		var compositeVisualizer = inspector.GetSpecialVisualizer(Inspector.SpecialVisualizer.Composite);
		return compositeVisualizer.InspectChildren(inspector, path, ref data, type);
	}
}
using System;
using DataTools;
using UnityEditor;

internal class MyColorVisualizer : DataVisualizer
{
	// 1. I want the standard Unity color picker as the main inspector
	public override bool InspectSelf(DataVisualization visualization, string name, ref object data, Type type)
	{
		MyColor income = EditorGUILayout.ColorField(name, (MyColor)data);
		return ApplyValueIfNotEqual(ref data, income);
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
	public override bool InspectChildren(DataVisualization visualization, string path, ref object data, Type type)
	{
		var compositeVisualizer = visualization.GetSpecialVisualizer(DataVisualization.SpecialVisualizer.Composite);
		return compositeVisualizer.InspectChildren(visualization, path, ref data, type);
	}
}
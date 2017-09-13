using DataTools.Editor;
using UnityEditor;
using UnityEngine;

public class Watch : EditorWindow
{
	private readonly DataVisualization v = new DataVisualization();
	private Vector2 scroll;

	[MenuItem("Window/Watch")]
	public static void ShowWindow()
	{
		GetWindow<Watch>();
	}

	public void OnGUI()
	{
		using (GUITools.Scroll(ref scroll))
		{
			v.Inspect("Inspector Options", "options", v.options);
			v.Inspect("Selection", "selection", Selection.objects);
			v.Inspect("Watch Window", "Watch Window", this);
			v.Inspect("TestClass", "TestClass", typeof(TestClass));
		} 
	}

	public void OnSelectionChange()
	{
		Repaint();
	}
}


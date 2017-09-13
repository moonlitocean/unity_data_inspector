using DataInspector;
using UnityEditor;
using UnityEngine;

public class Watch : EditorWindow
{
	private readonly Inspector v;
	private Vector2 scroll;

	public Watch()
	{
		// The most safe place to initialize v is here, expecially when you need to call member functions to setup v.
		// Awake() may not be called after recompile.
		v = new Inspector();
	}

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


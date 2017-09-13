using Assets.CustomInpectors;
using DataTools.Editor;
using FF;
using UnityEditor;
using UnityEngine;

public class Watch : EditorWindow
{
	private readonly DataVisualization v;
	private Vector2 scroll;

	public Watch()
	{
		// Note: Awake() will be called after Unity recompile.
		//	The most safe place to setup v is here.

		v = new DataVisualization();
		v.SetVisualizer(typeof(MyColor), new MyColorVisualizer());
		v.SetVisualizer(typeof(ObjectDictionary<,>), new ObjectDictionaryVisualizer());
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
			v.Inspect("TestClass", "TestClass", typeof(TestClass));
		}
	}
}


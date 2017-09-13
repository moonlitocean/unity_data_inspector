using DataInspector;
using UnityEditor;
using UnityEngine;

public class Watch : EditorWindow
{
	private readonly Inspector inspector;
	private Vector2 scroll;

	public Watch()
	{
		// Note: Awake() will be called after Unity recompile.
		//	The most safe place to setup v is here.

		inspector = new Inspector();
		inspector.SetVisualizer(typeof(MyColor), new MyColorVisualizer());
		inspector.SetVisualizer(typeof(ObjectDictionary<,>), new ObjectDictionaryVisualizer());
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
			inspector.Inspect("Inspector Options", "options", inspector.options);
			inspector.Inspect("TestClass", "TestClass", typeof(TestClass));
		}
	}
}


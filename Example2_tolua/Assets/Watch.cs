using DataInspector;
using LuaInterface;
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

		// Open extension. Only need open once
		Inspector.OpenToLua();
		
		inspector = new Inspector();
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
			inspector.Inspect("TestClass.state._data", "TestClass.state._data", TestClass.state.GetTable("_data"));
		}
	}
}


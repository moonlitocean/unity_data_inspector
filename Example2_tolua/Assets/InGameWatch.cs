using DataInspector;
using UnityEngine;

public class InGameWatch : MonoBehaviour
{
	private readonly Inspector v = new Inspector();
	private Vector2 scroll;

	public void Awake()
	{
		Inspector.OpenToLua();
	}

	public void OnGUI()
	{
		GUILayout.BeginVertical(GUILayout.Width(Screen.width));
		using (GUITools.Scroll(ref scroll))
		{
			v.Inspect("Inspector Options", "options", v.options);
			v.Inspect("TestClass", "TestClass", TestClass.state.GetTable("_data"));
		}
		GUILayout.EndVertical();
	}
}


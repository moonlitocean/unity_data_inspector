using DataInspector;
using UnityEngine;

public class InGameWatch : MonoBehaviour
{
	private readonly Inspector v = new Inspector();
	private Vector2 scroll;

	public void OnGUI()
	{
		GUILayout.BeginVertical(GUILayout.Width(Screen.width));
		using (GUITools.Scroll(ref scroll))
		{
			v.Inspect("Inspector Options", "options", v.options);
			v.Inspect("Watch Window", "Watch Window", this);
			v.Inspect("TestClass", "TestClass", typeof(TestClass));
		}
		GUILayout.EndVertical();
	}
}


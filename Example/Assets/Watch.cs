using FF.Editor;
using UnityEditor;
using UnityEngine;
#pragma warning disable 169

namespace Assets
{
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
				v.Inspect("Test", "Test", typeof(Test));
			} 
		}

		public void OnSelectionChange()
		{
			Repaint();
		}
	}
}

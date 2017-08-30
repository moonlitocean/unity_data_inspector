using System;
using FF;
using FF.Editor;
using Metadata;
using UnityEditor;
using UnityEngine;

internal class LocaleStringVisualizor : DataVisualizor
{
	public override bool IsFoldable()
	{
		return true;
	}

	public override bool HasCustomCreator(IEditorMark mark)
	{
		var locale = mark as LocaleAttribute;
		if (locale != null)
		{
			return locale.editorFormat != null;
		}
		return false;
	}

	public override object CustomCreateInstance(IEditorMark mark)
	{
		var locale = mark as LocaleAttribute;
		if (locale != null)
		{
			return locale.editorFormat;
		}
		return null;
	}

	public override bool InspectRoot(string name, ref object data, Type type)
	{
		if (data != null)
		{
			var path = (string) data;
			if (Locale.Has(path))
			{
				EditorGUILayout.LabelField(Locale.L(path));
			}
			else
			{
				using (GUITools.Color(Color.yellow))
					EditorGUILayout.LabelField(path);
			}
		}
		return false;
	}

	public override bool InspectContent(DataVisualization visualization, string path, ref object data, Type type)
	{
		return ApplyData(ref data, EditorGUILayout.TextField("Locale Path", (string) data));
	}
}
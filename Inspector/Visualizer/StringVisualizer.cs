using System;
using UnityEditor;

namespace DataInspector
{
	internal class StringVisualizer : VisualizerBase
	{
		public override bool HasCustomCreator(Type type, IMark mark)
		{
			return true;
		}

		public override object CustomCreateInstance(Type type, IMark mark)
		{
			return "";
		}

		public override bool InspectSelf(Inspector inspector, string name, ref object data, Type type)
		{
			if (data == null)
			{
				EditorGUILayout.LabelField(name, "null");
				return false;
			}

			return ApplyValueIfNotEqual(ref data, EditorGUILayout.TextField(name, (string) data));
		}
	}
}
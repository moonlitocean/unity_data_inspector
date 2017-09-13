using System;
using System.Collections;
using UnityEditor;

namespace DataInspector
{
	internal class HashSetVisualizer : VisualizerBase
	{
		public override bool HasChildren()
		{
			return true;
		}

		public override bool InspectSelf(Inspector inspector, string name, ref object data, Type type)
		{
			var container = data as IEnumerable;
			if (container == null)
				return false;

			EditorGUILayout.LabelField("Count: " + GetCount(data));
			return false;
		}

		public override bool InspectChildren(Inspector inspector, string path, ref object data, Type type)
		{
			var c = data as IEnumerable;
			if (c == null)
				return false;

			Type hashsetValueType = null;
			if (TypeTools.IsSubclassOfHashSet(type))
				hashsetValueType = TypeTools.GetHashSetValueType(type);

			int i = 0;
			bool changed = false;
			foreach (var value in c)
			{
				Type valueType = value != null ? value.GetType() : hashsetValueType;
				changed |= inspector.Inspect(i.ToString(), path + "." + i, value, valueType);
				++i;
			}
			return changed;
		}

		private static int GetCount(object data)
		{
			return data.GetValueEx<int>("Count");
		}
	}
}
using System;
using System.Collections;
using UnityEditor;

namespace DataInspector
{
	internal class DictionaryVisualizer : VisualizerBase
	{
		public override bool HasChildren()
		{
			return true;
		}

		public override bool InspectSelf(Inspector inspector, string name, ref object data, Type type)
		{
			var dictionary = data as IDictionary;
			if (dictionary == null)
				return false;

			EditorGUILayout.LabelField("Count: " + dictionary.Count);
			return false;
		}

		public override bool InspectChildren(Inspector inspector, string path, ref object data, Type type)
		{
			var dictionary = data as IDictionary;
			if (dictionary == null)
				return false;

			Type dictionaryValueType = null;
			if (TypeTools.IsSubclassOfDictionary(type))
				dictionaryValueType = TypeTools.GetDictionaryValueType(type);

			return GUIContainerTools.EditElems(dictionary, EditElem(inspector, path, dictionary, dictionaryValueType),
				inspector.isFoldout,
				path + "[fold]");
		}

		private static Func<object, object, bool> EditElem(Inspector inspector, string path,
			IDictionary dictionary, Type dictionaryValueType)
		{
			return (key, value) =>
			{
				var valueType = value != null ? value.GetType() : dictionaryValueType;
				return inspector.Inspect(key.ToString(), path + "." + key, value, valueType, null, v => dictionary[key] = v);
			};
		}
	}
}
using System;
using DataTools;
using FF;
using UnityEditor;

namespace Assets.CustomInpectors
{
	public class ObjectDictionaryVisualizer : DataVisualizer
	{
		public override bool HasChildren()
		{
			return true;
		}

		public override bool InspectSelf(DataVisualization visualization, string name, ref object data, Type type)
		{
			var dictionary = data as IObjectDictionary;
			if (dictionary == null)
				return false;
			
			EditorGUILayout.LabelField("Count: " + dictionary.Count);
			return false;
		}

		//public override bool InspectChildren(DataVisualization visualization, string path, ref object data, Type type)
		//{
		//	var dictionary = data as IObjectDictionary;
		//	if (dictionary == null)
		//		return false;

		//	Type dictionaryValueType = null;
		//	if (TypeTools.IsSubclassOfDictionary(type))
		//		dictionaryValueType = TypeTools.GetDictionaryValueType(type);

		//	return EditorUIUtils.EditElems(dictionary, EditElem(visualization, path, dictionary, dictionaryValueType), visualization.isFoldout,
		//		path + "[fold]");
		//}

		//private static Func<object, object, bool> EditElem(DataVisualization visualization, string path, IDictionary dictionary, Type dictionaryValueType)
		//{
		//	return (key, value) =>
		//	{
		//		var valueType = value != null ? value.GetType() : dictionaryValueType;
		//		return visualization.Inspect(key.ToString(), path + "." + key, value, valueType, null, v => dictionary[key] = v);
		//	};
		//}
	}
}

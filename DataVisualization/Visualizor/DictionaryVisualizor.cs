using System;
using System.Collections;
using FF;
using UnityEditor;

internal class DictionaryVisualizor : DataVisualizor
{
	public override bool IsFoldable()
	{
		return true;
	}

	public override bool HasContent(object data, Type type)
	{
		IDictionary dictionary = data as IDictionary;
		if (dictionary == null)
			return false;

		return dictionary.Count > 0;
	}

	public override bool InspectRoot(string name, ref object data, Type type)
	{
		IDictionary dictionary = data as IDictionary;
		if (dictionary == null)
			return false;

		EditorGUIUtility.labelWidth = DataVisualization.LabelWidth;
		EditorGUILayout.LabelField("Count: ", dictionary.Count.ToString());
		return false;
	}

	public override bool InspectContent(DataVisualization visualization, string path, ref object data, Type type)
	{
		IDictionary dictionary = data as IDictionary;
		if (dictionary == null)
			return false;

		Type dictionaryValueType = null;
		if (TypeUtil.IsSubclassOfDictionary(type))
		{
			dictionaryValueType = TypeUtil.GetDictionaryValueType(type);
		}

		return EditorUIUtils.EditElems(dictionary, EditElem(visualization, path, dictionary, dictionaryValueType), visualization.opened,
			path + "[fold]");
	}

	private static Func<object, object, bool> EditElem(DataVisualization visualization, string path, IDictionary dictionary, Type dictionaryValueType)
	{
		return (key, value) =>
		{
			Type valueType = value != null ? value.GetType() : dictionaryValueType;
			return visualization.Inspect(key.ToString(), path + "." + key, value, valueType, null, v => dictionary[key] = v);
		};
	}
}
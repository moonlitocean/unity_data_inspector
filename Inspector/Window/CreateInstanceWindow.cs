#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using DataInspector;
using UnityEditor;
using UnityEngine;

public class CreateInstanceWindow : EditorWindow
{
	private static CreateInstanceWindow instance;
	private static string resultPath;
	private static bool hasResult;
	private static object result;

	private Type baseType;

	private List<Type> derivedTypes;
	private Vector2 scroll;
	private Action<object> onValueChanged;

	public static bool HasResult(string path)
	{
		return hasResult && resultPath != null && resultPath == path;
	}

	public static object TakeResult()
	{
		if(resultPath == null || !hasResult)
			return null;

		object ret = result;
		resultPath = null;
		result = null;
		return ret;
	}

	public static void CreateInstance(string path, Type baseType, VisualizerBase visualizer, IMark mark)
	{
		if (instance != null)
			throw new Exception("Only one CreateInstanceWindow can be created");

		resultPath = path;
		hasResult = false;
		result = null;
		if (visualizer.HasCustomCreator(baseType, mark))
		{
			hasResult = true;
			result = visualizer.CustomCreateInstance(baseType, mark);
			return;
		}

		var derived = TypeTools.GetDerivedTypeWithDefaultCtor(baseType);
		if (derived.Count > 1 || derived.Count == 1 && derived[0] != baseType)
		{
			CreateInstanceWindow window = CreateInstanceWindow.CreateInstance<CreateInstanceWindow>();
			window.baseType = baseType;
			window.derivedTypes = derived;
			window.position = new Rect(GUIUtility.GUIToScreenPoint(Event.current.mousePosition), new Vector2(400, 200));
			window.ShowAuxWindow();
			return;
		}

		hasResult = true;
		if(TypeTools.CanCreateInstance(baseType))
			result = Activator.CreateInstance(baseType);
	}


	// »æÖÆµ¯´°µÄGUI
	void OnGUI()
	{
		if (baseType == null || derivedTypes == null)
			return;

		Type option = null;
		if (derivedTypes.Count > 0)
		{
			EditorGUILayout.LabelField($"Create Instance Of Type {baseType.Name}, {derivedTypes.Count} Constructor(s) Found");
			using (GUITools.Scroll(ref scroll))
			{
				for (int i = 0; i < derivedTypes.Count; ++i)
				{
					using (GUITools.HorizontalScope())
					{
						EditorGUILayout.LabelField(derivedTypes[i].Name);
						if (GUILayout.Button("Create"))
						{
							option = derivedTypes[i];
						}
					}
				}
			}
		}
		else
		{
			EditorGUILayout.LabelField($"No default Constructor of Type found: {baseType.Name}");
		}

		if (option != null)
		{
			hasResult = true;
			result = Activator.CreateInstance(option);
			Close();
		}
	}

	void OnDisable()
	{
		instance = null;
	}
}

#endif
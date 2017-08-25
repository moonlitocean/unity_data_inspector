using System;
using System.Collections.Generic;
using FF;
using FF.Editor;
using FF.Unity;
using Metadata;
using Unique;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public class DataVisualizationOptions
{
	public class SimpleContentOption
	{
		public bool showNonPublicFields;
		public bool showClassStaticFields;
		public bool showDataVisualizationFields;
		public bool sortFields;
	}

	public SimpleContentOption simpleContent = new SimpleContentOption();
}

public class DataVisualization
{
	public const float LabelWidth = 300;
	
	private static readonly Dictionary<Type, DataVisualizor> cachedVisualizer = new Dictionary<Type, DataVisualizor>();
	public readonly Dictionary<string, bool> opened = new Dictionary<string, bool>(StringComparer.Ordinal);
	public DataVisualizationOptions options = new DataVisualizationOptions();

	// Visualizors映射
	private readonly DataVisualizor atomVisualizor;				// 被高频调用，所以不走finelTypeVisualizor
	private readonly Dictionary<Type, DataVisualizor> visualizors = new Dictionary<Type, DataVisualizor>();
    private readonly Dictionary<Type, DataVisualizor> finelTypeVisualizor = new Dictionary<Type, DataVisualizor>();

    public DataVisualization()
    {
		atomVisualizor = GetVisualizer(typeof(AtomDataVisualizor));

		/////////////////////////////////////////////////////////////////////////
		// 注册 type --> visualize 的直接一对一映射。注意不支持继承
		// C# 标准库类型
	    finelTypeVisualizor.Add(typeof(DateTime), GetVisualizer(typeof(DateTimeVisualizor)));
	    finelTypeVisualizor.Add(typeof(TimeSpan), GetVisualizer(typeof(TimeSpanVisualizor)));

	    // UnityEngine类型，不使用编辑器原生逻辑，特殊处理
	    finelTypeVisualizor.Add(typeof(GameObject), GetVisualizer(typeof(GameObjectVisualizor)));

	    // UnityEngine类型，直接使用Unity编辑器原生逻辑，由 UnityDataVisualizor 统一处理的系列
	    finelTypeVisualizor.Add(typeof(Color), GetVisualizer(typeof(UnityDataVisualizor)));
	    finelTypeVisualizor.Add(typeof(Vector2), GetVisualizer(typeof(UnityDataVisualizor)));
	    finelTypeVisualizor.Add(typeof(Vector3), GetVisualizer(typeof(UnityDataVisualizor)));
	    finelTypeVisualizor.Add(typeof(Vector4), GetVisualizer(typeof(UnityDataVisualizor)));
	    finelTypeVisualizor.Add(typeof(Bounds), GetVisualizer(typeof(UnityDataVisualizor)));
	    finelTypeVisualizor.Add(typeof(AnimationCurve), GetVisualizer(typeof(UnityDataVisualizor)));
	    finelTypeVisualizor.Add(typeof(Rect), GetVisualizer(typeof(UnityDataVisualizor)));

	    // 基础数学类型，自定义的FF系列，BB系列
	    finelTypeVisualizor.Add(typeof(FFQuaternion), GetVisualizer(typeof(FFQuaternionVisualizor)));
	    finelTypeVisualizor.Add(typeof(FFColor), GetVisualizer(typeof(FFColorVisualizor)));
	    finelTypeVisualizor.Add(typeof(BBFloat), GetVisualizer(typeof(FloatDataVisualizor)));
	    finelTypeVisualizor.Add(typeof(BBInt), GetVisualizer(typeof(BBIntVisualizor)));
	    finelTypeVisualizor.Add(typeof(BBVector3), GetVisualizer(typeof(Vector3DataVisualizor)));
	    finelTypeVisualizor.Add(typeof(BBObjectData), GetVisualizer(typeof(BBObjectDataVisualizor)));

	    // 自定义游戏内类型
	    finelTypeVisualizor.Add(typeof(ObjectData), GetVisualizer(typeof(ObjectDataVisualizor)));
	    finelTypeVisualizor.Add(typeof(BlackBoardData), GetVisualizer(typeof(BlackBoardDataVisualizor)));
	    finelTypeVisualizor.Add(typeof(AITargetData), GetVisualizer(typeof(AITargetDataVisualizor)));
	    finelTypeVisualizor.Add(typeof(InspectorMethod), GetVisualizer(typeof(CallableMethodVisualizor)));
	    finelTypeVisualizor.Add(typeof(UINode), GetVisualizer(typeof(UINodeVisualizor)));
		finelTypeVisualizor.Add(typeof(AttributeVariable), GetVisualizer(typeof(AttributeVariableVisualizor)));
		finelTypeVisualizor.Add(typeof(RangeAttributeVariable_long), GetVisualizer(typeof(RangeAttributeVariable_longVisualizor)));
		finelTypeVisualizor.Add(typeof(RangeAttributeVariable_float), GetVisualizer(typeof(RangeAttributeVariable_floatVisualizor)));
    }

	private DataVisualizor GetVisualizer(Type visualizerType)
	{
		if (!visualizors.ContainsKey(visualizerType))
			visualizors[visualizerType] = (DataVisualizor) TypeUtil.CreateInstance(visualizerType);

		return visualizors[visualizerType];
	}

	public bool Inspect(string name, string path, object data,
		Type type = null, 
		IEditorMark mark = null,
		Action<object> OnValueChanged = null)
	{
		if(type == null)
			type = data != null ? data.GetType() : null;

		DataVisualizor visualizor = GetDataVisualizor(data, type, mark);
		bool changed = false;
		object changedData = data;
		if (visualizor != null)
		{
			string fieldinfo = name;
			if (visualizor.IsFoldable())
			{
				const int lableWidth = 220;
				EditorGUILayout.BeginHorizontal();
				using (GUITools.LayoutHorizontal(GUILayout.Width(lableWidth)))
				{
					opened[path] = FFGUILayout.Foldout(opened.ContainsKey(path) && opened[path], fieldinfo, true);
				}
				changed |= InspectRoot(name, type, ref changedData, visualizor, mark);
				EditorGUILayout.EndHorizontal();

				if (visualizor.HasContent(data, type))
				{
					if (changedData != null && opened[path])
					{
						EditorGUI.indentLevel++;
						changed |= visualizor.InspectContent(this, path, ref changedData, type);
						EditorGUI.indentLevel--;
					}
				}
			}
			else
			{
				changed |= InspectRoot(fieldinfo, type, ref changedData, visualizor, mark);
			}
		}

	    if (changed && OnValueChanged != null)
	    {
		    OnValueChanged(changedData);
	    }
	    return changed;
	}


	private static bool InspectRoot(string name, Type type, ref object data, DataVisualizor visualizor, IEditorMark mark)
	{
		using (GUITools.LayoutHorizontal())
		{
			bool changed = visualizor.InspectRoot(name, ref data, type);
			if (type != null && type.IsClass)
			{
				EditorGUILayout.Space();
				if (data != null)
				{
					if (GUILayout.Button("-", GUILayout.Width(20)))
					{
						data = null;
						changed = true;
					}
				}
				else
				{
					if (GUILayout.Button("+", GUILayout.Width(20)))
					{
						data = CreateClassInstance(type, visualizor, mark);
						changed = true;
					}
				}
			}
			return changed;
		}
	}

	private static object CreateClassInstance(Type type, DataVisualizor visualizor, IEditorMark mark)
	{
		if (visualizor.HasCustomCreator(mark))
			return visualizor.CustomCreateInstance(mark);

		if (type == typeof(string))
		{
			return "";
		}
		else if (type.IsArray)
		{
			return Array.CreateInstance(type.GetElementType(), new int[type.GetArrayRank()]);
		}
		else
		{
			try
			{
				return Activator.CreateInstance(type);
			}
			catch (Exception e)
			{
				Debug.Log(Global.FormatException(e, true));
				return null;
			}
		}
	}

	private DataVisualizor GetDataVisualizor(object data, Type type, IEditorMark mark)
	{
		if (mark == null && data == null)
			return atomVisualizor;

		DataVisualizor v;
		if(!cachedVisualizer.TryGetValue(type, out v))
		{
			v = FindVisualizor(type, mark);
			cachedVisualizer[type] = v;
		}
		return v;
	}

	private DataVisualizor FindVisualizor(Type type, IEditorMark mark)
	{
		if (type == null && mark == null)
			throw new NullReferenceException("There must be at least one argument that is not null");

		if (mark != null)
		{
			if (mark is CTimeAttribute)
				return GetVisualizer(typeof (DateTimeVisualizor));

			if (mark is LocaleAttribute)
				return GetVisualizer(typeof (LocaleStringVisualizor));

			throw new NotImplementedException(mark.GetType().ToString());
		}

		if (finelTypeVisualizor.ContainsKey(type))
			return finelTypeVisualizor[type];

		//////////////////////////////////////////////////////////////////
		// 涉及到继承等情况的处理
		if (type.IsPrimitive || type == typeof (string))
			return GetVisualizer(typeof(AtomDataVisualizor));

		if (type.IsEnum)
			return GetVisualizer(typeof(EnumVisualizor));

		if (type.IsSubclassOf(typeof(Type)))
			return GetVisualizer(typeof(StaticContainerVisualizor));

		if (type.IsSubclassOf(typeof(Object)))
			return GetVisualizer(typeof(UnityObjectVisualizor));

		if (type.IsSubclassOf(typeof(MulticastDelegate)))
			return GetVisualizer(typeof(MulticastDelegateVisualizor));

		if (TypeUtil.IsSubclassOfList(type) || type.IsArray)
			return GetVisualizer(typeof(ListVisualizor));

		if (TypeUtil.IsSubclassOfDictionary(type))
			return GetVisualizer(typeof(DictionaryVisualizor));

		if (TypeUtil.IsSubclassOfObjectDictionary(type))
			return GetVisualizer(typeof(ObjectDictionaryVisualizor));

		if (TypeUtil.IsSubclassOfHashSet(type))
			return GetVisualizer(typeof(HashSetVisualizor));

		return GetVisualizer(typeof(SimpleContentVisualizor));
	}
}
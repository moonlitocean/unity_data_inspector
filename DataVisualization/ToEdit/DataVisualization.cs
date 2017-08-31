using System;
using System.Collections.Generic;
using FF;
using FF.Editor;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public class DataVisualizationOptions
{
	public class CompositeOption
	{
		public bool showNonPublicFields;
		public bool showStaticFields;
		public bool sortFields;
	}

	public readonly CompositeOption composite = new CompositeOption();
}

public class DataVisualization
{
	public const float LabelWidth = 300;
	
	private static readonly Dictionary<Type, DataVisualizer> cachedVisualizer = new Dictionary<Type, DataVisualizer>();
	public readonly Dictionary<string, bool> opened = new Dictionary<string, bool>(StringComparer.Ordinal);
	public readonly DataVisualizationOptions options = new DataVisualizationOptions();

	// Visualizors映射
	private readonly DataVisualizer atomVisualizer;				// 被高频调用，所以不走finelTypeVisualizor
	private readonly Dictionary<Type, DataVisualizer> visualizors = new Dictionary<Type, DataVisualizer>();
    private readonly Dictionary<Type, DataVisualizer> finelTypeVisualizor = new Dictionary<Type, DataVisualizer>();

    public DataVisualization()
    {
		atomVisualizer = GetVisualizer(typeof(AtomVisualizer));

		/////////////////////////////////////////////////////////////////////////
		// 注册 type --> visualize 的直接一对一映射。注意不支持继承
		// C# 标准库类型
	    finelTypeVisualizor.Add(typeof(DateTime), GetVisualizer(typeof(DateTimeVisualizer)));
	    finelTypeVisualizor.Add(typeof(TimeSpan), GetVisualizer(typeof(TimeSpanVisualizer)));

	    // UnityEngine类型，不使用编辑器原生逻辑，特殊处理
	    // finelTypeVisualizor.Add(typeof(GameObject), GetVisualizer(typeof(GameObjectVisualizer)));

	    // UnityEngine类型，直接使用Unity编辑器原生逻辑，由 UnityTypeVisualizer 统一处理的系列
	    finelTypeVisualizor.Add(typeof(Color), GetVisualizer(typeof(UnityTypeVisualizer)));
	    finelTypeVisualizor.Add(typeof(Vector2), GetVisualizer(typeof(UnityTypeVisualizer)));
	    finelTypeVisualizor.Add(typeof(Vector3), GetVisualizer(typeof(UnityTypeVisualizer)));
	    finelTypeVisualizor.Add(typeof(Vector4), GetVisualizer(typeof(UnityTypeVisualizer)));
	    finelTypeVisualizor.Add(typeof(Bounds), GetVisualizer(typeof(UnityTypeVisualizer)));
		finelTypeVisualizor.Add(typeof(Rect), GetVisualizer(typeof(UnityTypeVisualizer)));
	    finelTypeVisualizor.Add(typeof(AnimationCurve), GetVisualizer(typeof(UnityTypeVisualizer)));


	    // 基础数学类型，自定义的FF系列，BB系列
		//finelTypeVisualizor.Add(typeof(FFQuaternion), GetVisualizer(typeof(FFQuaternionVisualizor)));
		//finelTypeVisualizor.Add(typeof(FFColor), GetVisualizer(typeof(FFColorVisualizor)));
		//finelTypeVisualizor.Add(typeof(BBFloat), GetVisualizer(typeof(FloatDataVisualizor)));
		//finelTypeVisualizor.Add(typeof(BBInt), GetVisualizer(typeof(BBIntVisualizor)));
		//finelTypeVisualizor.Add(typeof(BBVector3), GetVisualizer(typeof(Vector3DataVisualizor)));
		//finelTypeVisualizor.Add(typeof(BBObjectData), GetVisualizer(typeof(BBObjectDataVisualizor)));

	    // 自定义游戏内类型
		//finelTypeVisualizor.Add(typeof(ObjectData), GetVisualizer(typeof(ObjectDataVisualizor)));
		//finelTypeVisualizor.Add(typeof(BlackBoardData), GetVisualizer(typeof(BlackBoardDataVisualizor)));
		//finelTypeVisualizor.Add(typeof(AITargetData), GetVisualizer(typeof(AITargetDataVisualizor)));
		//finelTypeVisualizor.Add(typeof(InspectorMethod), GetVisualizer(typeof(CallableMethodVisualizor)));
		//finelTypeVisualizor.Add(typeof(UINode), GetVisualizer(typeof(UINodeVisualizor)));
		//finelTypeVisualizor.Add(typeof(AttributeVariable), GetVisualizer(typeof(AttributeVariableVisualizor)));
		//finelTypeVisualizor.Add(typeof(RangeAttributeVariable_long), GetVisualizer(typeof(RangeAttributeVariable_longVisualizor)));
		//finelTypeVisualizor.Add(typeof(RangeAttributeVariable_float), GetVisualizer(typeof(RangeAttributeVariable_floatVisualizor)));
    }

	private DataVisualizer GetVisualizer(Type visualizerType)
	{
		if (!visualizors.ContainsKey(visualizerType))
			visualizors[visualizerType] = (DataVisualizer) TypeTools.CreateInstance(visualizerType);

		return visualizors[visualizerType];
	}

	public bool Inspect(string name, string path, object data,
		Type type = null, 
		IMark mark = null,
		Action<object> OnValueChanged = null)
	{
		if(type == null)
			type = data != null ? data.GetType() : null;

		DataVisualizer visualizer = GetDataVisualizor(type, mark);
		bool changed = false;
		object changedData = data;
		if (visualizer != null)
		{
			string fieldinfo = name;
			if (visualizer.HasChildren())
			{
				const int lableWidth = 220;
				EditorGUILayout.BeginHorizontal();
				using (new EditorGUILayout.HorizontalScope(GUILayout.Width(lableWidth)))
				{
					opened[path] = GUITools.Foldout(opened.ContainsKey(path) && opened[path], fieldinfo, true);
				}
				changed |= InspectRoot(name, type, ref changedData, visualizer, mark);
				EditorGUILayout.EndHorizontal();

				if (changedData != null && opened[path])
				{
					EditorGUI.indentLevel++;
					changed |= visualizer.InspectChildren(this, path, ref changedData, type);
					EditorGUI.indentLevel--;
				}
			}
			else
			{
				changed |= InspectRoot(fieldinfo, type, ref changedData, visualizer, mark);
			}
		}

	    if (changed && OnValueChanged != null)
	    {
		    OnValueChanged(changedData);
	    }
	    return changed;
	}


	private static bool InspectRoot(string name, Type type, ref object data, DataVisualizer visualizer, IMark mark)
	{
		using (new EditorGUILayout.HorizontalScope())
		{
			bool changed = visualizer.InspectSelf(name, ref data, type);
			if (type != null && type.IsClass)
			{
				GUILayout.FlexibleSpace();
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
						data = CreateClassInstance(type, visualizer, mark);
						changed = true;
					}
				}
			}
			return changed;
		}
	}

	private static object CreateClassInstance(Type type, DataVisualizer visualizer, IMark mark)
	{
		if (visualizer.HasCustomCreator(mark))
			return visualizer.CustomCreateInstance(mark);

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
				Debug.Log(e);
				return null;
			}
		}
	}

	private DataVisualizer GetDataVisualizor(Type type, IMark mark)
	{
		if (mark == null && type == null)
			return atomVisualizer;

		DataVisualizer v;
		if(!cachedVisualizer.TryGetValue(type, out v))
		{
			v = FindVisualizor(type, mark);
			cachedVisualizer[type] = v;
		}
		return v;
	}

	private DataVisualizer FindVisualizor(Type type, IMark mark)
	{
		if (type == null && mark == null)
			throw new NullReferenceException("There must be at least one argument that is not null");

		if (mark != null)
		{
			if (mark is UnixTimestampAttribute)
				return GetVisualizer(typeof (DateTimeVisualizer));

			//if (mark is LocaleAttribute)
			//	return GetVisualizer(typeof (LocaleStringVisualizor));

			throw new NotImplementedException(mark.GetType().ToString());
		}

		if (finelTypeVisualizor.ContainsKey(type))
			return finelTypeVisualizor[type];

		//////////////////////////////////////////////////////////////////
		// 涉及到继承等情况的处理
		if (type.IsPrimitive || type == typeof (string))
			return GetVisualizer(typeof(AtomVisualizer));

		if (type.IsEnum)
			return GetVisualizer(typeof(EnumVisualizer));

		if (type.IsSubclassOf(typeof(Type)))
			return GetVisualizer(typeof(StaticVisualizer));

		if (type.IsSubclassOf(typeof(Object)))
			return GetVisualizer(typeof(UnityObjectVisualizer));

		if (type.IsSubclassOf(typeof(MulticastDelegate)))
			return GetVisualizer(typeof(MulticastDelegateVisualizer));

		if (TypeTools.IsSubclassOfList(type) || type.IsArray)
			return GetVisualizer(typeof(ListArrayVisualizer));

		if (TypeTools.IsSubclassOfDictionary(type))
			return GetVisualizer(typeof(DictionaryVisualizer));

		if (TypeTools.IsSubclassOfHashSet(type))
			return GetVisualizer(typeof(HashSetVisualizer));

		return GetVisualizer(typeof(CompositeVisualizer));
	}
}
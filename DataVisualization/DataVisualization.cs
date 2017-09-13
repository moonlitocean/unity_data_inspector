using System;
using System.Collections;
using System.Collections.Generic;
using DataTools;
using DataTools.Editor;
using UnityEditor;
using UnityEngine;


public class DataVisualization
{
	public enum SpecialVisualizer
	{
		Primitive,
		Composite,
		Enum
	};
	public class Options
	{
		public float labelWidth = 250;
		public bool showNonPublicFields;
		public bool showStaticFields;
		public bool sortFields;
	}
	public readonly Options options = new Options();
	public readonly Dictionary<string, bool> isFoldout = new Dictionary<string, bool>(StringComparer.Ordinal);

	private static readonly Dictionary<Type, DataVisualizer> cachedVisualizer = new Dictionary<Type, DataVisualizer>();

	// Visualizors映射
	private DataVisualizer primitiveVisualizer;
	private DataVisualizer compositeVisualizer;
	private DataVisualizer enumVisualizer;
    private readonly Dictionary<Type, DataVisualizer> rules = new Dictionary<Type, DataVisualizer>();
	private readonly Dictionary<Type, DataVisualizer> markRules = new Dictionary<Type, DataVisualizer>();

    public DataVisualization()
    {
		/////////////////////////////////////////////////////////////////////////
		// 注册三个基本visualizer
		primitiveVisualizer = new PrimitiveVisualizer();
	    compositeVisualizer = new CompositeVisualizer();
	    enumVisualizer = new EnumVisualizer();

		/////////////////////////////////////////////////////////////////////////
		// 注册 type --> visualize 的映射。
		// 支持继承、接口、完全无绑定的泛型基类和完全绑定的泛型基类。不支持部分绑定的泛型基类
		// 特殊规则
	    rules.Add(typeof (Type), new StaticVisualizer());

		// C# 基本类型
		rules.Add(typeof(string), new StringVisualizer());
		rules.Add(typeof(MulticastDelegate), new MulticastDelegateVisualizer());
		rules.Add(typeof(DateTime), new DateTimeVisualizer());
		rules.Add(typeof(TimeSpan), new TimeSpanVisualizer());

		// C# 容器类型
		rules.Add(typeof(IList), new ListVisualizer());
		rules.Add(typeof(IDictionary), new DictionaryVisualizer());
		rules.Add(typeof(HashSet<>), new HashSetVisualizer());

	    // UnityEngine类型，直接使用Unity编辑器原生逻辑，由 UnityTypeVisualizer 统一处理的系列
		rules.Add(typeof(UnityEngine.Object), new UnityObjectVisualizer());

	    var unityTypeVisualizer = new UnityTypeVisualizer();
		rules.Add(typeof(Color), unityTypeVisualizer);
		rules.Add(typeof(Vector2), unityTypeVisualizer);
		rules.Add(typeof(Vector3), unityTypeVisualizer);
		rules.Add(typeof(Vector4), unityTypeVisualizer);
		rules.Add(typeof(Bounds), unityTypeVisualizer);
		rules.Add(typeof(Rect), unityTypeVisualizer);
		rules.Add(typeof(AnimationCurve), unityTypeVisualizer);

		// Marker
	    markRules.Add(typeof (UnixTimestampAttribute), rules[typeof (DateTime)]);
	    // SetVisualizer(typeof(LocaleAttribute), GetVisualizer(typeof(LocaleStringVisualizor)));

	    // 基础数学类型，自定义的FF系列，BB系列
	    //rules.Add(typeof(FFQuaternion), GetVisualizer(typeof(FFQuaternionVisualizor)));
	    //rules.Add(typeof(FFColor), GetVisualizer(typeof(FFColorVisualizor)));
	    //rules.Add(typeof(BBFloat), GetVisualizer(typeof(FloatDataVisualizor)));
	    //rules.Add(typeof(BBInt), GetVisualizer(typeof(BBIntVisualizor)));
	    //rules.Add(typeof(BBVector3), GetVisualizer(typeof(Vector3DataVisualizor)));
	    //rules.Add(typeof(BBObjectData), GetVisualizer(typeof(BBObjectDataVisualizor)));

	    // 自定义游戏内类型
	    //rules.Add(typeof(ObjectData), GetVisualizer(typeof(ObjectDataVisualizor)));
	    //rules.Add(typeof(BlackBoardData), GetVisualizer(typeof(BlackBoardDataVisualizor)));
	    //rules.Add(typeof(AITargetData), GetVisualizer(typeof(AITargetDataVisualizor)));
	    //rules.Add(typeof(InspectorMethod), GetVisualizer(typeof(CallableMethodVisualizor)));
	    //rules.Add(typeof(UINode), GetVisualizer(typeof(UINodeVisualizor)));
	    //rules.Add(typeof(AttributeVariable), GetVisualizer(typeof(AttributeVariableVisualizor)));
	    //rules.Add(typeof(RangeAttributeVariable_long), GetVisualizer(typeof(RangeAttributeVariable_longVisualizor)));
	    //rules.Add(typeof(RangeAttributeVariable_float), GetVisualizer(typeof(RangeAttributeVariable_floatVisualizor)));
    }

	public void RemoveVisualizer(Type type)
	{
		if (type == null)
			throw new ArgumentException();

		if (rules.ContainsKey(type))
			rules.Remove(type);
		if (markRules.ContainsKey(type))
			markRules.Remove(type);
	}

	public void RemoveAllVisualizers()
	{
		rules.Clear();
		markRules.Clear();
	}

	public void SetVisualizer(Type type, DataVisualizer income)
	{
		if (type == null || income == null)
			throw new ArgumentException();

		if (type.GetInterface(typeof (IMark).Name) != null)
			markRules[type] = income;
		else
			rules[type] = income;
	}

	public void SetSpecialVisualizer(SpecialVisualizer type, DataVisualizer income)
	{
		if (income == null)
			throw new ArgumentException();

		switch (type)
		{
			case SpecialVisualizer.Composite:
				compositeVisualizer = income;
				break;
			case SpecialVisualizer.Enum:
				enumVisualizer = income;
				break;
			case SpecialVisualizer.Primitive:
				primitiveVisualizer = income;
				break;
			default:
				throw new NotImplementedException(type.ToString());
		}
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
					isFoldout[path] = GUITools.Foldout(isFoldout.ContainsKey(path) && isFoldout[path], fieldinfo, true);
				}
				changed |= InspectRoot(name, type, ref changedData, visualizer, mark);
				EditorGUILayout.EndHorizontal();

				if (changedData != null && isFoldout[path])
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


	private bool InspectRoot(string name, Type type, ref object data, DataVisualizer visualizer, IMark mark)
	{
		using (new EditorGUILayout.HorizontalScope())
		{
			bool changed = visualizer.InspectSelf(this, name, ref data, type);
			if (type != null && type.IsClass)
			{
                if(!(data is string))
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
		if (visualizer.HasCustomCreator(type, mark))
			return visualizer.CustomCreateInstance(type, mark);

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

	private DataVisualizer GetDataVisualizor(Type type, IMark mark)
	{
		if (mark == null && type == null)
			return primitiveVisualizer;

		DataVisualizer v;
		if(!cachedVisualizer.TryGetValue(type, out v))
		{
			v = FindVisualizor(type, mark);
			cachedVisualizer[type] = v;
		}
		return v;
	}

	// Match Order:
	//	* Registered IMark
	//  * Primitives
	//  * Enums
	//  * Registered concrete type that equals to the current type
	//  * Registered type that is the first matched base type or generice base type
	//		Concrete type first, dont support partially binded type
	//  * Registered interface that is the interface of current type
	//		If multiply interfaces match, the result is unspecified, depend on the sequence of GetInterfaces()
	private DataVisualizer FindVisualizor(Type type, IMark mark)
	{
		if (type == null && mark == null)
			throw new NullReferenceException("There must be at least one argument that is not null");

		if (mark != null)
			return FindMarkVisualizer(mark);

		if (type.IsPrimitive)
			return primitiveVisualizer;

		if (type.IsEnum)
			return enumVisualizer;

		if (rules.ContainsKey(type))
			return rules[type];

		DataVisualizer visualizor = FindBaseTypeVisualizer(type);
		if (visualizor != null)
			return visualizor;

		visualizor = FindInterfaceVisualizer(type);
		if (visualizor != null)
			return visualizor;

		return compositeVisualizer;
	}

	private DataVisualizer FindMarkVisualizer(IMark mark)
	{
		var t = mark.GetType();
		if (markRules.ContainsKey(t))
			return markRules[t];

		throw new NotImplementedException(mark.ToString());
	}

	private DataVisualizer FindBaseTypeVisualizer(Type type)
	{
		for (Type t = type; t != null; t = t.BaseType)
		{
			if (rules.ContainsKey(t))
				return rules[t];

			if (t.IsGenericType)
			{
				var generic = t.GetGenericTypeDefinition();
				if (rules.ContainsKey(generic))
					return rules[generic];
			}
		}

		return null;
	}

	private DataVisualizer FindInterfaceVisualizer(Type type)
	{	
		foreach (var t in type.GetInterfaces())
		{
			if (rules.ContainsKey(t))
				return rules[t];

			if (t.IsGenericType)
			{
				var generic = t.GetGenericTypeDefinition();
				if (rules.ContainsKey(generic))
					return rules[generic];
			}
		}

		return null;
	}
}
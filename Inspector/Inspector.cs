using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DataInspector
{
	public class Inspector
	{
		public enum SpecialVisualizer
		{
			PrimitiveAndNull,
			Enum,
			Composite,
		};

		public class Options
		{
			public int labelWidth = 250;	// The label width of the first column.
			public int indentOffset = 15;	// Magic number, the offset of each EditorGUI.indentLevel. 
											// (Used to correctly layout foldout rows, there should be some more Unity way but I dont know)

			// TODO: Only supports CompositeVisualizer. Should apply to other visalizers.
			public bool showNonPublicFields;
			public bool showStaticFields;
			public bool sortFields;
		}

		public readonly Options options = new Options();
		public readonly Dictionary<string, bool> isFoldout = new Dictionary<string, bool>(StringComparer.Ordinal);

		private static readonly Dictionary<Type, VisualizerBase> cachedVisualizer = new Dictionary<Type, VisualizerBase>();

		///////////////////////////////////////////////////////////////
		// Visualizors in three categories: special, type rules, and IMark rules
		private VisualizerBase primitiveAndNullVisualizer;
		private VisualizerBase compositeVisualizer;
		private VisualizerBase enumVisualizer;
		private readonly Dictionary<Type, VisualizerBase> rules = new Dictionary<Type, VisualizerBase>();
		private readonly Dictionary<Type, VisualizerBase> markRules = new Dictionary<Type, VisualizerBase>();

		public Inspector()
		{
			RegisterDefaultVisualizers();
		}

		private void RegisterDefaultVisualizers()
		{
			/////////////////////////////////////////////////////////////////////////
			// Register three special visualizer
			primitiveAndNullVisualizer = new PrimitiveVisualizer();
			compositeVisualizer = new CompositeVisualizer();
			enumVisualizer = new EnumVisualizer();

			// static visualizers
			// If input is a type, then show static members of that type
			//
			// The draw back is, you cannot inpect the real content of Type type anymore.
			rules.Add(typeof (Type), new StaticVisualizer());

			/////////////////////////////////////////////////////////////////////////
			// type --> visualize
			// C# basic types
			rules.Add(typeof (string), new StringVisualizer());
			rules.Add(typeof (MulticastDelegate), new MulticastDelegateVisualizer());
			rules.Add(typeof (DateTime), new DateTimeVisualizer());
			rules.Add(typeof (TimeSpan), new TimeSpanVisualizer());

			// C# containers
			rules.Add(typeof (IList), new ListVisualizer());
			rules.Add(typeof (IDictionary), new DictionaryVisualizer());
			rules.Add(typeof (HashSet<>), new HashSetVisualizer());

			// UnityEngine types
			rules.Add(typeof (UnityEngine.Object), new UnityObjectVisualizer());

			var unityTypeVisualizer = new UnityTypeVisualizer();
			rules.Add(typeof (Color), unityTypeVisualizer);
			rules.Add(typeof (Vector2), unityTypeVisualizer);
			rules.Add(typeof (Vector3), unityTypeVisualizer);
			rules.Add(typeof (Vector4), unityTypeVisualizer);
			rules.Add(typeof (Bounds), unityTypeVisualizer);
			rules.Add(typeof (Rect), unityTypeVisualizer);
			rules.Add(typeof (AnimationCurve), unityTypeVisualizer);

			// Markers
			markRules.Add(typeof (UnixTimestampAttribute), new UnixTimeStampVisualizer());
		}

		public void RemoveAllVisualizers()
		{
			AssertValidSetupState();

			rules.Clear();
			markRules.Clear();
		}

		public VisualizerBase GetVisualizer(Type type)
		{
			if (type == null)
				throw new ArgumentNullException("type");

			if (rules.ContainsKey(type))
				return rules[type];
			else
				return markRules[type];
		}

		public void SetVisualizer(Type type, VisualizerBase income)
		{
			if (type == null)
				throw new ArgumentNullException("type");
			if (income == null)
				throw new ArgumentNullException("income");
			AssertValidSetupState();

			if (type.GetInterface(typeof (IMark).Name) != null)
				markRules[type] = income;
			else
				rules[type] = income;
		}

		public void RemoveVisualizer(Type type)
		{
			if (type == null)
				throw new ArgumentNullException("type");
			AssertValidSetupState();

			if (rules.ContainsKey(type))
				rules.Remove(type);
			if (markRules.ContainsKey(type))
				markRules.Remove(type);
		}


		public VisualizerBase GetSpecialVisualizer(SpecialVisualizer type)
		{
			switch (type)
			{
				case SpecialVisualizer.Composite:
					return compositeVisualizer;
				case SpecialVisualizer.Enum:
					return enumVisualizer;
				case SpecialVisualizer.PrimitiveAndNull:
					return primitiveAndNullVisualizer;
				default:
					throw new NotImplementedException(type.ToString());
			}
		}

		public void SetSpecialVisualizer(SpecialVisualizer type, VisualizerBase income)
		{
			if (income == null)
				throw new ArgumentNullException("income");
			AssertValidSetupState();

			switch (type)
			{
				case SpecialVisualizer.Composite:
					compositeVisualizer = income;
					break;
				case SpecialVisualizer.Enum:
					enumVisualizer = income;
					break;
				case SpecialVisualizer.PrimitiveAndNull:
					primitiveAndNullVisualizer = income;
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
			if (type == null)
				type = data != null ? data.GetType() : null;

			EditorGUIUtility.labelWidth = options.labelWidth;
			VisualizerBase visualizer = GetVisualizor(type, mark);
			bool changed = false;
			object changedData = data;
			if (visualizer != null)
			{
				string fieldinfo = name;
				var postfix = visualizer.GetLabelPostfix(this, data, type);
				if (postfix != null)
					fieldinfo += postfix;

				if (visualizer.HasChildren())
				{
					bool alwaysShowChildren = visualizer.AlwaysShowChildren();
					if (!alwaysShowChildren)
					{
						using (new EditorGUILayout.HorizontalScope())
						{
							var width = options.labelWidth - options.indentOffset*EditorGUI.indentLevel;
							using (new EditorGUILayout.HorizontalScope(GUILayout.Width(width)))
							{
								isFoldout[path] = GUITools.Foldout(isFoldout.ContainsKey(path) && isFoldout[path], fieldinfo, true);
							}
							changed |= InspectRoot(name, type, ref changedData, visualizer, mark);
						}
					}
					else
					{
						changed |= InspectRoot(name, type, ref changedData, visualizer, mark);
					}

					if (changedData != null && (alwaysShowChildren || isFoldout[path]))
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

		private bool InspectRoot(string name, Type type, ref object data, VisualizerBase visualizer, IMark mark)
		{
			using (new EditorGUILayout.HorizontalScope())
			{
				bool changed = visualizer.InspectSelf(this, name, ref data, type);
				if (type != null && type.IsClass)
				{
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

		private static object CreateClassInstance(Type type, VisualizerBase visualizer, IMark mark)
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

		// Match Order:
		//	* IMark
		//  * type is null -> Primitive
		//  * type.isPrimitive
		//  * type.isEnum
		//  * Registered concrete type that equals to the current type
		//  * Registered type that is the first matched exact type or generice base type to a ancestor
		//		Exact type first
		//  * Registered interface
		//		If multiply interfaces match, the result is unspecified, depend on the sequence of GetInterfaces()
		//  * Finally, if no visualizer is matched, return composite visualizer
		private VisualizerBase GetVisualizor(Type type, IMark mark)
		{
			if (mark != null)
				return FindMarkVisualizer(mark);
			if (type == null)
				return primitiveAndNullVisualizer;

			VisualizerBase v;
			if (!cachedVisualizer.TryGetValue(type, out v))
			{
				v = FindVisualizor(type);
				cachedVisualizer[type] = v;
			}
			return v;
		}

		private VisualizerBase FindVisualizor(Type type)
		{
			if (type.IsPrimitive)
				return primitiveAndNullVisualizer;

			if (type.IsEnum)
				return enumVisualizer;

			if (rules.ContainsKey(type))
				return rules[type];

			VisualizerBase visualizor = FindBaseTypeVisualizer(type);
			if (visualizor != null)
				return visualizor;

			visualizor = FindInterfaceVisualizer(type);
			if (visualizor != null)
				return visualizor;

			return compositeVisualizer;
		}

		private VisualizerBase FindMarkVisualizer(IMark mark)
		{
			var t = mark.GetType();
			if (markRules.ContainsKey(t))
				return markRules[t];

			throw new NotImplementedException(mark.ToString());
		}

		private VisualizerBase FindBaseTypeVisualizer(Type type)
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

		private VisualizerBase FindInterfaceVisualizer(Type type)
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

		private static void AssertValidSetupState()
		{
			if (cachedVisualizer.Count != 0)
				throw new InvalidOperationException("Can not setup visualizer after Inspect()");
		}
	}
}
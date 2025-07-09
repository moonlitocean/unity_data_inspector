using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace DataInspector
{
	public partial class Inspector
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

			public int listBucketSize = 30;	// If container (list or dictionary) is too large, the elements would not be shown straight forward
											// under the container but instead devided into buckets, with incremental key range. 
											// listBucketSize defines the size of a bucket. 
											//	
											// e.g. for list of size 100 and bucket size 30, the buckets would be:
											//	 0~29,  30~59,  60~89, 90~99
											// 
											// There can be multilevel bucket if the size is super large.
			public int showSearchAtSize = 10;	// If container is big enough, show search box

			// TODO: Only supports CompositeVisualizer. Should apply to other visalizers.
			public bool showNonPublicFields;
			public bool showStaticFields;
			public bool showProperties;
			public bool sortFields;
		}

		public readonly Options options = new Options();
		public readonly Dictionary<string, bool> isFoldout = new Dictionary<string, bool>(StringComparer.Ordinal);

		private readonly Dictionary<Type, VisualizerBase> cachedVisualizer = new Dictionary<Type, VisualizerBase>();

		///////////////////////////////////////////////////////////////
		// Visualizors in three categories: special, type rules, and IMark rules
		private VisualizerBase primitiveAndNullVisualizer;
		private VisualizerBase compositeVisualizer;
		private VisualizerBase enumVisualizer;
		private readonly Dictionary<Type, VisualizerBase> rules = new Dictionary<Type, VisualizerBase>();
		private readonly Dictionary<Type, VisualizerBase> markRules = new Dictionary<Type, VisualizerBase>();
		private readonly Stack<bool> parentIsAlwaysShow = new Stack<bool>();
		private bool inInspector;

		///////////////////////////////////////////////////////////////
		// Extra init step
		private static readonly Dictionary<string, Action<Inspector>> OnRegisterDefaultVisualizers = new Dictionary<string, Action<Inspector>>();

		///////////////////////////////////////////////////////////////
		// When you change the value under editing textbox in OnGUI, e.g. Click a "Move up" button
		// to move array elem, move applied immediately, then comes the value change event over a
		// wrong target.
		// 
		// To avoid the unexpected change, we pause value change for a whole frame.
		// see DropFocus_SkipValueChangeOneFrame()
		private enum SkipOneFrameStep
		{
			None,
			Step0_WaitNextLayout,
			Step1_WaitNextNonLayout,
			Step2_WaitNextLayout,
		}
		private static SkipOneFrameStep skipValueChange = SkipOneFrameStep.None;

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
			rules.Add(typeof(Type), new StaticVisualizer());

			/////////////////////////////////////////////////////////////////////////
			// type --> visualize
			// C# basic types
			rules.Add(typeof(string), new StringVisualizer());
			rules.Add(typeof(MulticastDelegate), new MulticastDelegateVisualizer());
			rules.Add(typeof(DateTime), new DateTimeVisualizer());
			rules.Add(typeof(TimeSpan), new TimeSpanVisualizer());

			// C# containers
			rules.Add(typeof(Array), new ArrayVisualizer());
			rules.Add(typeof(List<>), new ListVisualizer());
			rules.Add(typeof(IDictionary), new DictionaryVisualizer());
			rules.Add(typeof(HashSet<>), new HashSetVisualizer());

			// UnityEngine types
			rules.Add(typeof(UnityEngine.Object), new UnityObjectVisualizer());

			var unityTypeVisualizer = new UnityTypeVisualizer();
			rules.Add(typeof(Color), unityTypeVisualizer);
			rules.Add(typeof(Vector2), unityTypeVisualizer);
			rules.Add(typeof(Vector3), unityTypeVisualizer);
			rules.Add(typeof(Vector4), unityTypeVisualizer);
			rules.Add(typeof(Bounds), unityTypeVisualizer);
			rules.Add(typeof(Rect), unityTypeVisualizer);
			rules.Add(typeof(AnimationCurve), unityTypeVisualizer);

			// Markers
			markRules.Add(typeof(UnixTimestampAttribute), new UnixTimeStampVisualizer());

			// Extension
			foreach (var extension in OnRegisterDefaultVisualizers)
				extension.Value(this);
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
			Action<object> OnValueChanged = null,
			Action OnGUIDrawRootButtons = null)
		{
			if (inInspector)
				return InspectInternal(name, path, data, type, mark, OnValueChanged, OnGUIDrawRootButtons);

			try
			{
				inInspector = true;
				TickSkipValueChangeState();
				GUITools.Setup();
				return InspectInternal(name, path, data, type, mark, OnValueChanged, OnGUIDrawRootButtons);
			}
			finally
			{
				inInspector = false;
			}
		}

		public bool InspectInternal(string name, string path, object data,
			Type type = null,
			IMark mark = null,
			Action<object> OnValueChanged = null,
			Action OnGUIDrawRootButtons = null)
		{
			if (data != null)
				type = data.GetType();

			GUITools.SetLabelWidth(options.labelWidth);
			VisualizerBase visualizer = GetVisualizer(type, mark);
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
					// Note: to avoid infinite expand that may cause by alwaysShowChildren,
					// If parentAlwaysShowChild, then current node ignores alwaysShowChildren.
					var parentAlwaysShowChild = parentIsAlwaysShow.Count > 0 && parentIsAlwaysShow.Peek();
					bool alwaysShowChildren = !parentAlwaysShowChild && visualizer.AlwaysShowChildren();
					if (!alwaysShowChildren)
					{
						using (GUITools.HorizontalScope())
						{
							var width = options.labelWidth - options.indentOffset * GUITools.GetIndentLevel();
							using (GUITools.HorizontalScope(width))
							{
								isFoldout[path] = GUITools.Foldout(isFoldout.ContainsKey(path) && isFoldout[path], fieldinfo);
							}
							changed |= InspectRoot(name, path, type, ref changedData, visualizer, mark, OnGUIDrawRootButtons);
						}
					}
					else
					{
						changed |= InspectRoot(name, path, type, ref changedData, visualizer, mark, OnGUIDrawRootButtons);
					}

					if (changedData != null && (alwaysShowChildren || isFoldout[path]))
					{
						try
						{
							parentIsAlwaysShow.Push(alwaysShowChildren);
							using(GUITools.Indent())
								changed |= visualizer.InspectChildren(this, path, ref changedData, type);
						}
						finally
						{
							parentIsAlwaysShow.Pop();
						}
					}
				}
				else
				{
					changed |= InspectRoot(fieldinfo, path, type, ref changedData, visualizer, mark, OnGUIDrawRootButtons);
				}
			}

			if (changed && OnValueChanged != null)
			{
				OnValueChanged(changedData);
			}
			return changed;
		}

		public static bool IsSkipValueChangeOneFrame()
		{
			return skipValueChange != SkipOneFrameStep.None;
		}

		public static void DropFocus_SkipValueChangeOneFrame()
		{
			GUI.FocusControl(null);
			skipValueChange = SkipOneFrameStep.Step0_WaitNextLayout;
		}

		private static void TickSkipValueChangeState()
		{
			switch (skipValueChange)
			{
				case SkipOneFrameStep.Step0_WaitNextLayout:
					if (Event.current.type == EventType.Layout)
						skipValueChange = SkipOneFrameStep.Step1_WaitNextNonLayout;
					break;

				case SkipOneFrameStep.Step1_WaitNextNonLayout:
					if (Event.current.type != EventType.Layout && Event.current.type != EventType.Used)
						skipValueChange = SkipOneFrameStep.Step2_WaitNextLayout;
					break;

				case SkipOneFrameStep.Step2_WaitNextLayout:
					if (Event.current.type == EventType.Layout)
						skipValueChange = SkipOneFrameStep.None;
					break;
			}
		}

		private bool InspectRoot(string name, string path, Type type, ref object data, VisualizerBase visualizer, IMark mark, Action OnGUIDrawRootButtons)
		{
			using (GUITools.HorizontalScope())
			{
				bool changed = visualizer.InspectSelf(this, name, ref data, type);
				if (OnGUIDrawRootButtons != null)
				{
					OnGUIDrawRootButtons();
				}

				if (type != null && (type.IsClass || type.IsInterface || type == typeof(Type)))
				{
					if (data != null)
					{
						using (GUITools.Color(Color.red))
						{
							if (GUILayout.Button("-", GUILayout.Width(20)))
							{
								data = null;
								changed = true;
							}
						}
					}
					else
					{
						#if UNITY_EDITOR
							bool enabled = TypeTools.HasDerivedCreateDefaultInstance(type);
							using (GUITools.Enabled(enabled))
							using (GUITools.Color(enabled? Color.green : Color.gray))
							{
								if (GUILayout.Button("+", GUILayout.Width(20)))
								{
									CreateInstanceWindow.CreateInstance(path, type, visualizer, mark);
								}
							}

							if (CreateInstanceWindow.HasResult(path))
							{
								data = CreateInstanceWindow.TakeResult();
								changed = true;
							}
						#endif
					}
				}
				return changed;
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
		private VisualizerBase GetVisualizer(Type type, IMark mark)
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

		private void AssertValidSetupState()
		{
			if (cachedVisualizer.Count != 0)
				throw new InvalidOperationException("Can not setup visualizer after Inspect()");
		}
	}
}
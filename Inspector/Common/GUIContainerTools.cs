using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DataInspector
{
	public static class GUIContainerTools
	{
		public interface IDictParser
		{
			int Size(object collection);
			object[] Keys(object collection);
			object Get(object dict, object key);
			void Set(object dict, object key, object value);
			Type ValueType(object dict);
			bool Resizable(object collection);						// 如果Resizable为false，则不需要实现Resize
			object Resize(object collection, int size);				// 返回新大小的容器（可以是自己）
		}

		private class DictionaryDisplay
		{
			public int bucketSize;
			public object[] keys;				// 原始key列表，用于判断Dictionary是否变化。如果不需要判断，则可以为null
			public object[] resultKeys;			// 过滤、排序过的key，用于显示。
		}

		private class SearchInputState
		{
			// 为提高手感，有一个较为复杂的输入逻辑。
			// 对于每段连续输入（打字间隔<0.5秒），在连入输入开始后一秒以上的时候，每秒更新一次filter。连续输入结束时立即更新filter
			public string text;
			public DateTime inputStart;
			public DateTime lastTextChange;
			public DateTime lastFilterChange;

			public string filter;
			public bool changed;
		}

		private class DictionaryGUIState
		{
			public object dict;			// 实际的字典（抽象的key-value对）
			public IDictParser parser;	// 字典对应的解析器

			public SearchInputState searchInput;
			public DictionaryDisplay display;

			public int Size() { return parser.Size(dict); }
			public object[] Keys() { return parser.Keys(dict); }
			public object Get(object key) { return parser.Get(dict, key); }
			public void Set(object key, object value) { parser.Set(dict, key, value); }
			public Type ValueType() {return parser.ValueType(dict);}
			public bool Resizeable(){return parser.Resizable(dict);}
			public object Resize(int size){return parser.Resize(dict, size);}
		}

		private static readonly Dictionary<WeakReference, DictionaryGUIState> guiCache = new Dictionary<WeakReference, DictionaryGUIState>();
		private static DateTime lastCacheRecycleCheck;

		// 如果数据发生了修改，则返回true。
		//
		// parser用于解析dict对象
		public static bool EditDict(Inspector inspector, string path, object dict, IDictParser parser)
		{
			UpdateCheck();

			DictionaryGUIState state = GetOrCreateCachedState(dict, parser);
			DrawSearchInput(state);
			RebuildDisplayIfNeed(state, inspector.options.listBucketSize);

			return Traversal(inspector, path, state, 0, state.display.resultKeys.Length);
		}

		private static void RebuildDisplayIfNeed(DictionaryGUIState state, int bucketSize)
		{
			if (state.display == null || 
				Event.current.type == EventType.Layout && (state.searchInput.changed || IsKeyChanged(state) || state.display.bucketSize != bucketSize)
			)
			{
				var display = new DictionaryDisplay();
				display.keys = state.Keys();
				display.bucketSize = bucketSize;

				if (!string.IsNullOrEmpty(state.searchInput.text))
					display.resultKeys = Sorted(FilterKeys(display.keys, state.searchInput));
				else
					display.resultKeys = Sorted(display.keys);
				state.display = display;
			}
		}

		private static bool IsKeyChanged(DictionaryGUIState state)
		{
			return state.display != null && !ArrayEqual(state.display.keys, state.Keys());
		}

		private static void DrawSearchInput(DictionaryGUIState state)
		{
			if (state.searchInput == null)
				state.searchInput = new SearchInputState();

			var now = DateTime.Now;
			SearchInputState input = state.searchInput;
			using (GUITools.Indent())
			{
				using (GUITools.HorizontalScope())
				{
					var newInput = GUITools.TextField("Search: ", input.text ?? "");
					if (newInput != input.text)
					{
						input.text = newInput;
						input.lastTextChange = now;
						if (input.inputStart.Ticks == 0)
							input.inputStart = now;
					}
					else
					{
						if (Math.Abs((now - input.lastTextChange).TotalSeconds) > 0.5)
							input.inputStart = new DateTime();
					}

					if (input.text != input.filter &&
						(
							input.inputStart.Ticks == 0 ||
							Math.Abs((now - input.lastFilterChange).TotalSeconds) > 1 &&
							Math.Abs((now - input.inputStart).TotalSeconds) > 1
						))
					{
						input.changed = true;
						input.filter = input.text;
						input.lastFilterChange = DateTime.Now;
					}
					else
					{
						input.changed = false;
					}
				}
			}
		}

		private static object[] FilterKeys(object[] keys, SearchInputState search)
		{
			if (!string.IsNullOrEmpty(search.filter))
			{
				return keys
					.Where(o => o != null && o.ToString().IndexOf(search.filter, StringComparison.Ordinal) != -1)
					.ToArray();
			}
			return keys;
		}

		private static void UpdateCheck()
		{
			if (DateTime.Now.Second == lastCacheRecycleCheck.Second)
				return;
			lastCacheRecycleCheck = DateTime.Now;

			RemoveDeadCache();
		}

		private static void RemoveDeadCache()
		{
			foreach (var weakRef in guiCache.Keys)
			{
				if (!weakRef.IsAlive)
					guiCache.Remove(weakRef);
			}
		}

		private static DictionaryGUIState GetOrCreateCachedState(object dict, IDictParser parser)
		{
			var weakKey = guiCache.FirstOrDefault(o => o.Key.Target == dict).Key;
			if (weakKey == null)
			{
				weakKey = new WeakReference(dict);
				guiCache[weakKey] = new DictionaryGUIState
				{
					dict = dict,
					parser = parser,
				};
			}

			return guiCache[weakKey];
		}

		private static bool Traversal(Inspector inspector, string path, DictionaryGUIState state, int start, int end)
		{
			bool changed = false;
			var bucketSize = Math.Max(state.display.bucketSize, 2);
			var valueType = state.ValueType();

			if (end - start <= bucketSize)
			{
				for (int index = start; index < end; ++index)
				{
					changed |= InspectElement(inspector, path, state.display.resultKeys[index], state, valueType);
				}
			}
			else
			{
				// Allow multiple level of bucket, calculate the current step
				int step = bucketSize;
				while (step * bucketSize < end - start)
					step *= bucketSize;

				for (int inner = start; inner < end; inner += step)
				{
					int innerEnd = Math.Min(end, inner + step);

					var innerKeyBegin = state.display.resultKeys[inner];
					var innerKeyEnd = state.display.resultKeys[innerEnd - 1];
					var label = FormatKeyRange(innerKeyBegin, innerKeyEnd);
					var foldoutPath = path + "[" + innerKeyBegin + "~" + innerKeyEnd + "]";

					inspector.isFoldout[foldoutPath] = GUITools.Foldout(inspector.isFoldout.ContainsKey(foldoutPath) && inspector.isFoldout[foldoutPath], label);
					if (inspector.isFoldout[foldoutPath])
					{
						using (GUITools.Indent())
						{
							changed |= Traversal(inspector, path, state, inner, innerEnd);
						}
					}
				}
			}

			return changed;
		}

		private static bool InspectElement(Inspector inspector, string path, object key, DictionaryGUIState state, Type dictValueType)
		{
			object value = state.Get(key);
			Type valueType = value != null ? value.GetType() : dictValueType;
			string extraTypeInfo = string.Empty;
			if (valueType != dictValueType)
			{
				extraTypeInfo = string.Format("   ({0})", valueType.Name);
			}
			string name = key != null ? CutName(key.ToString()) : "null";
			return inspector.Inspect(name + extraTypeInfo, path + "." + name, value, valueType, null, v =>
			{
				if (value != v)
					state.Set(key, v);
			});
		}

		////////////////////////////////////////////////////////////////////////////////////
		// cache的工具函数
		private static bool ArrayEqual(object[] l, object[] r)
		{
			if (l.Length != r.Length)
				return false;
			for (var i = 0; i < l.Length; i++)
			{
				if(l[i] != r[i])
					return false;
			}
			return true;
		}

		////////////////////////////////////////////////////////////////////////////////////
		// ui key rules
		private static object[] Sorted(object[] keys)
		{
			bool isInt = false;
			bool isString = false;
			foreach (var key in keys)
			{
				if (key is int)
				{
					isInt = true;
					break;
				}
				else if (key is string)
				{
					isString = true;
					break;
				}
			}

			var result = keys.Clone() as object[];
			if (isInt)
			{
				var ar = keys.Cast<int>().ToArray();
				Array.Sort(ar, result);
			}
			else if (isString)
			{
				var ar = keys.Cast<string>().ToArray();
				Array.Sort(ar, result, StringComparer.Ordinal);
			}
			else
			{
				var ar = keys.Select(o => o.ToString()).ToArray();
				Array.Sort(ar, result, StringComparer.Ordinal);
			}
			return result;
		}

		private static string FormatKeyRange(object begin, object end)
		{
			return CutName(begin) + ", ...";
		}

		private static string CutName(object key)
		{
			string s = key != null ? key.ToString() : "null";
			if (s.IndexOf('\n') != -1)
				s = s.Replace("\n", "");

			if (s.Length <= 27)
				return s;
			else
				return s.Substring(0, 12) + "..." + s.Substring(s.Length - 12);
		}
	}
}

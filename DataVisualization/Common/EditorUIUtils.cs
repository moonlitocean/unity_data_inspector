using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FF.Editor;
using UnityEditor;

public static class EditorUIUtils
{
	private class DictionaryDisplay
	{
		public readonly object[] keys;		// 原始key列表，用于判断Dictionary是否变化。如果不需要判断，则可以为null
		public readonly object[] sorted;		// 排序过的key，用于显示。
		public readonly string[] cachedGroupTitles;

		public DictionaryDisplay(IDictionary dict)
		{
			keys = GetKeys(dict);
			sorted = Sorted(keys);
			cachedGroupTitles = BuildGroupTitles(dict, sorted);
		}

		public DictionaryDisplay(IDictionary dict, object[] sorted)
		{
			keys = null;
			this.sorted = sorted;
			cachedGroupTitles = BuildGroupTitles(dict, sorted);
		}
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
		public object target;			// 实际的字典
		public IDictionary data;		// 数据，可能是复制的

		public DictionaryDisplay display;
		public SearchInputState searchInput;
		public DictionaryDisplay searchResult;
	}

	private const int GroupElemsCount = 100;
	private static readonly Dictionary<WeakReference, DictionaryGUIState> guiCache = new Dictionary<WeakReference, DictionaryGUIState>();
	private static DateTime lastCacheRecycleCheck;

	public static bool EditElems<T>(IList<T> items, Func<T, bool> ShowItem, Dictionary<string, bool> foldout, string foldoutRoot)
	{
		bool changed = false;
		if (items.Count <= GroupElemsCount)
		{
			foreach (var elem in items)
			{
				changed |= ShowItem(elem);
			}
		}
		else
		{
			for (int i = 0; i < items.Count; i += GroupElemsCount)
			{
				string key = FormatKeyRange(items, i, Math.Min(i + GroupElemsCount - 1, items.Count - 1));

				var foldkey = foldoutRoot + "." + key;
				if (!foldout.ContainsKey(foldkey))
				{
					foldout[foldkey] = false;
				}
				foldout[foldkey] = GUITools.Foldout(foldout[foldkey], key, true);

				if (foldout[foldkey])
				{
					++EditorGUI.indentLevel;
					for (int index = 0; index < GroupElemsCount && i + index < items.Count; index++)
					{
						changed |= ShowItem(items[i + index]);
					}
					--EditorGUI.indentLevel;
				}
			}
		}
		return changed;
	}

	// 如果数据发生了修改，则返回true
	// bool EditItem(key, value) 返回数据项是否发生了修改
	public static bool EditElems(IDictionary dict, Func<object, object, bool> EditItem, Dictionary<string, bool> foldout, string foldoutRoot)
	{
		return EditDictionaryElems(dict, EditItem, foldout, foldoutRoot);
	}

	//////////////////////////////////////////////////////////////////////////////////////////////////
	// 实现
	private static bool EditDictionaryElems(object dict, Func<object, object, bool> EditItem, Dictionary<string, bool> foldout, string foldoutRoot)
	{
		UpdateCheck();
		DictionaryGUIState state = GetCachedDictionaryState(dict);
		PrepareData(state);

		DrawSearchInput(state);
		if (state.searchInput.changed)
		{
			RebuildSearchResult(state);
		}

		if (state.searchResult != null)
		{
			EditorGUILayout.LabelField(string.Format("{0}个结果：", state.searchResult.sorted.Length));
			return ShowGrouped(state.data, EditItem, foldout, foldoutRoot, state.searchResult);
		}
		else
		{
			UpdateDisplay(state);
			return ShowGrouped(state.data, EditItem, foldout, foldoutRoot, state.display);
		}
	}

	private static void DrawSearchInput(DictionaryGUIState state)
	{
		if (state.searchInput == null)
			state.searchInput = new SearchInputState();

		var now = DateTime.Now;
		SearchInputState input = state.searchInput;
		using (GUITools.Indent())
		using (new EditorGUILayout.HorizontalScope("box"))
		{
			var newInput = EditorGUILayout.TextField("搜索: ", input.text);
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

	private static void UpdateDisplay(DictionaryGUIState state)
	{
		if (state.display == null || !IsKeyEquals(state.data, state.display.keys))
		{
			state.display = new DictionaryDisplay(state.data);
		}
	}

	private static void RebuildSearchResult(DictionaryGUIState guiState)
	{
		guiState.searchResult = null;

		if (!string.IsNullOrEmpty(guiState.searchInput.filter))
		{
			var startWith = new List<object>();
			var inMiddle = new List<object>();
			foreach (var key in Sorted(GetKeys(guiState.data)))
			{
				var strKey = key.ToString();
				var index = strKey.IndexOf(guiState.searchInput.filter, StringComparison.Ordinal);
				if (index == -1)
					continue;

				if (strKey == guiState.searchInput.filter)
					startWith.Insert(0, key);
				else if (index == 0)
					startWith.Add(key);
				else
					inMiddle.Add(key);
			}

			guiState.searchResult = new DictionaryDisplay(guiState.data, startWith.Concat(inMiddle).ToArray());
		}
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

	private static DictionaryGUIState GetCachedDictionaryState(object dict)
	{
		var weakKey = guiCache.FirstOrDefault(o => o.Key.Target == dict).Key;
		if (weakKey == null)
		{
			weakKey = new WeakReference(dict);
			guiCache[weakKey] = new DictionaryGUIState
			{
				target = dict
			};
		}

		return guiCache[weakKey];
	}

	private static bool ShowGrouped(IDictionary dict, Func<object, object, bool> EditItem, Dictionary<string, bool> foldout, string foldoutRoot, DictionaryDisplay cache)
	{
		bool changed = false;
		object[] keys = cache.sorted;
		if (dict.Count <= GroupElemsCount)
		{
			foreach (object key in keys)
			{
				changed |= EditItem(key, dict[key]);
			}
		}
		else
		{
			for (int i = 0; i < keys.Length; i += GroupElemsCount)
			{
				string groupTitle = cache.cachedGroupTitles[i / GroupElemsCount];
				string foldkey = foldoutRoot + "." + groupTitle;
				if (!foldout.ContainsKey(foldkey))
				{
					foldout[foldkey] = false;
				}
				foldout[foldkey] = GUITools.Foldout(foldout[foldkey], groupTitle, true);

				if (foldout[foldkey])
				{
					++EditorGUI.indentLevel;
					for (int index = 0; index < GroupElemsCount && i + index < keys.Length; index++)
					{
						var key = keys[i + index];
						changed |= EditItem(key, dict[key]);
					}
					--EditorGUI.indentLevel;
				}
			}
		}
		return changed;
	}

	////////////////////////////////////////////////////////////////////////////////////
	// cache的工具函数
	private static void PrepareData(DictionaryGUIState state)
	{
		if (state.target is IDictionary)
		{
			state.data = state.target as IDictionary;
		}
	}

	private static object[] GetKeys(IDictionary dict)
	{
		object[] keys = new object[dict.Count];
		int index = 0;
		foreach (object key in dict.Keys)
		{
			keys[index++] = key;
		}
		return keys;
	}

	private static bool IsKeyEquals(IDictionary dict, object[] keys)
	{
		if (dict.Count != keys.Length)
			return false;
		int index = 0;
		foreach (object key in dict.Keys)
		{
			if (index >= keys.Length || keys[index++] != key)
				return false;
		}
		return true;
	}

	////////////////////////////////////////////////////////////////////////////////////
	// ui上文字展示规则
	private static string[] BuildGroupTitles(IDictionary dict, object[] sorted)
	{
		string[] result = new string[(sorted.Length - 1) / GroupElemsCount + 1];
		for (int i = 0; i < sorted.Length; i += GroupElemsCount)
		{
			result[i / GroupElemsCount] = FormatKeyRange(dict, sorted[i], sorted[Math.Min(i + GroupElemsCount - 1, sorted.Length - 1)]);
		}
		return result;
	}

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

	////////////////////////////////////////////////////////////////////////////////////
	// ui上文字展示规则
	private static string FormatKeyRange(IDictionary dict, object begin, object end)
	{
		if (IsPrintable(dict[begin]) && IsPrintable(dict[end]))
			return String.Format("{0,-50}  {1,-50}", CutName(begin), CutName(dict[begin]));
		else
			return String.Format("{0}", CutName(begin));
	}

	private static string FormatKeyRange<T>(IList<T> items, int begin, int end)
	{
		if (IsPrintable(items[begin]) && IsPrintable(items[end]))
			return String.Format("{0,-50}  {1,-50}", begin, CutName(items[begin]));
		else
			return String.Format("{0}", begin);
	}

	private static bool IsPrintable<T>(T item)
	{
		return item is int || item is string;
	}

	private static string CutName(object key)
	{
		string s = key.ToString();
		if (s.IndexOf('\n') != -1)
			s = s.Replace("\n", "");

		if (s.Length <= 50)
			return s;
		else
			return s.Substring(0, 25) + "..." + s.Substring(s.Length - 25);
	}
}

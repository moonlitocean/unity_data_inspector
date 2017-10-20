using System;
using System.Collections.Generic;
using UnityEditor;
using LuaInterface;

namespace DataInspector
{
    public class LuaTableVisualizer : VisualizerBase
    {
        public override bool HasChildren()
        {
            return true;
        }

        public override bool InspectSelf(Inspector inspector, string name, ref object data, Type type)
        {
            var curTable = data as LuaTable;
            if (curTable == null)
                return false;

            EditorGUILayout.LabelField(curTable.ToString());
            return false;
        }

        public override bool InspectChildren(Inspector inspector, string path, ref object data, Type type)
        {
            var curTable = data as LuaTable;
            if (curTable == null)
                return false;
            var _tables = _SpawnDict();
            var _funs = _SpawnDict();
            var _others = _SpawnDict();

            var enumerator = curTable.ToDictTable().GetEnumerator();
            while (enumerator.MoveNext())
            {
                var pair = enumerator.Current;
                if (pair.Value is LuaTable)
                {
                    _tables.Add(pair.Key, pair.Value);
                }
                else if (pair.Value is LuaFunction)
                {
                    _funs.Add(pair.Key, pair.Value);
                }
                else
                {
                    _others.Add(pair.Key, pair.Value);
                }
            }
            enumerator.Dispose();

            var e = _tables.GetEnumerator();
            while (e.MoveNext())
            {
                inspector.Inspect(e.Current.Key.ToString(), path + "." + e.Current.Key, e.Current.Value);
            }

            var metatable = curTable.GetMetaTable();
            if (metatable != null)
            {
                inspector.Inspect(".metatable", path + "." + ".metatable", metatable);
            }

            e = _others.GetEnumerator();
            while (e.MoveNext())
            {
                var pair = e.Current;
                inspector.Inspect(pair.Key.ToString(), path + "." + pair.Key, pair.Value, null, null, 
                    (v) => _SetTableValue(curTable, pair.Key, v));
            }

            e = _funs.GetEnumerator();
            while (e.MoveNext())
            {
                EditorGUILayout.LabelField(e.Current.Key.ToString(), "LuaFunction");
            }
            _RecycleDict(_tables);
            _RecycleDict(_others);
            _RecycleDict(_funs);
            return false;
        }

        private static void _SetTableValue(LuaTable table, object key, object value)
        {
            if (key is double)
            {
                table[(int) (double) key] = value;
            }
            else
            {
                table[key as string] = value;
            }
        }

        private Dictionary<object, object> _SpawnDict()
        {
            if (_dictPool.Count == 0)
                return new Dictionary<object, object>();
            return _dictPool.Pop();
        }

        private void _RecycleDict(Dictionary<object, object> obj)
        {
            obj.Clear();
            _dictPool.Push(obj);
        }

        private Stack<Dictionary<object, object>> _dictPool = new Stack<Dictionary<object,object>>();
    }
}
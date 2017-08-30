using System;
using System.Collections.Generic;
using FF.Editor;
using UnityEditor;
using UnityEngine;
#pragma warning disable 169

namespace Assets
{
	public class Base
	{
		public int x = 2;
	}

	public class Derived : Base
	{
		public int y = 3;
	}

	public class Derived2 : Base
	{
		public int z = 4;
	}

	public class Test
	{
		private enum EGoods
		{
			Banana,
			Apple,
			Carrot,
		};
		[Flags]
		private enum EMaskedGoods
		{
			Banana = 1,
			Apple = 2,
			Carrot = 4,
		};

		//----------------------------------------------------
		// Basic Visualizers
		// StaticVisualizer
		public static Test s_test = new Test();
		public static int s_int = 20;

		// AtomVisualizer
		public byte _byte = 1;
		public sbyte _sbyte = -2;
		public short _short = -3;
		public ushort _ushort = 4;
		public int _int = -5;
		public uint _uint = 6;
		public long _long = -7;
		public ulong _ulong = 8;
		public float _float = 9.9f;
		public double _double = 10.9;
		public char _char = 't';
		public bool _bool = false;
		public string _string = "hello";

		// protected, private
		protected int protected_int = 11;
		private int private_int = 12;

		// EnumVisualizer
		private EGoods _enum = EGoods.Banana;
		private EMaskedGoods _mask = EMaskedGoods.Banana | EMaskedGoods.Carrot;

		// CompositeVisualizer
		public Base pbase = new Derived();

		// ListArrayVisualizer
		// DictionaryVisualizer
		// HashSetVisualizer
		public int[] arrayi = new int[3];
		public List<int> listi = new List<int>();
		public Dictionary<int, Derived> dicti = new Dictionary<int, Derived>
		{
			{10, new Derived()},
			{20, new Derived()},
		};
		public HashSet<int> hashi = new HashSet<int>()
		{
			30,
			40
		};

		// UnityObjectVisualizer
		public Texture2D tex;
		public GameObject go;

		//----------------------------------------------------
		// Extended Visualizers
		// DateTimeVisualizer
		// TimeSpanVisualizer
		// 


		//----------------------------------------------------
		// Final Test
		public object o = new int[3];
	}

	public class Watch : EditorWindow
	{
		private readonly DataVisualization v = new DataVisualization();
		private Vector2 scroll;

		[MenuItem("Window/Watch")]
		public static void ShowWindow()
		{
			GetWindow<Watch>();
		}

		public void OnGUI()
		{
			using (GUITools.Scroll(ref scroll))
			{
				v.Inspect("Options", "options", v.options);
				v.Inspect("Selection", "selection", Selection.objects);
				v.Inspect("Watch Window", "Watch Window", this);
				v.Inspect("Test", "Test", typeof(Test));
			} 
		}

		public void OnSelectionChange()
		{
			Repaint();
		}
	}
}

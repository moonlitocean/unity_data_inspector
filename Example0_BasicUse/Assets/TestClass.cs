using System;
using System.Collections.Generic;
using DataInspector;
using UnityEngine;

#pragma warning disable 414

public class TestClass
{
	public class Base
	{
		public int basedata = 2;
	}

	public class Derived : Base
	{
		public int derived = 3;
	}

	public enum EGoods
	{
		Banana,
		Apple,
		Carrot,
	};

	[Flags]
	public enum EMaskedGoods
	{
		Banana = 1,
		Apple = 2,
		Carrot = 4,
	};

	public interface IBox {}
	public class Box : IBox
	{
		public int type = 23;
	}

	//----------------------------------------------------
	// Basic Visualizers
	// StaticVisualizer
	public static TestClass s_test = new TestClass();
	public static int s_int = 20;
	public static Base _s_base = new Derived();

	private static bool sp_bool = true;
	private static int sp_prop { get; set; }
	private static int sp_readonly_prop { get { return 10; } }
	private static int sp_badprop { get { throw new Exception(); } }
	private static int sp_fail_set_prop { get { return 20; } set { throw new Exception(); } }

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
	public EGoods _enum = EGoods.Banana;
	public EMaskedGoods _mask = EMaskedGoods.Banana | EMaskedGoods.Carrot;

	// CompositeVisualizer
	public Base _base = new Derived();

	// interface
	public IBox _ibox = new Box();
	public IBox _iboxnull = null;


	// ListVisualizer
	// DictionaryVisualizer
	// HashSetVisualizer
	public int[] _arrayi = new int[3];
	public List<int> _listi = new List<int>();
	public List<Base> _listbase = new List<Base>();

	public Dictionary<int, Derived> _dicti = new Dictionary<int, Derived>
	{
		{10, new Derived()},
		{20, new Derived()},
	};
	public HashSet<int> _hashi = new HashSet<int>()
	{
		30,
		40
	};

	// UnityObjectVisualizer
	public Texture2D _tex;
	public GameObject _gameobject;

	// UnityTypeVisualizer
	public Color _color = Color.yellow;
	public Vector2 _vector2 = new Vector2(1,2);
	public Vector3 _vector3 = new Vector3(3,4,5);
	public Vector4 _vector4 = new Vector4(6, 7, 8, 9);
	public Bounds _bounds = new Bounds(new Vector3(10, 11, 12), new Vector3(13, 14, 15));
	public AnimationCurve _curve = new AnimationCurve(new[] { new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1f, 0) });
	public Rect _rect = new Rect(16, 17, 18, 19);
		
	//----------------------------------------------------
	// Extended Visualizers
	// DateTimeVisualizer
	// TimeSpanVisualizer
	public DateTime _datetime = new DateTime(2010, 1, 1);
	public TimeSpan _timespan = TimeSpan.FromDays(2.5);
	[UnixTimestamp] public int _timestamp = 1262304000; // 2010/01/01 00:00:00 UTC

	//----------------------------------------------------
	// Final TestClass
	public object _object = new int[3];
	public List<int> _largelist = new List<int>(1000);
	public Dictionary<string, int> _largedict = new Dictionary<string, int>();

	public TestClass()
	{
		for (int i = 0; i < 1000; ++i)
			_largelist.Add(i * 10);
		for (int i = 0; i < 1000; ++i)
			_largedict["This is a key of very large dictionary where the current key is " + i] = i * 10;
	}
}

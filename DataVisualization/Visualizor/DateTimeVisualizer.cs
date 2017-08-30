using System;
using System.Globalization;
using UnityEditor;
using UnityEngine;

internal class DateTimeVisualizer : DataVisualizer
{
	private readonly static DateTime CTIME_BEGIN = DateTime.SpecifyKind(new DateTime(1970, 1, 1), DateTimeKind.Local);
	private readonly static CultureInfo culture = new CultureInfo("en-US");

	public override bool HasChildren()
	{
		return true;
	}

	public override bool InspectSelf(string name, ref object data, Type type)
	{
		DateTime time = ToTime(data, type);

		string oldstrdate = time.ToString(culture);
		string strdate = EditorGUILayout.TextField(oldstrdate);
		if (oldstrdate != strdate)
			time = ParseDateDateTime(strdate, time);

		if (GUILayout.Button("+", GUILayout.Width(20)))
			time = Add(time, 1);

		if (GUILayout.Button("-", GUILayout.Width(20)))
			time = Add(time, -1);

		if (GUILayout.Button("+10", GUILayout.Width(35)))
			time = Add(time, 10);

		if (GUILayout.Button("-10", GUILayout.Width(35)))
			time = Add(time, -10);

		if (GUILayout.Button("+60", GUILayout.Width(35)))
			time = Add(time, 60);

		if (GUILayout.Button("-60", GUILayout.Width(35)))
			time = Add(time, -60);

		if (GUILayout.Button("Now", GUILayout.Width(50)))
			time = DateTime.Now;

		return ApplyValueTypeData(ref data, ToReturnType(time, type));
	}

	private object ToReturnType(DateTime time, Type type)
	{
		if (type == typeof(long))
		{
			return DateToCTime(time);
		}
		else
		{
			return time;
		}
	}

	public override bool InspectChildren(DataVisualization visualization, string path, ref object data, Type type)
	{
		DateTime time = ToTime(data, type);
		long oldctime = (long)DateToCTime(time);
		visualization.Inspect("ctime", path + ".ctime", oldctime, typeof (long), null, newtime =>
		{
			time = ParseLongDateTime((long)newtime, time);
		});
		return ApplyValueTypeData(ref data, ToReturnType(time, type));
	}

	private static DateTime ToTime(object data, Type type)
	{
		DateTime time;
		if (type == typeof(long))
			time = CTimeToDate((long)data);
		else
			time = (DateTime)data;
		return time;
	}


	private DateTime Add(DateTime dateTime, int seconds)
	{
		return dateTime.AddSeconds(seconds);
	}

	private DateTime ParseDateDateTime(string time, DateTime defaultValue)
	{
		try
		{
			return DateTime.Parse(time);
		}
		catch (Exception)
		{
			return defaultValue;
		}
	}

	private DateTime ParseLongDateTime(long ctime, DateTime defaultValue)
	{
		try
		{
			return CTimeToDate(ctime);
		}
		catch (Exception)
		{
			return defaultValue;
		}
	}

	public static long DateToCTime(DateTime time)
	{
		if (time.Ticks == 0)
			return 0L;

		var ts = time.Subtract(CTIME_BEGIN);
		var ctime = (long)ts.TotalSeconds;

		if (ctime < 0L)
		{
			ctime = 0L;
		}

		return ctime;
	}

	// 从C标准库的时间，即自1970年1月1日起的秒数，转 DateTime。
	// 其中0为特殊值
	public static DateTime CTimeToDate(long time)
	{
		if (time == 0L)
		{
			return new DateTime();
		}

		if (time < 0L)
		{
			time = 0L;
		}

		return CTIME_BEGIN + new TimeSpan(time * TimeSpan.TicksPerSecond);
	}
}
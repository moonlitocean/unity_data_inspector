using System;
using System.Globalization;
using FF;
using UnityEditor;
using UnityEngine;

internal class DateTimeVisualizor : DataVisualizor
{
	private readonly CultureInfo culture = new CultureInfo("en-US");

	public override bool IsFoldable()
	{
		return true;
	}

	public override bool InspectRoot(string name, ref object data, Type type)
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
		{
			if (TimeMan.Instance != null)
				time = TimeMan.Instance.ServerUtcDateTime();
			else
				time = DateTime.Now;
		}

		return ApplyData(ref data, ToReturnType(time, type));
	}

	private object ToReturnType(DateTime time, Type type)
	{
		if (type == typeof(long))
		{
			return TimeUtil.DateToCTime(time);
		}
		else
		{
			return time;
		}
	}

	public override bool InspectContent(DataVisualization visualization, string path, ref object data, Type type)
	{
		DateTime time = ToTime(data, type);
		long oldctime = (long)TimeUtil.DateToCTime(time);
		visualization.Inspect("ctime", path + ".ctime", oldctime, typeof (long), null, newtime =>
		{
			time = ParseLongDateTime((long)newtime, time);
		});
		return ApplyData(ref data, ToReturnType(time, type));
	}

	private static DateTime ToTime(object data, Type type)
	{
		DateTime time;
		if (type == typeof(long))
			time = TimeUtil.CTimeToDate((long)data);
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
			return TimeUtil.CTimeToDate(ctime);
		}
		catch (Exception)
		{
			return defaultValue;
		}
	}
}
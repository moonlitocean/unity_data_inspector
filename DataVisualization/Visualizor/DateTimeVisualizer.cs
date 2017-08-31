using System;
using System.Globalization;
using UnityEditor;

internal class DateTimeVisualizer : DataVisualizer
{
	private readonly static DateTime CTIME_BEGIN = DateTime.SpecifyKind(new DateTime(1970, 1, 1), DateTimeKind.Unspecified);

	public override bool HasChildren()
	{
		return true;
	}

	public override bool InspectSelf(string name, ref object data, Type type)
	{
		var time = (DateTime)data;

		string oldstrdate = time.ToString(CultureInfo.InvariantCulture);
		string strdate = EditorGUILayout.TextField(oldstrdate);
		if (oldstrdate != strdate)
			time = ParseDateTime(strdate, time.Kind, time);

		return ApplyValueIfNotEqual(ref data, time);
	}

	public override bool InspectChildren(DataVisualization visualization, string path, ref object data, Type type)
	{
		var time = (DateTime)data;
		EditorGUILayout.TextField("Unix Time Stamp", ToUnixTimestampString(time));

		var ticks = EditorGUILayout.LongField("Ticks", time.Ticks);
		var kind = (DateTimeKind) EditorGUILayout.EnumPopup("Kind", time.Kind);
		if (ticks != time.Ticks || kind != time.Kind)
		{
			try
			{
				data = new DateTime(ticks, kind);
				return true;
			}
			catch (Exception){}
		}

		return false;
	}

	private DateTime ParseDateTime(string time, DateTimeKind kind, DateTime defaultValue)
	{
		try
		{
			return new DateTime(DateTime.Parse(time).Ticks, kind);
		}
		catch (Exception)
		{
			return defaultValue;
		}
	}

	private static string ToUnixTimestampString(DateTime time)
	{
		if (time >= CTIME_BEGIN)
		{
			var ts = time.Subtract(CTIME_BEGIN);
			var ctime = (uint) ts.TotalSeconds;
			return ctime.ToString(CultureInfo.InvariantCulture);
		}
		else
		{
			return "[Bad date: Under 1970]";
		}
	}
}
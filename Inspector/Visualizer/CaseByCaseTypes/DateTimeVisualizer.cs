using System;
using System.Globalization;

namespace DataInspector
{
	internal class DateTimeVisualizer : VisualizerBase
	{
		private static readonly DateTime CTIME_BEGIN = DateTime.SpecifyKind(new DateTime(1970, 1, 1), DateTimeKind.Utc);

		public override string GetLabelPostfix(Inspector inspector, object data, Type type)
		{
			var time = (DateTime) data;
			return " " + time.Kind;
		}

		public override bool HasChildren()
		{
			return true;
		}

		public override bool InspectSelf(Inspector inspector, string name, ref object data, Type type)
		{
			var time = (DateTime) data;

			string oldstrdate = time.ToString(CultureInfo.CurrentCulture);
			string strdate = GUITools.TextField(oldstrdate);
			if (oldstrdate != strdate)
				time = ParseDateTime(strdate, time.Kind, time);

			return ApplyValueIfNotEqual(ref data, time);
		}

		public override bool InspectChildren(Inspector inspector, string path, ref object data, Type type)
		{
			var time = (DateTime) data;
			GUITools.LabelField("Unix Time Stamp", ToUnixTimestampString(time));

			var ticks = GUITools.LongField("Ticks", time.Ticks);
			var kind = (DateTimeKind)GUITools.EnumPopup("Kind", time.Kind);
			if (ticks != time.Ticks || kind != time.Kind)
			{
				try
				{
					data = new DateTime(ticks, kind);
					return true;
				}
				catch (Exception)
				{
				}
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
			time = time.ToUniversalTime();
			if (time >= CTIME_BEGIN)
			{
				var ts = time.Subtract(CTIME_BEGIN);
				var ctime = (uint) ts.TotalSeconds;
				return ctime.ToString(CultureInfo.InvariantCulture);
			}
			else
			{
				return "[Bad date: Under 1970 UTC]";
			}
		}
	}
}
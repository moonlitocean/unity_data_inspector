using System;

namespace DataInspector
{
	internal class TimeSpanVisualizer : VisualizerBase
	{
		public override bool HasChildren()
		{
			return false;
		}

		public override bool InspectSelf(Inspector inspector, string name, ref object data, Type type)
		{
			var span = (TimeSpan) data;

			string oldstrdate = span.ToString();
			string str = GUITools.TextField(name, oldstrdate);
			if (oldstrdate != str)
				span = ParseTimeSpan(str, span);

			return ApplyValueIfNotEqual(ref data, span);
		}

		private TimeSpan ParseTimeSpan(string str, TimeSpan defaultValue)
		{
			try
			{
				return TimeSpan.Parse(str);
			}
			catch (Exception)
			{
				return defaultValue;
			}
		}
	}
}
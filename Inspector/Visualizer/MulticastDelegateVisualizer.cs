using System;

namespace DataInspector
{
	internal class MulticastDelegateVisualizer : VisualizerBase
	{
		public override bool HasChildren()
		{
			return true;
		}

		public override bool InspectChildren(Inspector inspector, string path, ref object data, Type type)
		{
			var action = data as MulticastDelegate;
			if (action == null)
				return false;


			string targetName = action.Target != null ? string.Format("Target[{0}]", action.Target.GetType().Name) : "Target";
			inspector.Inspect("Method", path + ".Method", action.Method);
			inspector.Inspect(targetName, path + ".Target", action.Target);

			var list = action.GetInvocationList();
			for (int index = 0; index < list.Length; ++index)
			{
				var subAction = list[index];
				if (subAction == action)
					continue;

				inspector.Inspect(index.ToString(), path + "." + index, subAction);
			}
			return false;
		}


		public override bool InspectSelf(Inspector inspector, string name, ref object data, Type type)
		{
			var action = data as MulticastDelegate;
			if (action != null)
			{
				GUITools.LabelField("");
			}
			return false;
		}
	}
}
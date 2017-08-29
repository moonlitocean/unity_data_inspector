﻿using System;
using UnityEditor;

internal class MulticastDelegateVisualizor : DataVisualizor
{
	public override bool IsFoldable()
	{
		return true;
	}

	public override bool InspectContent(DataVisualization visualization, string path, ref object data, Type type)
	{
		var action = data as MulticastDelegate;
		if (action == null)
			return false;


		string targetName = action.Target != null ? string.Format("Target[{0}]", action.Target.GetType().Name) : "Target";
		visualization.Inspect("Method", path+".Method", action.Method);
		visualization.Inspect(targetName, path + ".Target", action.Target);

		var list = action.GetInvocationList();
		for( int index = 0; index<list.Length; ++index)
		{
			var subAction = list[index];
			if (subAction == action )
				continue;

			visualization.Inspect(index.ToString(), path + "." + index, subAction);
		}
		return false;
	}


	public override bool InspectRoot(string name, ref object data, Type type)
	{
		var action = data as MulticastDelegate;
		if(action!=null)
		{
			EditorGUILayout.LabelField("");
		}
		return false;
	}
}
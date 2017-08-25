using System;
using FF;
using UnityEditor;

internal class EnumVisualizor : DataVisualizor
{
	public override bool IsFoldable()
	{
		return false;
	}

    public override bool InspectRoot(string name, ref object data, Type type)
    {
        Enum e = data as Enum;
        if (e == null)
            return false;
        if (TypeUtil.GetAttribute<FlagsAttribute>(type) != null)
        {
            return ApplyData(ref data, EditorGUILayout.EnumMaskField(name, e));
        }
        else
        {
	        return ApplyData(ref data, EditorGUILayout.EnumPopup(name, e));
        }
    }
}
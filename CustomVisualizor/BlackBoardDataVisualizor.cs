using System;
using FF;
using FF.Editor;
using Metadata;
using Unique.BlackBoard;
using UnityEditor;
using UnityEngine;

internal class BlackBoardDataVisualizor : DataVisualizor
{
	public override bool IsFoldable()
	{
		return true;
	}

    public override bool InspectRoot(string name, ref object data, Type type)
    {
        var blackBoardData = data as BlackBoardData;
        if (blackBoardData == null)
            return false;

        using (GUITools.LayoutHorizontal())
        {
            EditorGUILayout.LabelField(blackBoardData.ToString());
        }
        return false;
    }

    public override bool InspectContent(DataVisualization visualization, string path, ref object data, Type type)
	{
        var blackBoardData = data as BlackBoardData;
	    bool changed = false;
        if (blackBoardData != null)
        {
            using (GUITools.LayoutVertical())
            {
                var blackBoardDataName = EditorGUILayout.TextField("name", blackBoardData.name);
                if (blackBoardDataName != blackBoardData.name)
                {
                    changed = true;
                    blackBoardData.name = blackBoardDataName;
                }
                var valueType = (BlackBoardValueType) EditorGUILayout.EnumPopup("type", blackBoardData.valueType);
                if (valueType != blackBoardData.valueType)
                {
                    changed = true;
                    blackBoardData.valueType = valueType;
                    switch (valueType)
                    {
                        case BlackBoardValueType.None:
                            blackBoardData.value = new BlackBoardValue();
                            break;
                        case BlackBoardValueType.Float:
                            blackBoardData.value = new BlackBoardValue_Float();
                            break;
                        case BlackBoardValueType.Int:
                            blackBoardData.value = new BlackBoardValue_Int();
                            break;
                        case BlackBoardValueType.Object:
                            blackBoardData.value = new BlackBoardValue_Object();
                            break;
                        case BlackBoardValueType.Vector3:
                            blackBoardData.value = new BlackBoardValue_Vector3();
                            break;
                    }
                }
                if (blackBoardData.valueType == BlackBoardValueType.Float || blackBoardData.valueType == BlackBoardValueType.Vector3)
                    changed |= visualization.Inspect("value", "value", blackBoardData.value,
                        blackBoardData.value.GetType());
            }
        }
        return changed;
	}
}
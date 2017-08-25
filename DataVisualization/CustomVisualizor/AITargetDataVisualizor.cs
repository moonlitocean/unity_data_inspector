using System;
using FF;
using FF.Editor;
using Metadata;
using Unique.BlackBoard;
using UnityEditor;
using UnityEngine;

internal class AITargetDataVisualizor : DataVisualizor
{
	public override bool IsFoldable()
	{
		return false;
	}

    public override bool InspectRoot(string name, ref object data, Type type)
    {
        var objectData = data as AITargetData;
        if (objectData == null)
            return false;
        bool changed = false;
        DrawMetadataUtility.ShowAITargetData(objectData, () =>
        {
            changed = true;
        });

        return changed;
    }
}
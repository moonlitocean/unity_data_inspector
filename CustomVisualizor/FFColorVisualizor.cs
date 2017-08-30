using System;
using FF;
using UnityEditor;

internal class FFColorVisualizor : DataVisualizor
{
    public override bool IsFoldable()
    {
        return false;
    }

    public override bool InspectRoot(string name, ref object data, Type type)
    {
        var ffColor = (FFColor) data;
        var color = EditorGUILayout.ColorField(name, ffColor);
        if (ffColor != (FFColor)color)
        {
            ffColor = color;
            data = ffColor;
            return true;
        }
        return false;
    }
}
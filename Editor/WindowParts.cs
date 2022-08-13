using Quartzified.Editor.WorldEditor;
using UnityEditor;
using UnityEngine;

public static class WindowParts
{
    public static void WindowTitle(string title, int space = 24)
    {
        EditorGUILayout.LabelField(title, StyleUtils.TitleStyle(), GUILayout.Height(32));
        GUILayout.Space(space);
    }

    public static void WindowSectionTitle(string title)
    {
        EditorGUILayout.LabelField(title, StyleUtils.SectionStyle(), GUILayout.Height(32));
        GUILayout.Space(4);
    }

    #region Object Editor

    public static void EditorPlacementArea(ScriptableObjectTool tool)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label("Placement Area");
        tool.placementArea = EditorGUILayout.Slider(tool.placementArea, 1, 16);
        GUILayout.EndHorizontal();
    }

    public static void EditorScatter(ScriptableObjectTool tool)
    {
        tool.scatter = EditorToggle(tool.scatter, "Scatter Objects");
        if (tool.scatter)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Scatter Distance");
            tool.scatterDistance = EditorGUILayout.FloatField(tool.scatterDistance);
            GUILayout.EndHorizontal();
        }
    }

    public static void EditorLayer(ScriptableObjectTool tool)
    {
        GUILayout.BeginHorizontal();
        tool.useLayer = EditorToggle(tool.useLayer, "Use Layers");
        if (tool.useLayer)
        {
            tool.editLayerMask = EditorGUILayout.LayerField("", tool.editLayerMask);
        }
        GUILayout.EndHorizontal();
    }

    public static void EditorOffset(ScriptableObjectTool tool)
    {
        tool.posOffset = EditorToggle(tool.posOffset, "Position Offset");
        if (tool.posOffset)
        {
            GUILayout.BeginHorizontal();
            tool.posOffsetValues = EditorGUILayout.Vector3Field("", tool.posOffsetValues);
            GUILayout.EndHorizontal();
        }
    }

    public static void EditorRotation(ScriptableObjectTool tool)
    {
        tool.randomRotation = EditorToggle(tool.randomRotation, "Object Random Rotation");
        if (tool.randomRotation)
        {
            GUILayout.BeginHorizontal();
            tool.randomRotValues = EditorGUILayout.Vector3Field("", tool.randomRotValues);
            GUILayout.EndHorizontal();
        }
    }

    public static void EditorScale(ScriptableObjectTool tool)
    {
        if (!tool.randomScale && !tool.randomScale3D)
        {
            GUILayout.BeginHorizontal();
            tool.randomScale = EditorToggle(tool.randomScale, "Object Random Scale (Solid)");
            tool.randomScale3D = EditorToggle(tool.randomScale3D, "Object Random Scale (Specific)");
            GUILayout.EndHorizontal();
        }
        else if (tool.randomScale)
        {
            tool.randomScale = EditorToggle(tool.randomScale, "Object Random Scale (Solid)");
            GUILayout.BeginHorizontal();

            GUILayout.Label("Random Scale");
            tool.scaleRange.x = EditorGUILayout.FloatField(tool.scaleRange.x);
            tool.scaleRange.y = EditorGUILayout.FloatField(tool.scaleRange.y);
            tool.randomScale3D = false;

            GUILayout.EndHorizontal();
        }
        else if (tool.randomScale3D)
        {
            tool.randomScale3D = EditorToggle(tool.randomScale3D, "Object Random Scale (Specific)");

            GUILayout.BeginHorizontal();

            GUILayout.Label("Random Scale X");
            tool.scaleXRange.x = EditorGUILayout.FloatField(tool.scaleXRange.x);
            tool.scaleXRange.y = EditorGUILayout.FloatField(tool.scaleXRange.y);

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();

            GUILayout.Label("Random Scale Y");
            tool.scaleYRange.x = EditorGUILayout.FloatField(tool.scaleYRange.x);
            tool.scaleYRange.y = EditorGUILayout.FloatField(tool.scaleYRange.y);

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();

            GUILayout.Label("Random Scale Z");
            tool.scaleZRange.x = EditorGUILayout.FloatField(tool.scaleZRange.x);
            tool.scaleZRange.x = EditorGUILayout.FloatField(tool.scaleZRange.x);

            GUILayout.EndHorizontal();

            tool.randomScale = false;
        }
    }

    public static void EditorExtent(ScriptableObjectTool tool)
    {
        GUILayout.BeginHorizontal();
        if (tool.halfExtent)
        {
            tool.halfExtent = EditorToggle(tool.halfExtent, "Half Extent");
            tool.quarterExtent = false;
        }
        else if (tool.quarterExtent)
        {
            tool.quarterExtent = EditorToggle(tool.quarterExtent, "Quarter Extent");
            tool.halfExtent = false;
        }
        else
        {
            tool.halfExtent = EditorToggle(tool.halfExtent, "Half Extent");
            tool.quarterExtent = EditorToggle(tool.quarterExtent, "Quarter Extent");
        }
        GUILayout.EndHorizontal();
    }

    public static bool EditorToggle(bool toggle, string toggleName) => GUILayout.Toggle(toggle, toggleName);

    #endregion
}

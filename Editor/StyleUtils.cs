using UnityEngine;

namespace Quartzified.Editor.WorldEditor
{
    public static class StyleUtils
    {
        public static GUIStyle TitleStyle()
        {
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.alignment = TextAnchor.UpperCenter;
            style.fontSize = 18;
            style.fontStyle = FontStyle.Bold;
            style.padding = new RectOffset(0, 0, 8, 0);

            return style;
        }

        public static GUIStyle SectionStyle()
        {
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.alignment = TextAnchor.MiddleLeft;
            style.fontSize = 14;
            style.fontStyle = FontStyle.Bold;
            style.padding = new RectOffset(0, 0, 4, 4);

            return style;
        }
    }
}

using UnityEditor;
using UnityEngine;
using System.IO;

namespace Quartzified.Editor.WorldEditor
{
    public static class Utils
    {
        static string ToolObjectPath(string name) => "Assets/Editor/WorldEditor/" + name;
        static void CheckToolObjectPath()
        {
            if (!Directory.Exists("Assets/Editor/WorldEditor"))
                Directory.CreateDirectory("Assets/Editor/WorldEditor");
        }

        public static ScriptableObjectTool GetToolObject(string toolName)
        {
            return (ScriptableObjectTool)AssetDatabase.LoadAssetAtPath(ToolObjectPath(toolName), typeof(ScriptableObjectTool));
        }

        public static ScriptableObjectTool CreateToolObject(string toolName)
        {
            CheckToolObjectPath();

            ScriptableObjectTool tool = ScriptableObject.CreateInstance<ScriptableObjectTool>();
            string path = ToolObjectPath(toolName) + ".asset";
            AssetDatabase.CreateAsset(tool, path);
            return tool;
        }

        public static Bounds GetBoundsForGameObject(GameObject go)
        {
            Bounds bounds = new Bounds();
            if (go != null)
                bounds.center = go.transform.position;
            bounds.extents = Vector3.zero;

            Renderer[] allRenderers = go.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in allRenderers)
            {
                if (!renderer)
                    continue;

                bounds.Encapsulate(renderer.bounds);
            }

            return bounds;
        }

        public static Bounds GetHalfBoundsForGameObject(GameObject go)
        {
            Bounds bounds = new Bounds();
            bounds.center = go.transform.position;
            bounds.extents = Vector3.zero;

            Renderer[] allRenderers = go.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in allRenderers)
            {
                if (!renderer)
                    continue;

                bounds.Encapsulate(renderer.bounds);
            }

            bounds.extents = bounds.extents / 2;

            return bounds;
        }
    }
}
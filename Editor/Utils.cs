using UnityEngine;

namespace Quartzified.Editor.WorldEditor
{
    public static class Utils
    {
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
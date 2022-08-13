using System.Collections.Generic;
using UnityEngine;

namespace Quartzified.Editor.WorldEditor
{
    [System.Serializable]
    public class ScriptableObjectTool : ScriptableObject
    {
        [Header("Brush Settings")]
        public float placementArea = 1;

        [Header("Object Settings")]
        public bool randomObj = false;

        [Header("Layer Settings")]
        public bool useLayer = false;
        public int editLayerMask;

        [Header("Material Settings")]
        public bool useMaterials = false;

        [Header("Scatter Settings")]
        public bool scatter = false;
        public float scatterDistance = 1;

        [Header("Position Options")]
        public bool posOffset;
        public Vector3 posOffsetValues = Vector3.zero;

        [Header("Rotation Options")]
        public bool randomRotation;
        public Vector3 randomRotValues = Vector3.zero;

        [Header("Scale Options")]
        public bool randomScale;
        public bool randomScale3D;
        [Space]
        public Vector2 scaleRange = Vector2.one;
        [Space]
        public Vector2 scaleXRange = Vector2.one;
        public Vector2 scaleYRange = Vector2.one;
        public Vector2 scaleZRange = Vector2.one;

        [Space(12)]
        [Header("Foliage Density")]
        public bool halfExtent;
        public bool quarterExtent;

        [HideInInspector]
        public List<GameObject> objectsPlaced = new List<GameObject>();
        [HideInInspector]
        public GameObject justPlacedObject;
    }
}
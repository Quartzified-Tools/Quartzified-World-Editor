using UnityEngine;

namespace Quartzified.Editor.WorldEditor
{
    [System.Serializable]
    public struct ObjectToolVariables
    {
        [SerializeField]
        public GameObject[] worldObjects;

        public bool useLayer;
        public int editLayerMask;
        public bool useMaterials;
        [SerializeField]
        public Material[] editMaterials;

        public float placementArea;

        public bool randomObj;

        public bool scatter;
        public float scatterDistance;

        public bool randomRotation;
        public Vector3 randomRotValues;

        public bool randomScale;
        public bool randomScale3D;

        public float scaleMin;
        public float scaleMax;

        public float scaleMinX;
        public float scaleMaxX;
        public float scaleMinY;
        public float scaleMaxY;
        public float scaleMinZ;
        public float scaleMaxZ;
    }
}
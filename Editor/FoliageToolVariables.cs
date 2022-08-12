using System.Collections.Generic;
using UnityEngine;

namespace Quartzified.Editor.WorldEditor
{
    [System.Serializable]
    public struct FoliageToolVariables
    {
        public float placementArea;

        public GameObject[] foliageObjects;
        public bool randomObj;

        public bool useLayer;
        public int editLayerMask;

        public bool useMaterials;
        [SerializeField]
        public Material[] editMaterials;

        public bool posOffset;
        public Vector3 posOffsetValues;

        public bool randomRotation;
        public Vector3 randomRotValues;

        public bool randomScale;

        public float scaleMin;
        public float scaleMax;

        public bool halfExtent;
        public bool quarterExtent;

        public List<GameObject> foliagePlaced;
        public GameObject justAddedFoliage;
    }
}

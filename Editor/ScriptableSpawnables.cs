using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(menuName ="Quartzified/WorldEditor/Spawnables")]
public class ScriptableSpawnables : ScriptableObject
{
    public List<GameObject> spawnables = new List<GameObject>();

    public GameObject GetSpawnable(bool rnd)
    {
        GameObject spawnable = null;

        if (spawnables.Count > 0)
            spawnable = rnd ? spawnables[Random.Range(0, spawnables.Count)] : spawnables[0];

        if(spawnable != null)
            return PrefabUtility.InstantiatePrefab(spawnable) as GameObject;

        return null;
    }
}

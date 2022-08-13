using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Quartzified.Editor.WorldEditor
{

    public static class EditObject
    {
        public static GameObject GetSpawnObject(ScriptableObjectTool tool, List<GameObject> spawnObjects)
        {
            // Check List Length is greater than 0
            if (spawnObjects.Count <= 0)
            {
                Debug.LogWarning("No Objects to spawn have been set!\nSpawning was Canceled.");
                return null;
            }

            // Default first object in list
            GameObject go = spawnObjects[0];

            // Get Random Object if requested
            if (tool.randomObj)
            {
                int rnd = Random.Range(0, spawnObjects.Count);
                go = spawnObjects[rnd];
            }

            // Ensure a valid object has been selected
            if (go == null)
            {
                Debug.LogWarning("Reference to an object is missing.\nSpawning was Canceled");
                return null;
            }

            return PrefabUtility.InstantiatePrefab(go) as GameObject;
        }

        public static void SetObjectRotation(ScriptableObjectTool tool, GameObject spawnedObj)
        {
            // Apply Random Rotatiopn
            if (tool.randomRotation)
            {
                // Get random rotation value
                Vector3 rndRotation = new Vector3(Random.Range(0, tool.randomRotValues.x), Random.Range(0, tool.randomRotValues.y), Random.Range(0, tool.randomRotValues.z));
                // Apply random rotaton value
                spawnedObj.transform.rotation = Quaternion.Euler(rndRotation) * spawnedObj.transform.rotation;
            }
        }

        public static void SetObjectScale(ScriptableObjectTool tool, GameObject spawnedObj)
        {
            // Set Random overall scale
            if (tool.randomScale)
            {
                float scale = Random.Range(tool.scaleRange.x, tool.scaleRange.y);
                spawnedObj.transform.localScale = new Vector3(scale, scale, scale);
            }

            // Set Random specific scale
            if (tool.randomScale3D)
            {
                float scaleX = Random.Range(tool.scaleXRange.x, tool.scaleXRange.y);
                float scaleY = Random.Range(tool.scaleYRange.x, tool.scaleYRange.y);
                float scaleZ = Random.Range(tool.scaleZRange.x, tool.scaleZRange.y);
                spawnedObj.transform.localScale = new Vector3(scaleX, scaleY, scaleZ);
            }
        }

        public static void SetObjectPosition(ScriptableObjectTool tool, Vector3 pos, GameObject spawnedObj)
        {
            if (tool.placementArea == 1)
            {
                if (tool.posOffset)
                {
                    pos += tool.posOffsetValues;
                }

                spawnedObj.transform.position = pos;
            }
            else
            {
                Vector3 placementArea = (Random.insideUnitSphere * (tool.placementArea - 1));
                placementArea.y = 0;

                spawnedObj.transform.position = pos + placementArea;
            }
        }
    }
}
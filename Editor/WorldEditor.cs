using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Quartzified.Editor.WorldEditor
{
    public class WorldEditor : EditorWindow
    {
        ScriptableObject worldEditor;
        SerializedObject serializedWorldEditor;
        Vector2 scrollPos;
        int modeSelect;
        int toolSelect;

        bool showObjects;
        GameObject spawnObject;
        List<GameObject> spawnObjects = new List<GameObject>();
        int objCount = 1;

        bool showMaterials;
        Material editMaterial;
        List<Material> editMaterials = new List<Material>();
        int materialCount = 1;

        public ScriptableObjectTool objectTool;

        public ScriptableObjectTool foliageTool;

        public FoliageToolVariables foliageVar;

        float yOffset;
        int savedLayer;
        bool alignNormals;

        RaycastHit hit;

        Vector3 upVector = new Vector3(0, 90, 0);

        Vector2 mousePos;
        Vector2 oldMousePos;

        Vector3 hitPosGizmo;
        Vector3 hitNormal;

        GameObject justAddedGameObject;
        bool objectJustAdded;

        Vector3 scatterPos;
        Vector3 scatterPosOld;

        //Top Menu Code
        [MenuItem("Quartzified/World Editor")]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow<WorldEditor>("World Editor").Show();
        }

        private void OnFocus()
        {
            if (worldEditor == null)
                worldEditor = this;

            if(serializedWorldEditor == null)
                serializedWorldEditor = new SerializedObject(worldEditor);
        }

        //Window Code
        private void OnGUI()
        {
            GUILayout.BeginVertical();

            scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.Width(this.position.width), GUILayout.Height(this.position.height - 1));

            switch (modeSelect)
            {
                case 0:
                    MainToolWindow();
                    break;
                case 1:
                    ObjectToolWindow();
                    if(Selection.activeGameObject)
                        Selection.activeObject = null;
                    break;
                case 2:
                    FoliageToolWindow();
                    if (Selection.activeGameObject)
                        Selection.activeObject = null;
                    break;
                case 3:
                    EditToolWindow();
                    break;
            }

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Redo"))
            {
                Undo.PerformRedo();
            }

            if (GUILayout.Button("Undo"))
            {
                Undo.PerformUndo();
            }

            GUILayout.EndHorizontal();

            GUILayout.EndScrollView();

            GUILayout.EndVertical();

            serializedWorldEditor.ApplyModifiedProperties();
        }

        void MainToolWindow()
        {
            EditorGUILayout.LabelField("Tool Select", StyleUtils.TitleStyle(), GUILayout.Height(28));
            GUILayout.Space(24);

            if (GUILayout.Button("Object Mode"))
            {
                modeSelect = 1;
                toolSelect = -1;
            }
            if (GUILayout.Button("Foliage Mode"))
            {
                modeSelect = 2;
                toolSelect = -1;
            }
            if (GUILayout.Button("Edit Mode"))
            {
                modeSelect = 3;
                toolSelect = -1;
            }
        }

        void ObjectToolWindow()
        {
            EditorGUILayout.LabelField("Object Spawning", StyleUtils.TitleStyle(), GUILayout.Height(32));
            GUILayout.Space(24);

            EditorGUILayout.LabelField("Brush Settings", StyleUtils.SectionStyle(), GUILayout.Height(32));

            GUILayout.BeginHorizontal();
            GUILayout.Label("Placement Area");
            objectTool.placementArea = EditorGUILayout.Slider(objectTool.placementArea, 1, 16);
            GUILayout.EndHorizontal();

            objectTool.scatter = GUILayout.Toggle(objectTool.scatter, "Scatter Objects");
            if (objectTool.scatter)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Scatter Distance");
                objectTool.scatterDistance = EditorGUILayout.FloatField(objectTool.scatterDistance);
                GUILayout.EndHorizontal();
            }

            GUILayout.Space(8);

            EditorGUILayout.LabelField("Object Selection", StyleUtils.SectionStyle(), GUILayout.Height(22));

            DrawCustomGameObjectList(typeof(GameObject), "Spawn Objects");

            GUILayout.Space(8);

            objectTool.randomObj = GUILayout.Toggle(objectTool.randomObj, "Choose Random Object");

            GUILayout.Space(8);

            EditorGUILayout.LabelField("Inclusions / Exclusions", StyleUtils.SectionStyle(), GUILayout.Height(22));

            GUILayout.BeginHorizontal();
            objectTool.useLayer = GUILayout.Toggle(objectTool.useLayer, "Use Layers");
            if (objectTool.useLayer)
            {
                objectTool.editLayerMask = EditorGUILayout.LayerField("", objectTool.editLayerMask);
            }
            GUILayout.EndHorizontal();

            objectTool.useMaterials = GUILayout.Toggle(objectTool.useMaterials, "Use Materials");
            if (objectTool.useMaterials)
            {
                DrawCustomMaterialList(typeof(Material), "Material Restictions");
            }

            GUILayout.Space(8);

            EditorGUILayout.LabelField("Object Manipulation", StyleUtils.SectionStyle(), GUILayout.Height(22));

            objectTool.randomRotation = GUILayout.Toggle(objectTool.randomRotation, "Object Random Rotation");
            if (objectTool.randomRotation)
            {
                GUILayout.BeginHorizontal();
                objectTool.randomRotValues = EditorGUILayout.Vector3Field("", objectTool.randomRotValues);
                GUILayout.EndHorizontal();
            }

            if (!objectTool.randomScale && !objectTool.randomScale3D)
            {
                GUILayout.BeginHorizontal();
                objectTool.randomScale = GUILayout.Toggle(objectTool.randomScale, "Object Random Scale (Solid)");
                objectTool.randomScale3D = GUILayout.Toggle(objectTool.randomScale3D, "Object Random Scale (Specific)");
                GUILayout.EndHorizontal();
            }
            else if (objectTool.randomScale)
            {
                objectTool.randomScale = GUILayout.Toggle(objectTool.randomScale, "Object Random Scale (Solid)");
                GUILayout.BeginHorizontal();

                GUILayout.Label("Random Scale");
                objectTool.scaleRange.x = EditorGUILayout.FloatField(objectTool.scaleRange.x);
                objectTool.scaleRange.y = EditorGUILayout.FloatField(objectTool.scaleRange.y);

                GUILayout.EndHorizontal();
            }
            else if (objectTool.randomScale3D)
            {
                objectTool.randomScale3D = GUILayout.Toggle(objectTool.randomScale3D, "Object Random Scale (Specific)");

                GUILayout.BeginHorizontal();

                GUILayout.Label("Random Scale X");
                objectTool.scaleXRange.x = EditorGUILayout.FloatField(objectTool.scaleXRange.x);
                objectTool.scaleXRange.y = EditorGUILayout.FloatField(objectTool.scaleXRange.y);

                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();

                GUILayout.Label("Random Scale Y");
                objectTool.scaleYRange.x = EditorGUILayout.FloatField(objectTool.scaleYRange.x);
                objectTool.scaleYRange.y = EditorGUILayout.FloatField(objectTool.scaleYRange.y);

                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();

                GUILayout.Label("Random Scale Z");
                objectTool.scaleZRange.x = EditorGUILayout.FloatField(objectTool.scaleZRange.x);
                objectTool.scaleZRange.x = EditorGUILayout.FloatField(objectTool.scaleZRange.x);

                GUILayout.EndHorizontal();

                
            }

            GUILayout.Space(12);

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Add"))
            {
                toolSelect = 1;
            }

            if (GUILayout.Button("Remove"))
            {
                toolSelect = 2;
            }

            GUILayout.EndHorizontal();

            GUILayout.Space(12);

            if (GUILayout.Button("Return"))
            {
                modeSelect = 0;
                toolSelect = -1;
            }
        }

        void FoliageToolWindow()
        {
            EditorGUILayout.LabelField("Foliage Spawning", StyleUtils.TitleStyle(), GUILayout.Height(28));
            GUILayout.Space(24);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Placement Area");
            foliageVar.placementArea = EditorGUILayout.Slider(foliageVar.placementArea, 1, 16);
            GUILayout.EndHorizontal();
            GUILayout.Space(8);

            EditorGUILayout.LabelField("Object Selection", StyleUtils.SectionStyle(), GUILayout.Height(22));

            SerializedProperty serializedGameObjects = serializedWorldEditor.FindProperty("foliageVar.foliageObjects");
            EditorGUILayout.PropertyField(serializedGameObjects, true);
            foliageVar.randomObj = GUILayout.Toggle(foliageVar.randomObj, "Choose Random Object");

            GUILayout.Space(8);

            EditorGUILayout.LabelField("Inclusions / Exclusions", StyleUtils.SectionStyle(), GUILayout.Height(22));

            GUILayout.BeginHorizontal();
            foliageVar.useLayer = GUILayout.Toggle(foliageVar.useLayer, "Use Layers");
            if (foliageVar.useLayer)
            {
                foliageVar.editLayerMask = EditorGUILayout.LayerField("", foliageVar.editLayerMask);
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            foliageVar.useMaterials = GUILayout.Toggle(foliageVar.useMaterials, "Use Materials");
            if (foliageVar.useMaterials)
            {
                SerializedProperty serializedMaterials = serializedWorldEditor.FindProperty("foliageVar.editMaterials");
                EditorGUILayout.PropertyField(serializedMaterials, true);
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(8);

            foliageVar.posOffset = GUILayout.Toggle(foliageVar.posOffset, "Foliage Position Offset");
            if (foliageVar.posOffset)
            {
                GUILayout.BeginHorizontal();
                foliageVar.posOffsetValues = EditorGUILayout.Vector3Field("", foliageVar.posOffsetValues);
                GUILayout.EndHorizontal();
            }

            foliageVar.randomRotation = GUILayout.Toggle(foliageVar.randomRotation, "Foliage Random Rotation");
            if (foliageVar.randomRotation)
            {
                GUILayout.BeginHorizontal();
                foliageVar.randomRotValues = EditorGUILayout.Vector3Field("", foliageVar.randomRotValues);
                GUILayout.EndHorizontal();
            }

            foliageVar.randomScale = GUILayout.Toggle(foliageVar.randomScale, "Foliage Random Scale (Solid)");
            if (foliageVar.randomScale)
            {
                GUILayout.BeginHorizontal();

                GUILayout.Label("Random Scale");
                foliageVar.scaleMin = EditorGUILayout.FloatField(foliageVar.scaleMin);
                foliageVar.scaleMax = EditorGUILayout.FloatField(foliageVar.scaleMax);

                GUILayout.EndHorizontal();
            }

            GUILayout.Space(8);

            GUILayout.BeginHorizontal();

            if (foliageVar.halfExtent && !foliageVar.quarterExtent)
            {
                foliageVar.halfExtent = GUILayout.Toggle(foliageVar.halfExtent, "Half Extent");
            }
            else if (!foliageVar.halfExtent && foliageVar.quarterExtent)
            {
                foliageVar.quarterExtent = GUILayout.Toggle(foliageVar.quarterExtent, "Quarter Extent");
            }
            else
            {
                foliageVar.halfExtent = GUILayout.Toggle(foliageVar.halfExtent, "Half Extent");
                foliageVar.quarterExtent = GUILayout.Toggle(foliageVar.quarterExtent, "Quarter Extent");
            }

            GUILayout.EndHorizontal();

            GUILayout.Space(8);

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Place Foliage"))
            {
                toolSelect = 11;
            }

            GUILayout.EndHorizontal();

            GUILayout.Space(12);
            if (GUILayout.Button("Return"))
            {
                modeSelect = 0;
                toolSelect = -1;
            }
        }

        void EditToolWindow()
        {
            EditorGUILayout.LabelField("Edit Mode", StyleUtils.TitleStyle(), GUILayout.Height(28));
            GUILayout.Space(24);

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Bottom"))
            {
                DropObjects("Bottom");
                toolSelect = -1;
            }

            if (GUILayout.Button("Origin"))
            {
                DropObjects("Origin");
                toolSelect = -1;
            }

            if (GUILayout.Button("Center"))
            {
                DropObjects("Center");
                toolSelect = -1;
            }

            GUILayout.EndHorizontal();

            alignNormals = GUILayout.Toggle(alignNormals, "Align Normals");
            if (alignNormals)
            {
                GUILayout.BeginHorizontal();
                upVector = EditorGUILayout.Vector3Field("Up Vector", upVector);
                GUILayout.EndHorizontal();
            }

            GUILayout.Space(12);
            if (GUILayout.Button("Return"))
            {
                modeSelect = 0;
                toolSelect = -1;
            }
        }

        private void OnEnable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;

            SceneView.duringSceneGui += OnSceneGUI;

            objectTool = Utils.GetToolObject("ObjectTool");
            if(objectTool == null)
            {
                objectTool = Utils.CreateToolObject("ObjectTool");
            }

            foliageTool = Utils.GetToolObject("FoliageTool");
            if (foliageTool == null)
            {
                foliageTool = Utils.CreateToolObject("FoliageTool");
            }
        }

        private void OnDisable()
        {
            //Saving
            modeSelect = 0;
            toolSelect = -1;

            //Clearing
            foliageVar.foliagePlaced.Clear();
        }

        private void OnDestroy()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        void OnSceneGUI(SceneView sceneView)
        {
            Event e = Event.current;
            mousePos = e.mousePosition;
            float ppp = EditorGUIUtility.pixelsPerPoint;
            mousePos.y = sceneView.camera.pixelHeight - mousePos.y * ppp;
            mousePos.x *= ppp;

            Ray rayGizmo = sceneView.camera.ScreenPointToRay(mousePos);
            RaycastHit hitGizmo;

            if (Physics.Raycast(rayGizmo, out hitGizmo, 200f))
            {
                hitPosGizmo = hitGizmo.point;
                hitNormal = hitGizmo.normal;
            }

            switch (modeSelect)
            {
                case 1:
                    ObjectTool(sceneView);
                    HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
                    break;
                case 2:
                    FoliageTool(sceneView);
                    HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
                    break;
            }

            ToolSelectVisual();
        }

        void ObjectTool(SceneView sceneView)
        {
            Event e = Event.current;

            if (e.type == EventType.MouseDown && e.button == 0)
            {
                switch (toolSelect)
                {
                    case 1:
                        if (spawnObjects.Count > 0)
                        {
                            float t = 2f * Mathf.PI * Random.Range(0f, objectTool.placementArea);
                            float u = Random.Range(0f, objectTool.placementArea) + Random.Range(0f, objectTool.placementArea);
                            float r = (u > 1 ? 2 - u : u);
                            Vector3 origin = Vector3.zero;

                            if (objectTool.placementArea != 1)
                            {
                                origin.x += r * Mathf.Cos(t);
                                origin.y += r * Mathf.Sin(t);
                            }
                            else
                            {
                                origin = Vector3.zero;
                            }

                            Ray ray = sceneView.camera.ScreenPointToRay(mousePos);
                            ray.origin += origin;

                            RaycastHit hit;

                            if (objectTool.useLayer)
                            {
                                Debug.Log("Use Layer");
                                if (Physics.Raycast(ray, out hit, 200f, 1 << objectTool.editLayerMask))
                                {
                                    Debug.Log("Raycast");
                                    if (CheckUseMaterialObject(hit))
                                    {
                                        Debug.Log("Materials");
                                        SpawnObject(hit);

                                        objectJustAdded = true;
                                        oldMousePos = e.mousePosition;

                                        scatterPosOld = hit.point;
                                    }
                                }
                            }
                            else if (Physics.Raycast(ray, out hit, 200f))
                            {
                                if (CheckUseMaterialObject(hit))
                                {
                                    SpawnObject(hit);

                                    objectJustAdded = true;
                                    oldMousePos = e.mousePosition;

                                    scatterPosOld = hit.point;
                                }
                            }
                        }
                        break;
                    case 2:
                        Ray rayRemove = sceneView.camera.ScreenPointToRay(mousePos);
                        RaycastHit hitObj;
                        if (Physics.Raycast(rayRemove, out hitObj, 200f))
                        {
                            foreach (GameObject obj in spawnObjects)
                            {
                                if (hitObj.transform.name.Equals(obj.name))
                                {
                                    Undo.DestroyObjectImmediate(hitObj.transform.gameObject);
                                    break;
                                }
                                else
                                {
                                    Transform t = hitObj.transform;
                                    while (t.parent != null)
                                    {
                                        if (t.parent.name.Equals(obj.name))
                                        {
                                            Undo.DestroyObjectImmediate(t.parent.gameObject);
                                            break;
                                        }

                                        t = t.parent.transform;
                                    }
                                }
                            }
                        }
                        break;
                }
            }

            if (objectJustAdded)
            {
                if (e.type == EventType.MouseDrag && e.button == 0 && toolSelect == 1)
                {
                    if (objectTool.scatter)
                    {
                        Ray ray = sceneView.camera.ScreenPointToRay(mousePos);

                        RaycastHit hit;

                        if (objectTool.useLayer)
                        {
                            if (Physics.Raycast(ray, out hit, 200f, 1 << objectTool.editLayerMask))
                            {
                                scatterPos = hit.point;

                                if (hit.transform.gameObject != justAddedGameObject && Vector3.Distance(scatterPos, scatterPosOld) >= objectTool.scatterDistance)
                                {
                                    if (CheckUseMaterialObject(hit))
                                    {
                                        SpawnObject(hit);
                                        scatterPosOld = hit.point;
                                    }
                                }
                            }
                        }
                        else if (Physics.Raycast(ray, out hit, 200f))
                        {
                            scatterPos = hit.point;

                            if (hit.transform.gameObject != justAddedGameObject && Vector3.Distance(scatterPos, scatterPosOld) >= objectTool.scatterDistance)
                            {
                                if (CheckUseMaterialObject(hit))
                                {
                                    SpawnObject(hit);
                                    scatterPosOld = hit.point;
                                }
                            }
                        }

                    }
                    else
                    {
                        Vector2 difference = e.mousePosition - oldMousePos;

                        justAddedGameObject.transform.Rotate(justAddedGameObject.transform.up * difference.x);

                        oldMousePos = e.mousePosition;
                    }

                }

                if (e.type == EventType.MouseUp && e.button == 0)
                    objectJustAdded = false;
            }
        }

        void SpawnObject(RaycastHit hit)
        {

            if (spawnObjects.Count <= 0)
            {
                Debug.LogWarning("No Objects to spawn have been set!\nSpawning was Canceled.");
                return;
            }

            GameObject go = spawnObjects[0];

            //Random Object
            if (objectTool.randomObj)
            {
                int rnd = Random.Range(0, spawnObjects.Count + 1);
                go = spawnObjects[rnd];
            }

            if (go == null)
            {
                Debug.LogWarning("Reference to an object is missing.\nSpawning was Canceled");
                return;
            }

            GameObject spawnedObj = PrefabUtility.InstantiatePrefab(go) as GameObject;

            Vector3 goRot = spawnedObj.transform.eulerAngles;

            //Random Rotation
            if (objectTool.randomRotation)
            {
                goRot = new Vector3(Random.Range(0, objectTool.randomRotValues.x), Random.Range(0, objectTool.randomRotValues.y), Random.Range(0, objectTool.randomRotValues.z));
            }

            //Randon Scale
            if (objectTool.randomScale && !objectTool.randomScale3D)
            {
                float scale = Random.Range(objectTool.scaleRange.x, objectTool.scaleRange.y);
                spawnedObj.transform.localScale = new Vector3(scale, scale, scale);
            }
            else if (!objectTool.randomScale && objectTool.randomScale3D)
            {
                float scaleX = Random.Range(objectTool.scaleXRange.x, objectTool.scaleXRange.y);
                float scaleY = Random.Range(objectTool.scaleYRange.x, objectTool.scaleYRange.y);
                float scaleZ = Random.Range(objectTool.scaleZRange.x, objectTool.scaleZRange.y);
                spawnedObj.transform.localScale = new Vector3(scaleX, scaleY, scaleZ);
            }

            //Set Position
            spawnedObj.transform.position = hit.point;

            //Set Rotation
            if(objectTool.randomRotation)
                spawnedObj.transform.rotation = Quaternion.Euler(goRot) * spawnedObj.transform.rotation;

            justAddedGameObject = spawnedObj;

            Undo.RegisterCreatedObjectUndo(spawnedObj, "WorldEditor: Spawn Object");
        }

        void FoliageTool(SceneView sceneView)
        {
            Event e = Event.current;

            if (e.type == EventType.MouseDrag && e.button == 0)
            {
                switch (toolSelect)
                {
                    case 11:
                        if (foliageVar.foliageObjects.Length > 0)
                        {
                            float t = 2f * Mathf.PI * Random.Range(0f, foliageVar.placementArea);
                            float u = Random.Range(0f, foliageVar.placementArea) + Random.Range(0f, foliageVar.placementArea);
                            float r = (u > 1 ? 2 - u : u);
                            Vector3 origin = Vector3.zero;

                            if (foliageVar.placementArea != 1)
                            {
                                origin.x += r * Mathf.Cos(t);
                                origin.y += r * Mathf.Sin(t);
                            }
                            else
                            {
                                origin = Vector3.zero;
                            }

                            Ray ray = sceneView.camera.ScreenPointToRay(mousePos);
                            ray.origin += origin;

                            RaycastHit hit;

                            if (foliageVar.useLayer)
                            {
                                Debug.Log("Use Layer");
                                if (Physics.Raycast(ray, out hit, 200f, 1 << foliageVar.editLayerMask))
                                {
                                    Debug.Log("Raycast");
                                    if (CheckUseMaterialFoliage(hit))
                                    {
                                        Debug.Log("Materials");
                                        SpawnFoliage(hit);

                                        oldMousePos = e.mousePosition;
                                    }
                                }
                            }
                            else if (Physics.Raycast(ray, out hit, 200f))
                            {
                                if (CheckUseMaterialFoliage(hit))
                                {
                                    SpawnFoliage(hit);

                                    oldMousePos = e.mousePosition;
                                }
                            }

                        }
                        break;
                }
            }
        }

        void SpawnFoliage(RaycastHit hit)
        {
            GameObject go = foliageVar.foliageObjects[0];

            //Random Object
            if (foliageVar.randomObj)
            {
                int rnd = Random.Range(0, foliageVar.foliageObjects.Length);
                go = foliageVar.foliageObjects[rnd];
            }

            GameObject spawnedObject = PrefabUtility.InstantiatePrefab(go) as GameObject;

            Vector3 goRot = spawnedObject.transform.eulerAngles;

            //Random Rotation
            if (foliageVar.randomRotation)
            {
                goRot = new Vector3(Random.Range(0, foliageVar.randomRotValues.x), Random.Range(0, foliageVar.randomRotValues.y), Random.Range(0, foliageVar.randomRotValues.z));
            }

            //Randon Scale
            if (foliageVar.randomScale)
            {
                float scale = Random.Range(foliageVar.scaleMin, foliageVar.scaleMax);
                spawnedObject.transform.localScale = new Vector3(scale, scale, scale);
            }

            Vector3 spawnPos = hit.point;
            if (foliageVar.posOffset)
            {
                spawnPos += foliageVar.posOffsetValues;
            }

            //Set Position
            spawnedObject.transform.position = spawnPos;

            //Set Rotation
            spawnedObject.transform.rotation = Quaternion.Euler(goRot);

            foliageVar.justAddedFoliage = spawnedObject;

            if (foliageVar.foliagePlaced.Count > 0)
            {
                foreach (GameObject foliage in foliageVar.foliagePlaced)
                {
                    if (foliage != null && foliageVar.justAddedFoliage != null)
                    {
                        if (foliageVar.halfExtent)
                        {
                            if (Utils.GetHalfBoundsForGameObject(foliage).Intersects(Utils.GetBoundsForGameObject(foliageVar.justAddedFoliage)))
                            {
                                DestroyImmediate(foliageVar.justAddedFoliage);
                                return;
                            }
                        }
                        else if (foliageVar.quarterExtent)
                        {
                            if (Utils.GetHalfBoundsForGameObject(foliage).Intersects(Utils.GetHalfBoundsForGameObject(foliageVar.justAddedFoliage)))
                            {
                                DestroyImmediate(foliageVar.justAddedFoliage);
                                return;
                            }
                        }
                        else
                        {
                            if (Utils.GetBoundsForGameObject(foliage).Intersects(Utils.GetBoundsForGameObject(foliageVar.justAddedFoliage)))
                            {
                                DestroyImmediate(foliageVar.justAddedFoliage);
                                return;
                            }
                        }
                    }
                }
            }

            if (foliageVar.justAddedFoliage != null)
            {
                foliageVar.foliagePlaced.Add(foliageVar.justAddedFoliage);
                Undo.RegisterCreatedObjectUndo(foliageVar.justAddedFoliage, "WorldEditor: Spawn Foliage");
            }
        }

        void ToolSelectVisual()
        {
            switch (toolSelect)
            {
                case 1:
                    Handles.color = Color.cyan;
                    Handles.DrawWireDisc(hitPosGizmo, hitNormal, objectTool.placementArea);
                    Handles.color = new Color(0, 1, 1, 0.1f);
                    Handles.DrawSolidDisc(hitPosGizmo, hitNormal, objectTool.placementArea);
                    break;
                case 2:
                    Handles.color = Color.red;
                    Handles.DrawWireDisc(hitPosGizmo, hitNormal, 1f);
                    Handles.color = new Color(1, 0, 0, 0.1f);
                    Handles.DrawSolidDisc(hitPosGizmo, hitNormal, 1f);
                    break;

                case 11:
                    Handles.color = Color.cyan;
                    Handles.DrawWireDisc(hitPosGizmo, hitNormal, foliageVar.placementArea);
                    Handles.color = new Color(0, 1, 1, 0.1f);
                    Handles.DrawSolidDisc(hitPosGizmo, hitNormal, foliageVar.placementArea);
                    break;
            }
        }

        void DropObjects(string method)
        {
            Undo.RecordObjects(Selection.transforms, "Drop Objects");

            for (int i = 0; i < Selection.transforms.Length; i++)
            {
                GameObject go = Selection.transforms[i].gameObject;
                if (!go) continue;

                Bounds bounds = Utils.GetBoundsForGameObject(go);
                savedLayer = go.layer;
                go.layer = 2;

                if (Physics.Raycast(go.transform.position, -Vector3.up, out hit))
                {
                    switch (method)
                    {
                        case "Bottom":
                            yOffset = go.transform.position.y - bounds.min.y;
                            break;
                        case "Origin":
                            yOffset = 0;
                            break;
                        case "Center":
                            yOffset = bounds.center.y - go.transform.position.y;
                            break;
                    }

                    if (alignNormals)
                    {
                        Quaternion oldRot = go.transform.rotation;
                        go.transform.up = hit.normal + upVector;

                        oldRot.x = go.transform.rotation.x;
                        oldRot.z = go.transform.rotation.z;
                        go.transform.rotation = oldRot;
                    }


                    go.transform.position = new Vector3(hit.point.x, hit.point.y + yOffset, hit.point.z);
                }
                go.layer = savedLayer;
            }
        }

        bool CheckUseMaterialObject(RaycastHit hit)
        {
            if (objectTool.useMaterials)
            {
                foreach (Material mat in editMaterials)
                {
                    Renderer renderer = hit.transform.GetComponent<Renderer>();
                    if (renderer != null)
                        for (int i = 0; i < renderer.sharedMaterials.Length; i++)
                        {
                            if (renderer.sharedMaterials[i] == mat)
                                return true;
                        }
                }
                return false;
            }
            else
                return true;

        }

        bool CheckUseMaterialFoliage(RaycastHit hit)
        {
            if (foliageVar.useMaterials)
            {
                foreach (Material mat in foliageVar.editMaterials)
                {
                    Renderer renderer = hit.transform.GetComponent<Renderer>();
                    if (renderer != null)
                        for (int i = 0; i < renderer.sharedMaterials.Length; i++)
                        {
                            if (renderer.sharedMaterials[i] == mat)
                                return true;
                        }
                }
                return false;
            }
            else
                return true;
        }

        void DrawCustomGameObjectList(System.Type type, string title = "Foldout")
        {
            if (spawnObject != null)
            {
                EditorGUILayout.BeginHorizontal();

                // Create Int Field for List Size
                objCount = EditorGUILayout.IntField(objCount, GUILayout.Width(32));
                Mathf.Clamp(objCount, 1, int.MaxValue);

                EditorGUILayout.BeginVertical();

                // Create Foldout to organize List
                showObjects = EditorGUILayout.Foldout(showObjects, title);

                // If Foldout create necessary Object Field
                if (showObjects)
                {
                    for (int i = 0; i < objCount; i++)
                    {
                        if (spawnObjects.Count <= i)
                        {
                            spawnObjects.Add(null);
                        }

                        if (i == 0)
                        {
                            spawnObject = (GameObject)EditorGUILayout.ObjectField(spawnObject, type, true);
                            spawnObjects[i] = spawnObject;
                        }
                        else
                        {
                            spawnObjects[i] = (GameObject)EditorGUILayout.ObjectField(spawnObjects[i], type, true);
                        }

                    }
                }

                EditorGUILayout.EndVertical();

                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                GUIStyle style = new GUIStyle();
                style.padding = new RectOffset(8, 0, 0, 0);
                style.richText = true;

                EditorGUILayout.LabelField("<color=white>Spawn Object</color>", style, GUILayout.Width(92));
                spawnObject = (GameObject)EditorGUILayout.ObjectField(spawnObject, type, true);
                spawnObjects.Clear();
                objCount = 1;

                EditorGUILayout.EndHorizontal();
            }
        }
        void DrawCustomMaterialList(System.Type type, string title = "Foldout")
        {
            if(editMaterial != null)
            {
                EditorGUILayout.BeginHorizontal();

                // Create Int Field for List Size
                materialCount = EditorGUILayout.IntField(materialCount, GUILayout.Width(32));
                Mathf.Clamp(materialCount, 1, int.MaxValue);

                EditorGUILayout.BeginVertical();

                // Create Foldout to organize List
                showMaterials = EditorGUILayout.Foldout(showMaterials, title);

                // If Foldout create necessary Object Field
                if (showMaterials)
                {
                    for (int i = 0; i < materialCount; i++)
                    {
                        if (editMaterials.Count <= i)
                        {
                            editMaterials.Add(null);
                        }

                        if(i == 0)
                        {
                            editMaterial = (Material)EditorGUILayout.ObjectField(editMaterial, type, true);
                            editMaterials[i] = editMaterial;
                        }
                        else
                        {
                            editMaterials[i] = (Material)EditorGUILayout.ObjectField(editMaterials[i], type, true);
                        }

                    }
                }

                EditorGUILayout.EndVertical();

                EditorGUILayout.EndHorizontal();
            }
            else
            {
                GUILayout.Space(4);
                EditorGUILayout.BeginHorizontal();
                GUIStyle style = new GUIStyle();
                style.padding = new RectOffset(8, 0, 0, 0);
                style.richText = true;

                EditorGUILayout.LabelField("<color=white>Edit Material</color>", style, GUILayout.Width(82));
                editMaterial = (Material)EditorGUILayout.ObjectField(editMaterial, type, true);
                editMaterials.Clear();
                materialCount = 1;

                EditorGUILayout.EndHorizontal();
            }

        }
    }
}
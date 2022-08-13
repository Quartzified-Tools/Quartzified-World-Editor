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

        // Custom GameObject List
        bool showObjects = true;
        GameObject spawnObject;
        List<GameObject> spawnObjects = new List<GameObject>();
        int objCount = 1;

        // Custom Material List
        bool showMaterials = true;
        Material editMaterial;
        List<Material> editMaterials = new List<Material>();
        int materialCount = 1;

        public ScriptableObjectTool objectTool;
        public ScriptableObjectTool foliageTool;

        float yOffset;
        int savedLayer;
        bool alignNormals;

        RaycastHit hit;

        Vector3 upVector = new Vector3(0, 90, 0);

        Vector2 mousePos;
        Vector2 oldMousePos;

        Vector3 hitPosGizmo;
        Vector3 hitNormal;

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

            objectTool = Utils.GetToolObject("ObjectTool");
            if (objectTool == null)
            {
                objectTool = Utils.CreateToolObject("ObjectTool");
            }

            foliageTool = Utils.GetToolObject("FoliageTool");
            if (foliageTool == null)
            {
                foliageTool = Utils.CreateToolObject("FoliageTool");
            }
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
            WindowParts.WindowTitle("Tool Select", 24);

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
            WindowParts.WindowTitle("Object Spawning", 12);

            WindowParts.WindowSectionTitle("Brush Settings");
            WindowParts.EditorPlacementArea(objectTool);
            WindowParts.EditorScatter(objectTool);

            GUILayout.Space(8);

            WindowParts.WindowSectionTitle("Object Selection");
            DrawCustomGameObjectList(typeof(GameObject), "Spawn Objects");
            GUILayout.Space(4);
            objectTool.randomObj = WindowParts.EditorToggle(objectTool.randomObj, "Choose Random Object");

            GUILayout.Space(8);

            WindowParts.WindowSectionTitle("Inclusions / Exclusions");
            WindowParts.EditorLayer(objectTool);
            GUILayout.Space(4);
            objectTool.useMaterials = WindowParts.EditorToggle(objectTool.useMaterials, "Use Materials");
            if (objectTool.useMaterials)
            {
                DrawCustomMaterialList(typeof(Material), "Material Restrictions");
            }

            GUILayout.Space(8);

            WindowParts.WindowSectionTitle("Object Manipulation");
            WindowParts.EditorRotation(objectTool);
            GUILayout.Space(4);
            WindowParts.EditorScale(objectTool);

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
            WindowParts.WindowTitle("Foliage Spawning", 12);

            WindowParts.WindowSectionTitle("Brush Settings");
            WindowParts.EditorPlacementArea(foliageTool);

            GUILayout.Space(8);

            WindowParts.WindowSectionTitle("Object Selection");
            DrawCustomGameObjectList(typeof(GameObject), "Spawn Objects");
            GUILayout.Space(4);
            foliageTool.randomObj = WindowParts.EditorToggle(foliageTool.randomObj, "Choose Random Object");

            GUILayout.Space(8);

            WindowParts.WindowSectionTitle("Inclusions / Exclusions");
            WindowParts.EditorLayer(foliageTool);
            GUILayout.Space(4);
            foliageTool.useMaterials = WindowParts.EditorToggle(foliageTool.useMaterials, "Use Materials");
            if (foliageTool.useMaterials)
            {
                DrawCustomMaterialList(typeof(Material), "Material Restrictions");
            }

            GUILayout.Space(8);

            WindowParts.WindowSectionTitle("Object Manipulation");
            WindowParts.EditorOffset(foliageTool);
            GUILayout.Space(4);
            WindowParts.EditorRotation(foliageTool);
            GUILayout.Space(4);
            WindowParts.EditorScale(foliageTool);

            GUILayout.Space(8);

            WindowParts.EditorExtent(foliageTool);

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
            WindowParts.WindowTitle("Edit Mode", 12);

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

            objectTool?.objectsPlaced.Clear();
            foliageTool?.objectsPlaced.Clear();
        }

        private void OnDestroy() => SceneView.duringSceneGui -= OnSceneGUI;

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
                            Ray ray = sceneView.camera.ScreenPointToRay(mousePos);

                            RaycastHit hit;

                            if (objectTool.useLayer)
                            {
                                Debug.Log("Use Layer");
                                if (Physics.Raycast(ray, out hit, 200f, 1 << objectTool.editLayerMask))
                                {
                                    Debug.Log("Raycast");
                                    if (CheckUseMaterial(objectTool, hit))
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
                                if (CheckUseMaterial(objectTool, hit))
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

                                if (hit.transform.gameObject != objectTool.justPlacedObject && Vector3.Distance(scatterPos, scatterPosOld) >= objectTool.scatterDistance)
                                {
                                    if (CheckUseMaterial(objectTool, hit))
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

                            if (hit.transform.gameObject != objectTool.justPlacedObject && Vector3.Distance(scatterPos, scatterPosOld) >= objectTool.scatterDistance)
                            {
                                if (CheckUseMaterial(objectTool, hit))
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

                        objectTool.justPlacedObject.transform.Rotate(objectTool.justPlacedObject.transform.up * difference.x);

                        oldMousePos = e.mousePosition;
                    }

                }

                if (e.type == EventType.MouseUp && e.button == 0)
                    objectJustAdded = false;
            }
        }

        void SpawnObject(RaycastHit hit)
        {
            GameObject spawnedObj = EditObject.GetSpawnObject(objectTool, spawnObjects);

            if(spawnedObj == null)
            {
                Debug.LogWarning("Reference to an object is missing.\nSpawning was Canceled");
                return;
            }

            // Set Position
            EditObject.SetObjectPosition(objectTool, hit.point, spawnedObj);

            // Set Rotation
            EditObject.SetObjectRotation(objectTool, spawnedObj);

            // Set Scale
            EditObject.SetObjectScale(objectTool, spawnedObj);

            objectTool.justPlacedObject = spawnedObj;

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
                        if (spawnObjects.Count > 0)
                        {
                            float t = 2f * Mathf.PI * Random.Range(0f, foliageTool.placementArea);
                            float u = Random.Range(0f, foliageTool.placementArea) + Random.Range(0f, foliageTool.placementArea);
                            float r = (u > 1 ? 2 - u : u);
                            Vector3 origin = Vector3.zero;

                            if (foliageTool.placementArea != 1)
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

                            if (foliageTool.useLayer)
                            {
                                if (Physics.Raycast(ray, out hit, 200f, 1 << foliageTool.editLayerMask))
                                {
                                    if (CheckUseMaterial(foliageTool, hit))
                                    {
                                        SpawnFoliage(hit);

                                        oldMousePos = e.mousePosition;
                                    }
                                }
                            }
                            else if (Physics.Raycast(ray, out hit, 200f))
                            {
                                if (CheckUseMaterial(foliageTool, hit))
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
            GameObject spawnedObj = EditObject.GetSpawnObject(foliageTool, spawnObjects);

            if (spawnedObj == null)
            {
                Debug.LogWarning("Reference to an object is missing.\nSpawning was Canceled");
                return;
            }

            // Set Position
            EditObject.SetObjectPosition(foliageTool, hit.point, spawnedObj);

            // Set Rotation
            EditObject.SetObjectRotation(foliageTool, spawnedObj);

            // Set Scale
            EditObject.SetObjectScale(foliageTool, spawnedObj);


            foliageTool.justPlacedObject = spawnedObj;

            if (foliageTool.objectsPlaced.Count > 0)
            {
                foreach (GameObject foliage in foliageTool.objectsPlaced)
                {
                    if (foliage != null && foliageTool.justPlacedObject != null)
                    {
                        if (foliageTool.halfExtent)
                        {
                            if (Utils.GetHalfBoundsForGameObject(foliage).Intersects(Utils.GetBoundsForGameObject(foliageTool.justPlacedObject)))
                            {
                                DestroyImmediate(foliageTool.justPlacedObject);
                                return;
                            }
                        }
                        else if (foliageTool.quarterExtent)
                        {
                            if (Utils.GetHalfBoundsForGameObject(foliage).Intersects(Utils.GetHalfBoundsForGameObject(foliageTool.justPlacedObject)))
                            {
                                DestroyImmediate(foliageTool.justPlacedObject);
                                return;
                            }
                        }
                        else
                        {
                            if (Utils.GetBoundsForGameObject(foliage).Intersects(Utils.GetBoundsForGameObject(foliageTool.justPlacedObject)))
                            {
                                DestroyImmediate(foliageTool.justPlacedObject);
                                return;
                            }
                        }
                    }
                }
            }

            if (foliageTool.justPlacedObject != null)
            {
                foliageTool.objectsPlaced.Add(foliageTool.justPlacedObject);
                Undo.RegisterCreatedObjectUndo(foliageTool.justPlacedObject, "WorldEditor: Spawn Foliage");
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
                    Handles.DrawWireDisc(hitPosGizmo, hitNormal, foliageTool.placementArea);
                    Handles.color = new Color(0, 1, 1, 0.1f);
                    Handles.DrawSolidDisc(hitPosGizmo, hitNormal, foliageTool.placementArea);
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

        bool CheckUseMaterial(ScriptableObjectTool tool, RaycastHit hit)
        {
            if (tool.useMaterials)
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

        void DrawCustomGameObjectList(System.Type type, string title = "Foldout")
        {
            if (spawnObject != null)
            {
                EditorGUILayout.BeginHorizontal();

                // Create Int Field for List Size
                objCount = EditorGUILayout.IntField(objCount, GUILayout.Width(32));
                if (objCount < 1)
                    objCount = 1;

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
                if (materialCount < 1)
                    materialCount = 1;

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
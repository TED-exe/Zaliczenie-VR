using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class PathCreateTool : EditorWindow
{
    //UI
    private Button createWaypointsButton;
    private Button createPathButton;
    private ListView pathsList;

    // Building mode
    private bool createPathMode = false;

    // Highlight
    private int layerToHighlight = 8;
    private Color highlightColor = Color.yellow;

    // Path
    CarPath currSelectedPath;

    // Debug
    private GameObject CameraPosition;

    private Dictionary<GameObject, Material> originalMaterial = new Dictionary<GameObject, Material>();

    // Editor Play Mode control
    private bool isInPlayMode = false;

    [SerializeField]
    private VisualTreeAsset m_VisualTreeAsset = default;

    [MenuItem("Window/PathCreateTool")]
    public static void ShowExample()
    {
        PathCreateTool wnd = GetWindow<PathCreateTool>();
        wnd.titleContent = new GUIContent("PathCreateTool");
    }

    public void CreateGUI()
    {
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/PathCreateTool.uxml");
        VisualElement root = visualTree.CloneTree();
        rootVisualElement.Add(root);

        createWaypointsButton = root.Q<Button>("CreateWaypointsButton");
        createWaypointsButton.clicked += EnterBuildingMode;

        createPathButton = root.Q<Button>("CreatePathButton");
        createPathButton.clicked += CreatePath;

        Selection.selectionChanged += OnSelectionChanged;

        SceneView.duringSceneGui += OnSceneGUI;

        pathsList = new ListView();
        root.Add(pathsList);

        // Rejestracja nasłuchiwania stanu trybu Play Mode
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private void OnSelectionChanged()
    {
        // Sprawdzamy, czy zaznaczony obiekt ma komponent WaypointPath
        GameObject selectedObject = Selection.activeGameObject;

        if (selectedObject != null && selectedObject.TryGetComponent<CarPath>(out var waypointPath))
        {
            ExitBuildingMode();
            Debug.Log("WaypointPath selected, exiting building mode.");
        }
    }

    private void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        // Obsługa zmian w trybie Play Mode
        if (state == PlayModeStateChange.ExitingEditMode)
        {
            isInPlayMode = true;
            ExitBuildingMode();
        }
        else if (state == PlayModeStateChange.ExitingPlayMode)
        {
            isInPlayMode = false;
            ExitBuildingMode();
        }
    }
    private void CreatePath()
    {
        if (isInPlayMode)  // Nie wchodzimy w tryb budowania w trybie Play Mode
            return;

        ExitBuildingMode();
        if (currSelectedPath != null)
        {
            DestroyImmediate(currSelectedPath.gameObject);
            currSelectedPath = null;
        }

        GameObject path = new GameObject("Path");
        currSelectedPath = path.AddComponent<CarPath>();
        currSelectedPath.CreatePath();
    }
    private void OnSceneGUI(SceneView view)
    {
        if (!createPathMode || isInPlayMode)  // Zatrzymujemy tryb budowania, jeśli jesteśmy w trybie Play Mode
            return;

        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

        Event e = Event.current;
        if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape)
        {
            ExitBuildingMode();
        }
        else if (e.type == EventType.MouseDown && e.button == (int)MouseButton.LeftMouse && createPathMode)
        {
            Selection.activeGameObject = null;

            Camera sceneCamera = view.camera;

            if (sceneCamera == null)
                return;

            Vector2 mousePosition = e.mousePosition;
            mousePosition.y = sceneCamera.pixelHeight - mousePosition.y;  // Odwrócenie współrzędnej Y
            Ray ray = sceneCamera.ScreenPointToRay(mousePosition);

            // Wykonanie raycasta
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, 1 << layerToHighlight))
            {
                if (currSelectedPath == null)
                    return;

                currSelectedPath.AddPathWaypoint(hit.point);
            }
        }
    }
    private void ExitBuildingMode()
    {
        if (currSelectedPath != null)
        {
            currSelectedPath.DrawQuadraticBezierPath();
        }

        // Włącz interaktywność przycisku
        createWaypointsButton.SetEnabled(true);
        createPathMode = false;

        RestoreOriginalHighlightedObject();
    }
    private void EnterBuildingMode()
    {
        if (isInPlayMode)  // Nie wchodzimy w tryb budowania w trybie Play Mode
            return;

        // Wyłącz interaktywność przycisku
        createWaypointsButton.SetEnabled(false);
        createPathMode = true;

        HighlightObjectOnSelectedLayer(layerToHighlight, highlightColor);
    }
    private void HighlightObjectOnSelectedLayer(int layerToHighlight, Color highlightColor)
    {
        GameObject[] allGameobjects = FindObjectsOfType<GameObject>();

        foreach (GameObject obj in allGameobjects)
        {
            if (obj.layer != layerToHighlight)
                continue;

            if (obj.TryGetComponent<Renderer>(out var renderer))
            {
                if (originalMaterial.ContainsKey(obj))
                    originalMaterial[obj] = renderer.material;
                else
                    originalMaterial.Add(obj, renderer.sharedMaterial);

                Material highlightMaterial = new Material(renderer.sharedMaterial);
                highlightMaterial.color = highlightColor;

                renderer.material = highlightMaterial;
            }
        }
    }
    private void RestoreOriginalHighlightedObject()
    {
        foreach (var element in originalMaterial)
        {
            if (element.Key == null)
                return;

            if (element.Key.TryGetComponent<Renderer>(out var render))
            {
                render.material = element.Value;
            }
        }

        originalMaterial.Clear();
    }
    private void OnDisable()
    {
        RestoreOriginalHighlightedObject();
        ExitBuildingMode();
        SceneView.duringSceneGui -= OnSceneGUI;

        // Odrejestrowanie nasłuchiwania stanu Play Mode
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
    }
}

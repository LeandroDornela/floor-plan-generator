using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;


public class FloorPlanGraphEditorWindow : EditorWindow
{
    private DataGraphView graphView;

    [MenuItem("Tools/Floor Plan Graph Editor")]
    public static void Open()
    {
        var window = GetWindow<FloorPlanGraphEditorWindow>();
        window.titleContent = new GUIContent("Floor Plan Graph Editor");
    }

    private void OnEnable()
    {
        ConstructGraphView();
        GenerateToolbar();
    }

    private void OnDisable()
    {
        rootVisualElement.Remove(graphView);
    }

    private void ConstructGraphView()
    {
        graphView = new DataGraphView
        {
            name = "Data Graph"
        };
        graphView.StretchToParentSize();
        rootVisualElement.Add(graphView);
    }

    private void GenerateToolbar()
    {
        var toolbar = new Toolbar();

        var nodeButton = new Button(() => graphView.CreateNode()) { text = "Add Node" };
        toolbar.Add(nodeButton);
        var rootNodeButton = new Button(() => graphView.CreateRootNode()) { text = "Add Root Node" };
        toolbar.Add(rootNodeButton);

        var saveButton = new Button(() => SaveGraph()) { text = "Save Graph" };
        var loadButton = new Button(() => LoadGraph()) { text = "Load Graph" };

        toolbar.Add(saveButton);
        toolbar.Add(loadButton);

        rootVisualElement.Add(toolbar);
    }

    private void SaveGraph()
    {
        string path = EditorUtility.SaveFilePanelInProject("Save Graph", "DataGraph", "asset", "Save your graph asset");
        if (string.IsNullOrEmpty(path)) return;

        var asset = ScriptableObject.CreateInstance<FloorPlanGraphData>();
        graphView.SaveGraphTo(asset);
        AssetDatabase.CreateAsset(asset, path);
        AssetDatabase.SaveAssets();
    }

    private void LoadGraph()
    {
        string path = EditorUtility.OpenFilePanel("Load Graph", "Assets", "asset");
        if (string.IsNullOrEmpty(path)) return;

        path = FileUtil.GetProjectRelativePath(path);
        var asset = AssetDatabase.LoadAssetAtPath<FloorPlanGraphData>(path);
        if (asset != null)
        {
            graphView.LoadGraphFrom(asset);
        }
    }
}

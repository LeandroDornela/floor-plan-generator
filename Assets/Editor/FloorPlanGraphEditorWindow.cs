using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;

namespace BuildingGenerator
{
    public class FloorPlanGraphEditorWindow : EditorWindow
    {
        private DataGraphView _graphView;

        private TextField _planIdField;
        private Vector2IntField _gridDimField;
        private FloorPlanGraphData _lastLoadedAsset;


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

            if (_lastLoadedAsset != null)
            {
                _graphView.LoadGraphFrom(_lastLoadedAsset);
                _planIdField.value = _graphView.PlanId;
                _gridDimField.value = _graphView.GridDimensions;
            }
        }

        private void OnDisable()
        {
            rootVisualElement.Remove(_graphView);
        }

        private void ConstructGraphView()
        {
            _graphView = new DataGraphView
            {
                name = "Data Graph"
            };

            _graphView.StretchToParentSize();
            var grid = new GridBackground();
            grid.StretchToParentSize();
            _graphView.Insert(0, grid);

            StyleSheet styleSheet = (StyleSheet)EditorGUIUtility.Load("GridBackground.uss");
            _graphView.styleSheets.Add(styleSheet);

            rootVisualElement.Add(_graphView);
        }

        private void GenerateToolbar()
        {
            var toolbarH1 = new Toolbar();
            var toolbarH2 = new Toolbar();
            var toolbarV = new Toolbar();
            toolbarH1.style.flexDirection = FlexDirection.Row;
            toolbarH2.style.flexDirection = FlexDirection.Row;
            toolbarH2.style.alignSelf = Align.Center;
            toolbarH2.style.height = 32;
            toolbarV.style.flexDirection = FlexDirection.Column;

            _planIdField = new TextField("Floor Plan ID");
            _planIdField.value = "New Floor Plan";
            _planIdField.style.flexGrow = 1;
            _planIdField.RegisterValueChangedCallback(evt => _graphView.PlanId = evt.newValue);
            toolbarH1.Add(_planIdField);

            _gridDimField = new Vector2IntField("Grid dimensions");
            _gridDimField.value = new Vector2Int(10, 10);
            _gridDimField.style.flexGrow = 1;
            _gridDimField.RegisterValueChangedCallback(evt => _graphView.GridDimensions = _gridDimField.value);
            toolbarH1.Add(_gridDimField);

            var nodeButton = new Button(() => _graphView.CreateNode()) { text = "Add Node" };
            toolbarH2.Add(nodeButton);
            var rootNodeButton = new Button(() => _graphView.CreateRootNode()) { text = "Add Root Node" };
            toolbarH2.Add(rootNodeButton);

            var saveButton = new Button(() => SaveGraph()) { text = "Save" };
            var loadButton = new Button(() => LoadGraph()) { text = "Load" };
            toolbarH2.Add(saveButton);
            toolbarH2.Add(loadButton);

            toolbarV.Add(toolbarH1);
            toolbarV.Add(toolbarH2);

            rootVisualElement.Add(toolbarV);
        }

        private void SaveGraph()
        {
            string path = EditorUtility.SaveFilePanelInProject("Save Graph", "DataGraph", "asset", "Save your graph asset");
            if (string.IsNullOrEmpty(path)) return;

            var asset = ScriptableObject.CreateInstance<FloorPlanGraphData>();
            _graphView.SaveGraphTo(asset);
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
                _graphView.LoadGraphFrom(asset);
            }

            _lastLoadedAsset = asset;
            _planIdField.value = _graphView.PlanId;
            _gridDimField.value = _graphView.GridDimensions;
        }
    }
}

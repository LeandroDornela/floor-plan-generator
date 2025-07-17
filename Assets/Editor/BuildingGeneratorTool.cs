using UnityEditor;
using UnityEngine;

namespace BuildingGenerator
{
    // TODO: Block buttons during the generation.
    public class BuildingGeneratorTool : EditorWindow
    {
        public BuildingGeneratorSettings buildingGeneratorSettings; // Config
        public MethodGrowthSettings methodGrowthSettings; // Config
        public FloorPlanGenSceneDebugger floorPlanGenSceneDebugger; // Debug
        public GameObject floorPlanGenSceneDebuggerPrefab;

        private BuildingGenerator buildingGenerator;
        private Vector2 scrollPos;

        private int selectedTab = 0;
        private string[] tabNames = { "Building Sets", "Method Sets", "Debug" };

        private bool _generationRunning = false;


        [MenuItem("Tools/Building Generator")]
        public static void ShowWindow()
        {
            GetWindow<BuildingGeneratorTool>("Building Generator");
        }


        private void OnDisable()
        {
            _generationRunning = false;
        }


        private void OnGUI()
        {
            GUIStyle largeLabel = new GUIStyle(GUI.skin.label);
            largeLabel.fontSize = 16; // Set your desired font size
            largeLabel.normal.textColor = new Color(0.8f, 0.8f, 0.8f);
            largeLabel.alignment = TextAnchor.MiddleCenter;
            largeLabel.fontStyle = FontStyle.Bold;

            selectedTab = GUILayout.Toolbar(selectedTab, tabNames);

            GUILayout.Space(10); // Add spacing below tabs

            // Switch content based on selected tab
            var paddingTabs = new RectOffset(16, 16, 0, 132);
            switch (selectedTab)
            {
                case 0:
                    DrawBuildingGenSetsTab(largeLabel, paddingTabs);
                    break;
                case 1:
                    DrawGenMethodSetsTab(largeLabel, paddingTabs);
                    break;
                case 2:
                    DrawDebugTab(largeLabel, paddingTabs);
                    break;
            }

            if (buildingGenerator == null)
            {
                buildingGenerator = new BuildingGenerator();
            }

            // =================================== Bottom part ===================================

            GUIStyle customButtonStyle = new GUIStyle(GUI.skin.button);
            customButtonStyle.fontSize = 12; // Set desired font size
            customButtonStyle.fontStyle = FontStyle.Bold; // Optional: Bold, Italic, etc.
            customButtonStyle.padding = new RectOffset(16, 16, 16, 16);

            bool enableGenBut = false;

            if (buildingGeneratorSettings == null ||
                buildingGeneratorSettings.FloorPlanConfig == null ||
                buildingGeneratorSettings.BuildingAssetsPack == null ||
                methodGrowthSettings == null ||
                floorPlanGenSceneDebugger == null ||
                floorPlanGenSceneDebuggerPrefab == null ||
                _generationRunning)
            {
                //customButtonStyle.normal.textColor = Color.red;
                enableGenBut = false;
            }
            else
            {
                //customButtonStyle.normal.textColor = Color.white;
                enableGenBut = true;
            }


            // GENERATE BUTTON
            GUI.enabled = enableGenBut;
            GUILayout.Space(16);
            //EditorGUILayout.BeginHorizontal();
            //GUILayout.FlexibleSpace();
            if (!Application.isPlaying)
            {
                if (GUILayout.Button("Generate", customButtonStyle))
                {
                    floorPlanGenSceneDebugger.Init(buildingGeneratorSettings.BuildingAssetsPack);
                    buildingGenerator.GenerateBuilding(buildingGeneratorSettings, methodGrowthSettings, floorPlanGenSceneDebugger, GenerationFinished);
                    _generationRunning = true;
                }
            }
            else
            {
                if (GUILayout.Button("Generate Debugging", customButtonStyle))
                {
                    if (Application.isPlaying)
                    {
                        floorPlanGenSceneDebugger.Init(buildingGeneratorSettings.BuildingAssetsPack);
                        buildingGenerator.GenerateBuilding(buildingGeneratorSettings, methodGrowthSettings, floorPlanGenSceneDebugger, GenerationFinished);
                        _generationRunning = true;
                    }
                }
            }
            GUI.enabled = true;
            //EditorGUILayout.EndHorizontal();

            // PROGRESS BAR
            //GUILayout.FlexibleSpace();
            float progress = buildingGenerator.GenerationProgress();
            if (progress >= 0 && progress < 1)
            {
                Rect r = EditorGUILayout.BeginVertical();
                EditorGUI.ProgressBar(r, buildingGenerator.GenerationProgress(), "Generating...");
                GUILayout.Space(18);
                EditorGUILayout.EndVertical();
                Repaint();
            }
            else if (progress == 1)
            {
                Rect r = EditorGUILayout.BeginVertical();
                EditorGUI.ProgressBar(r, 1, "Done");
                GUILayout.Space(18);
                EditorGUILayout.EndVertical();
                Repaint();
            }
            else
            {
                Rect r = EditorGUILayout.BeginVertical();
                EditorGUI.ProgressBar(r, 0, "");
                GUILayout.Space(18);
                EditorGUILayout.EndVertical();
                Repaint();
            }


            GUI.enabled = true;

            /*
            if (buildingGenerator != null)
            {
                Editor editor = Editor.CreateEditor(buildingGenerator);
                editor.OnInspectorGUI(); // This expands the ScriptableObject fields
            }
            */
        }


        private void DrawBuildingGenSetsTab(GUIStyle largeLabel, RectOffset padding)
        {
            GUIStyle scrollStyle = new GUIStyle(GUI.skin.scrollView);
            scrollStyle.padding = padding;
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, scrollStyle);
            //scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(position.width - _spacing), GUILayout.Height(position.height - 120));

            // BUILDING GENERATOR SETTINGS
            //GUILayout.Space(16);
            //GUILayout.Label("General Building Generator Settings", largeLabel);
            //EditorGUILayout.BeginVertical("box");
            buildingGeneratorSettings = (BuildingGeneratorSettings)EditorGUILayout.ObjectField("Building Gen Sets", buildingGeneratorSettings, typeof(BuildingGeneratorSettings), false);
            if (buildingGeneratorSettings != null)
            {
                Editor editor = Editor.CreateEditor(buildingGeneratorSettings);
                editor.OnInspectorGUI(); // This expands the ScriptableObject fields
                DestroyImmediate(editor);
            }
            //EditorGUILayout.EndVertical();

            EditorGUILayout.EndScrollView();
        }

        private void DrawGenMethodSetsTab(GUIStyle largeLabel, RectOffset padding)
        {
            GUIStyle scrollStyle = new GUIStyle(GUI.skin.scrollView);
            scrollStyle.padding = padding;
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, scrollStyle);
            //scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(position.width - _spacing), GUILayout.Height(position.height - 120));

            // METHOD SETTINGS
            //GUILayout.Space(16);
            //GUILayout.Label("Advanced Generation Method Settings", largeLabel);
            //EditorGUILayout.BeginVertical("box");
            methodGrowthSettings = (MethodGrowthSettings)EditorGUILayout.ObjectField("Gen Method Sets", methodGrowthSettings, typeof(MethodGrowthSettings), false);
            if (methodGrowthSettings != null)
            {
                Editor editor = Editor.CreateEditor(methodGrowthSettings);
                editor.OnInspectorGUI(); // This expands the ScriptableObject fields
                DestroyImmediate(editor);
            }
            //EditorGUILayout.EndVertical();

            EditorGUILayout.EndScrollView();
        }

        private void DrawDebugTab(GUIStyle largeLabel, RectOffset padding)
        {
            GUIStyle scrollStyle = new GUIStyle(GUI.skin.scrollView);
            scrollStyle.padding = padding;
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, scrollStyle);
            //scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(position.width - _spacing), GUILayout.Height(position.height - 120));

            // SCENE DEBUGGER
            //GUILayout.Space(16);
            //GUILayout.Label("Scene Debug", largeLabel);
            floorPlanGenSceneDebugger = (FloorPlanGenSceneDebugger)EditorGUILayout.ObjectField("Scene Debugger", floorPlanGenSceneDebugger, typeof(FloorPlanGenSceneDebugger), true);

            GUILayout.Space(16);
            floorPlanGenSceneDebuggerPrefab = (GameObject)EditorGUILayout.ObjectField("Scene Debugger Prefab", floorPlanGenSceneDebuggerPrefab, typeof(GameObject), false);
            if (GUILayout.Button("Create Scene Debugger Object"))
            {
                CreateSceneDebugger();
            }

            EditorGUILayout.EndScrollView();
        }

        private void CreateSceneDebugger()
        {
            var go = Instantiate(floorPlanGenSceneDebuggerPrefab);
            floorPlanGenSceneDebugger = go.GetComponent<FloorPlanGenSceneDebugger>();
        }

        void GenerationFinished()
        {
            _generationRunning = false;
        }
    }
}
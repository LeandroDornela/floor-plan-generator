using UnityEditor;
using UnityEngine;

namespace BuildingGenerator
{
    public class BuildingGeneratorTool : EditorWindow
    {
        // User defined
        public BuildingGeneratorSettings BuildingGeneratorSettings;
        public MethodGrowthSettings MethodGrowthSettings;



        // Generator runtime.
        private IBuildingInterpreter _buildingDataInterpreterInstance; // Scene visualization
        private BuildingGenerator _buildingGenerator;
        private bool _generationRunning = false;


        // Editor window
        private Vector2 _scrollPos;
        private int selectedTab = 0;
        private string[] tabNames = { "Generator", "Advanced Sets"};



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
            var paddingTabs = new RectOffset(16, 16, 0, 16);
            switch (selectedTab)
            {
                case 0:
                    DrawBuildingGenSetsTab(largeLabel, paddingTabs);
                    break;
                case 1:
                    DrawGenMethodSetsTab(largeLabel, paddingTabs);
                    break;
            }

            if (_buildingGenerator == null)
            {
                _buildingGenerator = new BuildingGenerator();
            }

            // =================================== Bottom part ===================================

            GUIStyle customButtonStyle = new GUIStyle(GUI.skin.button);
            customButtonStyle.fontSize = 12; // Set desired font size
            customButtonStyle.fontStyle = FontStyle.Bold; // Optional: Bold, Italic, etc.
            customButtonStyle.padding = new RectOffset(16, 16, 16, 16);

            bool enableGenBut = false;

            if (BuildingGeneratorSettings == null ||
                MethodGrowthSettings == null ||
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
            EditorGUILayout.BeginHorizontal();
            //GUILayout.FlexibleSpace();
            //if (!Application.isPlaying)
            //{
            if (GUILayout.Button("Generate New", customButtonStyle))
            {
                _buildingGenerator.GenerateBuilding(BuildingGeneratorSettings, MethodGrowthSettings);
                //_generationRunning = true;
            }

            if (GUILayout.Button("Regenerate", customButtonStyle, GUILayout.Width(128)))
            {
                var obj = Selection.activeObject;
                IBuildingInterpreter buildingInterpreter;
                if (obj != null)
                {
                    buildingInterpreter = ((GameObject)obj).GetComponent<IBuildingInterpreter>();

                    if (buildingInterpreter == null)
                    {
                        Debug.Log("No interpreter selected.");
                    }
                    else
                    {
                        _buildingGenerator.GenerateBuilding(BuildingGeneratorSettings, MethodGrowthSettings, buildingInterpreter);
                    }
                }

                //_generationRunning = true;
            }
            //}
            /*
            else
            {
                if (GUILayout.Button("Generate Debugging", customButtonStyle))
                {
                    if (Application.isPlaying)
                    {
                        _buildingGenerator.GenerateBuilding(BuildingGeneratorSettings, MethodGrowthSettings, BuildingDataInterpreterPrefab);
                        _generationRunning = true;
                    }
                }
            }
            */
            EditorGUILayout.EndHorizontal();
            GUI.enabled = true;


            // PROGRESS BAR
            //GUILayout.FlexibleSpace();
            float progress = _buildingGenerator.GenerationProgress();
            if (progress >= 0 && progress < 1)
            {
                Rect r = EditorGUILayout.BeginVertical();
                EditorGUI.ProgressBar(r, _buildingGenerator.GenerationProgress(), "Generating...");
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
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, scrollStyle);
            //scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(position.width - _spacing), GUILayout.Height(position.height - 120));

            // BUILDING GENERATOR SETTINGS
            //GUILayout.Space(16);
            //GUILayout.Label("General Building Generator Settings", largeLabel);
            //EditorGUILayout.BeginVertical("box");
            BuildingGeneratorSettings = (BuildingGeneratorSettings)EditorGUILayout.ObjectField("Building Gen Sets", BuildingGeneratorSettings, typeof(BuildingGeneratorSettings), false);
            if (BuildingGeneratorSettings != null)
            {
                Editor editor = Editor.CreateEditor(BuildingGeneratorSettings);
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
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, scrollStyle);
            //scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(position.width - _spacing), GUILayout.Height(position.height - 120));

            // METHOD SETTINGS
            //GUILayout.Space(16);
            //GUILayout.Label("Advanced Generation Method Settings", largeLabel);
            //EditorGUILayout.BeginVertical("box");
            MethodGrowthSettings = (MethodGrowthSettings)EditorGUILayout.ObjectField("Gen Method Sets", MethodGrowthSettings, typeof(MethodGrowthSettings), false);
            if (MethodGrowthSettings != null)
            {
                Editor editor = Editor.CreateEditor(MethodGrowthSettings);
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
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, scrollStyle);
            //scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(position.width - _spacing), GUILayout.Height(position.height - 120));

            // SCENE DEBUGGER
            //GUILayout.Space(16);
            //GUILayout.Label("Scene Debug", largeLabel);
            //buildingDataInterpreter = (FloorPlanGenSceneDebugger)EditorGUILayout.ObjectField("Scene Debugger", buildingDataInterpreter, typeof(FloorPlanGenSceneDebugger), true);

            /*
            GUILayout.Space(16);
            BuildingDataInterpreterPrefab = (GameObject)EditorGUILayout.ObjectField("Scene Debugger Prefab", BuildingDataInterpreterPrefab, typeof(GameObject), false);
            if (GUILayout.Button("Create Scene Debugger Object"))
            {
                CreateSceneDebugger();
            }
            */

            EditorGUILayout.EndScrollView();
        }

        /*
        private void CreateSceneDebugger()
        {
            var go = Instantiate(floorPlanGenSceneDebuggerPrefab);
            buildingDataInterpreter = go.GetComponent<FloorPlanGenSceneDebugger>();
        }
        */

        void GenerationFinished()
        {
            _generationRunning = false;
        }
    }
}
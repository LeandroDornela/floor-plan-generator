using UnityEditor;
using UnityEngine;

namespace BuildingGenerator
{
    // TODO: Block buttons during the generation.
    public class BuildingGeneratorTool : EditorWindow
    {
        public BuildingGeneratorSettings buildingGeneratorSettings; // Config
        public MethodGrowthSettings methodGrowthSettings; // Config
        public IFloorPlanConfig floorPlanConfig; // TODO: Will be replace by a full building config with more than one floor plan config.
        public FloorPlanGenSceneDebugger floorPlanGenSceneDebugger; // Debug

        private BuildingGenerator buildingGenerator;
        private Vector2 scrollPos;

        private int selectedTab = 0;
        private string[] tabNames = { "Plan Config", "Building Sets", "Method Sets", "Debug" };


        [MenuItem("Tools/Building Generator")]
        public static void ShowWindow()
        {
            GetWindow<BuildingGeneratorTool>("Building Generator");
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
            switch (selectedTab)
            {
                case 0:
                    DrawGeneralTab(largeLabel);
                    break;
                case 1:
                    DrawBuildingGenSetsTab(largeLabel);
                    break;
                case 2:
                    DrawGenMethodSetsTab(largeLabel);
                    break;
                case 3:
                    DrawDebugTab(largeLabel);
                    break;
            }

            if (buildingGenerator == null)
            {
                buildingGenerator = new BuildingGenerator();
            }

            GUIStyle customButtonStyle = new GUIStyle(GUI.skin.button);
            customButtonStyle.fontSize = 12; // Set desired font size
            customButtonStyle.fontStyle = FontStyle.Bold; // Optional: Bold, Italic, etc.

            if (buildingGeneratorSettings == null || methodGrowthSettings == null || floorPlanConfig == null)
            {
                customButtonStyle.normal.textColor = Color.red;
            }
            else
            {
                customButtonStyle.normal.textColor = Color.white;
            }


            GUILayout.Space(16);
            EditorGUILayout.BeginHorizontal();
            //GUILayout.FlexibleSpace();
            if (!Application.isPlaying)
            {
                if (GUILayout.Button("Generate", customButtonStyle, GUILayout.Height(40)))
                {
                    floorPlanGenSceneDebugger.Init(buildingGeneratorSettings.BuildingAssetsPack);
                    buildingGenerator.GenerateBuilding(buildingGeneratorSettings, methodGrowthSettings, floorPlanGenSceneDebugger, floorPlanConfig);
                }
            }
            else
            {
                if (GUILayout.Button("Generate Debugging", customButtonStyle, GUILayout.Height(40)))
                {
                    if (Application.isPlaying)
                    {
                        floorPlanGenSceneDebugger.Init(buildingGeneratorSettings.BuildingAssetsPack);
                        buildingGenerator.GenerateBuilding(buildingGeneratorSettings, methodGrowthSettings, floorPlanGenSceneDebugger, floorPlanConfig);
                    }
                }
            }


            EditorGUILayout.EndHorizontal();

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

        private void DrawGeneralTab(GUIStyle largeLabel)
        {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(position.width), GUILayout.Height(position.height - 120));

            // FLOOR PLAN CONGIF
            //GUILayout.Space(16);
            //GUILayout.Label("Floor Plan Configuration", largeLabel);
            //EditorGUILayout.BeginVertical("box");
            floorPlanConfig = (IFloorPlanConfig)EditorGUILayout.ObjectField("Floor Plan Config", floorPlanConfig, typeof(IFloorPlanConfig), false);
            if (floorPlanConfig != null)
            {
                Editor editor = Editor.CreateEditor(floorPlanConfig);
                editor.OnInspectorGUI(); // This expands the ScriptableObject fields
                DestroyImmediate(editor);
            }
            //EditorGUILayout.EndVertical();

            EditorGUILayout.EndScrollView();
        }

        private void DrawBuildingGenSetsTab(GUIStyle largeLabel)
        {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(position.width), GUILayout.Height(position.height - 120));

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

        private void DrawGenMethodSetsTab(GUIStyle largeLabel)
        {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(position.width), GUILayout.Height(position.height - 120));

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

        private void DrawDebugTab(GUIStyle largeLabel)
        {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(position.width), GUILayout.Height(position.height - 120));

            // SCENE DEBUGGER
            //GUILayout.Space(16);
            //GUILayout.Label("Scene Debug", largeLabel);
            floorPlanGenSceneDebugger = (FloorPlanGenSceneDebugger)EditorGUILayout.ObjectField("Scene Debugger", floorPlanGenSceneDebugger, typeof(FloorPlanGenSceneDebugger), true);

            EditorGUILayout.EndScrollView();
        }
    }
}
using System;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace BuildingGenerator
{
    [System.Serializable]
    public class BuildingGenerator
    {
        // OBS: Uma varialvel com a ref de uma classe serializada exposta no editor "nunca" será nula.
        private FloorPlanGenerator _floorPlanGenerator;
        private IBuildingInterpreter _buildingInterpreter;

        public Event GenerationStartedEvent = new Event();
        // Preferably for debugging to see the generation step by step.
        public Event<FloorPlanManager> FloorPlanUpdatedEvent = new Event<FloorPlanManager>();
        public Event<GeneratedBuildingData> GenerationFinishedEvent = new Event<GeneratedBuildingData>();


        public async UniTask<GeneratedBuildingData> GenerateBuilding(BuildingGeneratorSettings buildingGeneratorSettings, MethodGrowthSettings methodGrowthSettings, IBuildingInterpreter buildingInterpreter = null)
        {
            // Only instantiate a new interpreter if its given.
            if (buildingInterpreter != null)
            {
                _buildingInterpreter = buildingInterpreter;
                _buildingInterpreter.gameObject.name = buildingGeneratorSettings.BuildingConfig.FloorPlanConfig.name;
            }
            else if (buildingGeneratorSettings.BuildingDataInterpreterPrefab != null)
            {
                _buildingInterpreter = GameObject.Instantiate(buildingGeneratorSettings.BuildingDataInterpreterPrefab);
                _buildingInterpreter.gameObject.name = buildingGeneratorSettings.BuildingConfig.FloorPlanConfig.name;
            }

            // Initialize the scene building interpreter.
            if (_buildingInterpreter != null)
            {
                _buildingInterpreter.Init(this, buildingGeneratorSettings.BuildingConfig.BuildingAssetsPack);
            }
            else
            {
                Debug.LogWarning("Building interpreter is undefined.");
            }

            // Set debug logs.
            if (buildingGeneratorSettings.EnableDevLogs)
            {
                Utils.Debug._enable = true;
            }
            else
            {
                Utils.Debug._enable = false;
            }

            _floorPlanGenerator = new FloorPlanGenerator();
            _floorPlanGenerator.FloorPlanUpdatedEvent.Register(OnFloorPlanUpdated);
            FloorPlanData floorPlanData = buildingGeneratorSettings.BuildingConfig.FloorPlanConfig.GetFloorPlanData();

            var result = await _floorPlanGenerator.GenerateFloorPlans(buildingGeneratorSettings, methodGrowthSettings, floorPlanData, 1);

            GeneratedBuildingData generatedBuildingData = ScriptableObject.CreateInstance<GeneratedBuildingData>();
            generatedBuildingData.SetGeneratedPlans(result);
            if (buildingGeneratorSettings.SaveGeneratedPlanToAsset)
            {
                string fileName = $"{Guid.NewGuid()}.asset";
                string path = System.IO.Path.Combine("Assets", buildingGeneratorSettings.PlanGenPlanAssetsFolder, fileName);
                AssetDatabase.CreateAsset(generatedBuildingData, path);
                Debug.Log($"Generated building data saved to {path}.");
            }

            GenerationFinishedEvent.Invoke(generatedBuildingData);

            return generatedBuildingData;
        }

        // Gera um grande numero de resultados para análise de tempo de execução.
        public async UniTask<bool> DEBUG_GRAPH_GenerateBuilding(int graphSamples, BuildingGeneratorSettings buildingGeneratorSettings, MethodGrowthSettings methodGrowthSettings, IBuildingInterpreter buildingInterpreter = null)
        {
            // Only instantiate a new interpreter if its given.
            if (buildingInterpreter != null)
            {
                _buildingInterpreter = buildingInterpreter;
                _buildingInterpreter.gameObject.name = buildingGeneratorSettings.BuildingConfig.FloorPlanConfig.name;
            }
            else if (buildingGeneratorSettings.BuildingDataInterpreterPrefab != null)
            {
                _buildingInterpreter = GameObject.Instantiate(buildingGeneratorSettings.BuildingDataInterpreterPrefab);
                _buildingInterpreter.gameObject.name = buildingGeneratorSettings.BuildingConfig.FloorPlanConfig.name;
            }

            // Initialize the scene building interpreter.
            if (_buildingInterpreter != null)
            {
                _buildingInterpreter.Init(this, buildingGeneratorSettings.BuildingConfig.BuildingAssetsPack);
            }
            else
            {
                Debug.LogWarning("Building interpreter is undefined.");
            }

            // Set debug logs.
            if (buildingGeneratorSettings.EnableDevLogs)
            {
                Utils.Debug._enable = true;
            }
            else
            {
                Utils.Debug._enable = false;
            }

            _floorPlanGenerator = new FloorPlanGenerator();
            _floorPlanGenerator.FloorPlanUpdatedEvent.Register(OnFloorPlanUpdated);

            FloorPlanData floorPlanData = buildingGeneratorSettings.BuildingConfig.FloorPlanConfig.GetFloorPlanData();

            for (int i = 0; i < graphSamples; i++)
            {
                var result = await _floorPlanGenerator.GenerateFloorPlans(buildingGeneratorSettings, methodGrowthSettings, floorPlanData, 1);

                Debug.Log($"{floorPlanData.GridDimensions} done");

                floorPlanData.GridDimensions = floorPlanData.GridDimensions + new Vector2Int(2, 2);
            }

            return true;
        }


        public float GenerationProgress()
        {
            if (_floorPlanGenerator != null)
            {
                return _floorPlanGenerator.GenerationProgress;
            }

            return -1;
        }

        void OnFloorPlanUpdated(FloorPlanManager floorPlanManager)
        {
            FloorPlanUpdatedEvent.Invoke(floorPlanManager);
        }
    }
}
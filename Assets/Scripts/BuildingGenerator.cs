using com.cyborgAssets.inspectorButtonPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;

namespace BuildingGenerator
{
    [System.Serializable]
    public class BuildingGenerator
    {
        // OBS: Uma varialvel com a ref de uma classe serializada exposta no editor "nunca" será nula.
        private FloorPlanGenerator _floorPlanGenerator;
        //[SerializeField] private BuildingDataManager _buildingDataManager;

        //public BuildingGeneratorSettings buildingGeneratorSettings;

        /*
        [ProButton]
        public async void GenerateBuilding()
        {
            if (!Application.isPlaying) return;

            var result = await _floorPlanGenerator.GenerateFloorPlans(_buildingDataManager.GetFloorPlanData(), 1);

            foreach (var plan in result)
                plan.PrintFloorPlan();
        }
        */


        public async void GenerateBuilding(BuildingGeneratorSettings buildingGeneratorSettings, MethodGrowthSettings methodGrowthSettings, FloorPlanGenSceneDebugger sceneDebugger)
        {
            if (buildingGeneratorSettings.EnableDevLogs)
            {
                Utils.Debug._enable = true;
            }
            else
            {
                Utils.Debug._enable = false;
            }

            _floorPlanGenerator = new FloorPlanGenerator();
            FloorPlanData floorPlanData = buildingGeneratorSettings._testingFloorPlanConfig.GetFloorPlanData();
            var result = await _floorPlanGenerator.GenerateFloorPlans(buildingGeneratorSettings, methodGrowthSettings, sceneDebugger, floorPlanData, 1);
        }


        public float GenerationProgress()
        {
            if (_floorPlanGenerator != null)
            {
                return _floorPlanGenerator.GenerationProgress;
            }
            
            return -1;
        }
    }
}
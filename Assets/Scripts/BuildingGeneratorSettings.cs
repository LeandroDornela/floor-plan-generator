using UnityEngine;

namespace BuildingGenerator
{
    [CreateAssetMenu(fileName = "BuildingGeneratorSettings", menuName = "Building Generator/Building Generator Settings")]
    [System.Serializable]
    public class BuildingGeneratorSettings : ScriptableObject
    {
        [Tooltip("")]
        [SerializeField] private bool _useSeed = false;
        [Tooltip("")]
        [SerializeField] private int _seed = 0;
        [Tooltip("Maximum number that it will request the generation method to generate a valid floor plan.")]
        [SerializeField] private int _maxGenerationTries = 10;
        [Tooltip("Number of valid floor plans to generate. At the end choose the best option from the samples. Worst case generation = (_maxGenerationTries * _samples)")]
        [SerializeField] private int _samples = 10;
        [Space]
        [Tooltip("")]
        [SerializeField] private bool _saveGeneratedPlanToAsset = false;
        [SerializeField] private string _planGenPlanAssetsFolder = "Generated Plans";
        [Space]
        [Tooltip("")]
        [SerializeField] private BuildingConfig _buildingConfig;
        [Tooltip("")]
        [SerializeField] private IBuildingInterpreter _buildingDataInterpreterPrefab;


        [Header("Debug")]
        [SerializeField] private bool _screenshotPlan = true;
        [SerializeField] private bool _enableDevLogs = false;
        [SerializeField] private bool _saveGenStatsJson = true;


        public bool UseSeed => _useSeed;
        public int Seed => _seed;
        public int MaxGenerationTries => _maxGenerationTries;
        public int Samples => _samples;
        public bool ScreenshotPlan => _screenshotPlan;
        public bool EnableDevLogs => _enableDevLogs;
        public bool SaveGenStatsJson => _saveGenStatsJson;
        public BuildingConfig BuildingConfig => _buildingConfig;
        public IBuildingInterpreter BuildingDataInterpreterPrefab => _buildingDataInterpreterPrefab;
        public bool SaveGeneratedPlanToAsset => _saveGeneratedPlanToAsset;
        public string PlanGenPlanAssetsFolder => _planGenPlanAssetsFolder;
    }

}
using UnityEngine;

namespace BuildingGenerator
{
    [CreateAssetMenu(fileName = "BuildingGeneratorSettings", menuName = "Scriptable Objects/Building Generator Settings")]
    [System.Serializable]
    public class BuildingGeneratorSettings : ScriptableObject
    {
        [Header("Floor Plan Generator")]

        /// <summary>
        /// 
        /// </summary>
        [SerializeField] private bool _useSeed = false;

        /// <summary>
        /// 
        /// </summary>
        [SerializeField] private int _seed = 0;

        /// <summary>
        /// Maximum number that it will request the generation method to generate a valid floor plan.
        /// </summary>
        [SerializeField] private int _maxGenerationTries = 10;

        /// <summary>
        /// Number of valid floor plans to generate. At the end choose the best option from the samples.
        /// Worst case generation = (_maxGenerationTries * _samples)
        /// </summary>
        [SerializeField] private int _samples = 10;

        [SerializeField] private BuildingAssetsPack _buildingAssetsPack;
        [SerializeField] private IFloorPlanConfig _floorPlanConfig;


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
        public BuildingAssetsPack BuildingAssetsPack => _buildingAssetsPack;
        public IFloorPlanConfig FloorPlanConfig => _floorPlanConfig;
    }

}
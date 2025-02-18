using System;
using UnityEngine;


[Serializable]
public class BuildingDataManager
{
    [SerializeField] private TestingFloorPlansConfig testingFloorPlansConfig;

    [Obsolete]
    public FloorPlanConfig GetTestingFloorPlanConfig(int index = 0)
    {
        FloorPlanConfig config;
        config.GridDimensions = testingFloorPlansConfig.FloorPlanConfigs[index].GridDimensions;
        config.ZonesConfigs = testingFloorPlansConfig.FloorPlanConfigs[index].ZonesConfigs;
        config.Adjacencies = testingFloorPlansConfig.FloorPlanConfigs[index].Adjacencies;

        return config;
    }
}
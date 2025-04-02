using System;
using UnityEngine;


[Serializable]
public class BuildingDataManager
{
    [SerializeField] private IFloorPlanConfig floorPlanConfig; // TODO: must be in a moment a building config.


    public FloorPlanData GetFloorPlanData()
    {
        return floorPlanConfig.GetFloorPlanData();
    }
}
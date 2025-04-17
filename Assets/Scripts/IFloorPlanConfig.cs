using UnityEngine;

namespace BuildingGenerator
{
public abstract class IFloorPlanConfig : ScriptableObject
{
    // Convert a user friendly floor plan config in floor plan data.
    public abstract FloorPlanData GetFloorPlanData();
}
}
using UnityEngine;
using System.Collections.Generic;

namespace BuildingGenerator
{
public struct FloorPlanData
{
    public string FloorPlanId;
    public Vector2Int GridDimensions;
    public Dictionary<string, ZoneData> ZonesConfigs;
    public Dictionary<string, string[]> Adjacencies;

    public bool IsValid()
    {
        if(GridDimensions.x <= 0 || GridDimensions.y <= 0) return false;
        if(ZonesConfigs == null || ZonesConfigs.Count == 0) return false;
        if(Adjacencies == null || Adjacencies.Count == 0) return false;

        return true;
    }

    public FloorPlanData(string floorPlanId, Vector2Int gridDimensions, Dictionary<string, ZoneData> zonesConfigs, Dictionary<string, string[]> adjacencies)
    {
        FloorPlanId = floorPlanId;
        GridDimensions = gridDimensions;
        ZonesConfigs = zonesConfigs;
        Adjacencies = adjacencies;
    }
}
}
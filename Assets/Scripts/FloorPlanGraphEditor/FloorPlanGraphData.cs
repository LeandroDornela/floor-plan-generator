using UnityEngine;
using System.Collections.Generic;
using BuildingGenerator;
using System;

[CreateAssetMenu(fileName = "NewFloorPlanGraph", menuName = "Building Generator/Floor Plan Graph")]
public class FloorPlanGraphData : ScriptableObject
{
    public List<DataNodeModel> nodes = new();

    public FloorPlanData ConvertToFloorPlanData()
    {
        var planGUID = Guid.NewGuid().ToString();
        var planId = "testing";

        // Get the grid dimensions.
        var dims = new Vector2Int(10, 10);

        // Convert TestZoneConfig's to ZoneData's.
        Dictionary<string, ZoneData> zonesConfigs = new Dictionary<string, ZoneData>(nodes.Count);
        foreach (var model in nodes)
        {
            TestZoneConfig zoneConfig = new TestZoneConfig();
            zoneConfig._zoneID = model.zoneId;
            zoneConfig._parentZoneGUID = model.parentGUID;
            zoneConfig._areaRatio = model.areaRatio;
            zoneConfig._presetArea = model.presetAreaTexture;
            zoneConfig._hasOutsideDoor = model.hasOutsideDoor;

            zonesConfigs[model.guid] = zoneConfig.ToZoneConfig(dims);
        }

        // Get the adjacencies configuration.
        Dictionary<string, string[]> adj = new Dictionary<string, string[]>();
        foreach (var model in nodes)
        {
            if (model.adjacenciesGUIDs != null && model.adjacenciesGUIDs.Count > 0)
            {
                string[] adjacencies = new string[model.adjacenciesGUIDs.Count];

                for (int i = 0; i < adjacencies.Length; i++)
                {
                    adjacencies[i] = model.adjacenciesGUIDs[i];
                }

                adj.Add(model.guid, adjacencies);
            }
        }

        // Create a FloorPlanData from a TestFloorPlanConfig.
        return new FloorPlanData(planGUID, planId, dims, zonesConfigs, adj);
    }
}


[System.Serializable]
public class DataNodeModel
{
    public string guid;
    public Vector2 position;

    public string zoneId;
    public float areaRatio = 1;
    public bool hasOutsideDoor;
    public Texture2D presetAreaTexture;

    public string parentGUID;
    public List<string> childrenGUIDs = new();
    public List<string> adjacenciesGUIDs = new();
}
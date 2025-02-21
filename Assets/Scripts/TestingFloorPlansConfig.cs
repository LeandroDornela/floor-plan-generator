using UnityEngine;
using System;
using AYellowpaper.SerializedCollections;

[Serializable]
public struct GetTestingFloorPlanConfig
{
    public string FloorPlanId;
    public Vector2Int GridDimensions;
    [SerializedDictionary("Zone ID", "Config")]
    public SerializedDictionary<string, ZoneConfig> ZonesConfigs;
    [SerializedDictionary("Zone ID", "Adj. IDs")]
    public SerializedDictionary<string, string[]> Adjacencies;
    [SerializedDictionary("Zone ID", "Cells")]
    public SerializedDictionary<string, string> ZonesAreasPresets;
}

[CreateAssetMenu(fileName = "TestingFloorPlansConfig", menuName = "Scriptable Objects/TestingFloorPlansConfig")]
public class TestingFloorPlansConfig : ScriptableObject
{
    public GetTestingFloorPlanConfig[] FloorPlanConfigs;
}

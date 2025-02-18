using UnityEngine;
using System;
using AYellowpaper.SerializedCollections;

[Serializable]
public struct GetTestingFloorPlanConfig
{
    public string FloorPlanId;
    public Vector2Int GridDimensions;
    public SerializedDictionary<string, ZoneConfig> ZonesConfigs;
    public SerializedDictionary<string, string[]> Adjacencies;
}

[CreateAssetMenu(fileName = "TestingFloorPlansConfig", menuName = "Scriptable Objects/TestingFloorPlansConfig")]
public class TestingFloorPlansConfig : ScriptableObject
{
    public GetTestingFloorPlanConfig[] FloorPlanConfigs;
}

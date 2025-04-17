using UnityEngine;
using System;
using AYellowpaper.SerializedCollections;
using System.Collections.Generic;

namespace BuildingGenerator
{
[Serializable]
public struct TestZoneConfig
{
    [SerializeField] private string _parentZoneId;
    [SerializeField] private float _areaRatio;
    [SerializeField] private Texture2D _presetArea;

    public ZoneData ToZoneConfig(Vector2Int gridDimensions)
    {
        int[] presetArea = null;
        if(_presetArea != null)
        {
            presetArea = TextureToIntArray(_presetArea, gridDimensions);
        }

        return new ZoneData(_parentZoneId, _areaRatio, presetArea);
    }

    public int[] TextureToIntArray(Texture2D texture, Vector2Int gridDimensions)
    {
        /*
        if(!texture.Reinitialize(gridDimensions.x, gridDimensions.y))
        {
            Debug.LogError("Can't resize texture.");
            return default;
        }
        */

        Color[] colors = texture.GetPixels();
        int[] result = new int[colors.Length];

        for(int i = 0; i < colors.Length; i++)
        {
            if(colors[i].r == 0)
            {
                result[i] = 0;
            }
            else
            {
                result[i] = 1;
            }
        }

        /*
        string a = "";
        int k = 0;
        for(int j = 0; j < gridDimensions.y; j++)
        {
            for(int i = 0; i < gridDimensions.x; i++)
            {
                a += $"{result[k]} "; 
                k++;
            }
            a += "\n";
        }
        Debug.Log(a);
        */

        return result;
    }
}

[Serializable]
public struct TestFloorPlanConfig
{
    public string FloorPlanId;
    public Vector2Int GridDimensions;

    [SerializedDictionary("Zone ID", "Config")]
    public SerializedDictionary<string, TestZoneConfig> ZonesConfigs;

    [SerializedDictionary("Zone ID", "Adj. IDs")]
    public SerializedDictionary<string, string[]> Adjacencies;

    [SerializedDictionary("Zone ID", "Cells")]
    public SerializedDictionary<string, string> ZonesAreasPresets;
}


[CreateAssetMenu(fileName = "TestingFloorPlansConfig", menuName = "Scriptable Objects/TestingFloorPlansConfig")]
public class TestingFloorPlansConfig : IFloorPlanConfig
{
    public TestFloorPlanConfig[] FloorPlanConfigs;


    public override FloorPlanData GetFloorPlanData()
    {
        int index = 0;
        
        // Get the grid dimensions.
        var dims = FloorPlanConfigs[index].GridDimensions;

        // Convert TestZoneConfig's to ZoneData's.
        Dictionary<string, ZoneData> zonesConfigs = new Dictionary<string, ZoneData>(FloorPlanConfigs[index].ZonesConfigs.Count);
        foreach(var zConf in FloorPlanConfigs[index].ZonesConfigs)
        {
            zonesConfigs[zConf.Key] = zConf.Value.ToZoneConfig(dims);
        }

        // Get the adjacencies configuration.
        var adj = FloorPlanConfigs[index].Adjacencies;

        // Create a FloorPlanData from a TestFloorPlanConfig.
        return new FloorPlanData(dims, zonesConfigs, adj);
    }
}
}
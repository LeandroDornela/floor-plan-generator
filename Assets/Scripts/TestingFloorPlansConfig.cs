using UnityEngine;
using System;
using AYellowpaper.SerializedCollections;
using System.Collections.Generic;

namespace BuildingGenerator
{
    [Serializable]
    public struct TestZoneConfig
    {
        public string _zoneID;
        public string _parentZoneGUID;
        public float _areaRatio;
        public Texture2D _presetArea;
        public bool _hasOutsideDoor;
        public bool _hasWindows;


        public ZoneData ToZoneConfig(Vector2Int gridDimensions)
        {
            int[] presetArea = null;
            if (_presetArea != null)
            {
                presetArea = TextureToIntArray(_presetArea, gridDimensions);
            }

            return new ZoneData(_zoneID, _parentZoneGUID, _areaRatio, presetArea, _hasOutsideDoor, _hasWindows);
        }

        public int[] TextureToIntArray(Texture2D texture, Vector2Int gridDimensions)
        {

            Texture2D resizedTex = Utils.ResizeWithNearest(texture, gridDimensions.x, gridDimensions.y);

            Color[] colors = resizedTex.GetPixels();
            int[] result = new int[colors.Length];

            for (int i = 0; i < colors.Length; i++)
            {
                if (colors[i].r == 0)
                {
                    result[i] = 0;
                }
                else
                {
                    result[i] = 1;
                }
            }

            return result;
        }
    }


    [Serializable]
    public struct TestFloorPlanConfig
    {
        public string FloorPlanGUID;
        public string FloorPlanId;
        public Vector2Int GridDimensions;

        [SerializedDictionary("Zone ID", "Config")]
        public SerializedDictionary<string, TestZoneConfig> ZonesConfigs;

        [SerializedDictionary("Zone ID", "Adj. IDs")]
        public SerializedDictionary<string, string[]> Adjacencies;
    }


    [CreateAssetMenu(fileName = "TestingFloorPlansConfig", menuName = "Scriptable Objects/TestingFloorPlansConfig")]
    public class TestingFloorPlansConfig : IFloorPlanConfig
    {
        public TestFloorPlanConfig[] FloorPlanConfigs;


        public override FloorPlanData GetFloorPlanData()
        {
            int index = 0;

            var floorPlanGUID = FloorPlanConfigs[index].FloorPlanGUID;
            var planId = FloorPlanConfigs[index].FloorPlanId;

            // Get the grid dimensions.
            var dims = FloorPlanConfigs[index].GridDimensions;

            // Convert TestZoneConfig's to ZoneData's.
            Dictionary<string, ZoneData> zonesConfigs = new Dictionary<string, ZoneData>(FloorPlanConfigs[index].ZonesConfigs.Count);
            foreach (var zConf in FloorPlanConfigs[index].ZonesConfigs)
            {
                zonesConfigs[zConf.Key] = zConf.Value.ToZoneConfig(dims);
            }

            // Get the adjacencies configuration.
            var adj = FloorPlanConfigs[index].Adjacencies;

            // Create a FloorPlanData from a TestFloorPlanConfig.
            return new FloorPlanData(floorPlanGUID, planId, dims, zonesConfigs, adj);
        }
    }
}
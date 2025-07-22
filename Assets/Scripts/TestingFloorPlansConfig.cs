using UnityEngine;
using System;
using AYellowpaper.SerializedCollections;
using System.Collections.Generic;
using com.cyborgAssets.inspectorButtonPro;

namespace BuildingGenerator
{
    [Serializable]
    public class TestZoneConfig
    {
        public string _zoneID;
        [NaughtyAttributes.ReadOnly] public string _zoneGUID = Guid.NewGuid().ToString();
        public string _parentZoneGUID;
        public float _areaRatio = 1;
        public float _desiredAspectRatio = 1;
        public Texture2D _presetArea;
        public bool _hasOutsideDoor;
        public bool _hasWindows;


        public ZoneData ToZoneConfig(Vector2Int gridDimensions)
        {
            int[] presetArea = null;
            if (_presetArea != null)
            {
                presetArea = Utils.TextureToIntArray(_presetArea, gridDimensions);
            }

            Guid.TryParse(_parentZoneGUID, out var parentGUI);

            return new ZoneData(Guid.NewGuid(), _zoneID, parentGUI, _areaRatio, _desiredAspectRatio, presetArea, _hasOutsideDoor, _hasWindows);
        }
    }


    [Serializable]
    public struct TestFloorPlanConfig
    {
        public string FloorPlanId;
        public Vector2Int GridDimensions;

        [SerializedDictionary("Zone GUID", "Config")]
        public SerializedDictionary<string, TestZoneConfig> ZonesConfigs;

        [SerializedDictionary("Zone GUID", "Adj. IDs")]
        public SerializedDictionary<string, string[]> Adjacencies;
    }


    [CreateAssetMenu(fileName = "TestingFloorPlansConfig", menuName = "Building Generator/TestingFloorPlansConfig")]
    public class TestingFloorPlansConfig : IFloorPlanConfig
    {
        public TestFloorPlanConfig[] FloorPlanConfigs;


        public override FloorPlanData GetFloorPlanData()
        {
            int index = 0;

            var planId = FloorPlanConfigs[index].FloorPlanId;

            // Get the grid dimensions.
            var dims = FloorPlanConfigs[index].GridDimensions;

            // Convert TestZoneConfig's to ZoneData's.
            Dictionary<Guid, ZoneData> zonesConfigs = new Dictionary<Guid, ZoneData>(FloorPlanConfigs[index].ZonesConfigs.Count);
            foreach (var zConf in FloorPlanConfigs[index].ZonesConfigs)
            {
                zonesConfigs[Guid.Parse(zConf.Key)] = zConf.Value.ToZoneConfig(dims);
            }

            // Get the adjacencies configuration.
            Dictionary<Guid, Guid[]> adj = new Dictionary<Guid, Guid[]>();
            foreach (var adjRule in FloorPlanConfigs[index].Adjacencies)
            {
                Guid[] zoneAdjs = new Guid[adjRule.Value.Length];

                for (int i = 0; i < adjRule.Value.Length; i++)
                {
                    zoneAdjs[i] = Guid.Parse(adjRule.Value[i]);
                }

                adj.Add(Guid.Parse(adjRule.Key), zoneAdjs);
            }

            // Create a FloorPlanData from a TestFloorPlanConfig.
            return new FloorPlanData(planId, dims, zonesConfigs, adj);
        }


        private void UpdateToGUIDSys()
        {
            for (int i = 1; i < FloorPlanConfigs.Length; i++)
            {
                UpdatePlan(ref FloorPlanConfigs[i]);
            }
        }
        

        void UpdatePlan(ref TestFloorPlanConfig plan)
        {
            // Adjacencies.
            SerializedDictionary<string, string[]> newDict = new SerializedDictionary<string, string[]>();
            foreach (var rule in plan.Adjacencies)
            {
                string[] newZonesGUIDs = new string[rule.Value.Length];
                for (int i = 0; i < rule.Value.Length; i++)
                {
                    newZonesGUIDs[i] = plan.ZonesConfigs[rule.Value[i]]._zoneGUID;
                }

                newDict.Add(plan.ZonesConfigs[rule.Key]._zoneGUID, newZonesGUIDs);
            }
            plan.Adjacencies = new SerializedDictionary<string, string[]>(newDict);

            // Zones
            SerializedDictionary<string, TestZoneConfig> newZones = new SerializedDictionary<string, TestZoneConfig>();
            foreach (var zone in plan.ZonesConfigs)
            {
                TestZoneConfig config = zone.Value;
                config._zoneID = zone.Key;
                newZones.Add(config._zoneGUID, config);
            }
            plan.ZonesConfigs = new SerializedDictionary<string, TestZoneConfig>(newZones);
        }
    }
}
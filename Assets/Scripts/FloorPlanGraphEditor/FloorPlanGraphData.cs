using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEditor;

namespace BuildingGenerator
{
    [CreateAssetMenu(fileName = "NewFloorPlanGraph", menuName = "Building Generator/Floor Plan Graph")]
    public class FloorPlanGraphData : IFloorPlanConfig
    {
        public List<DataNodeModel> nodes = new List<DataNodeModel>();

        public override FloorPlanData GetFloorPlanData()
        {
            var planId = "testing";
            var planDimensions = new Vector2Int(25, 25);

            // Covert the data in the List "nodes data models" to ZoneData Dictionary.
            // And the adjacency rules for the nodes that have a adjacent zone set.
            Dictionary<Guid, ZoneData> zonesConfigs = new Dictionary<Guid, ZoneData>();
            Dictionary<Guid, Guid[]> adjacencyRules = new Dictionary<Guid, Guid[]>();
            foreach (var zDataModel in nodes)
            {
                // Convert preset area texture to int array.
                int[] presetArea = null;
                if (zDataModel.presetAreaTexture != null)
                {
                    presetArea = Utils.TextureToIntArray(zDataModel.presetAreaTexture, planDimensions);
                }

                Guid guid = Guid.Parse(zDataModel.guid);
                Guid parentGuid;
                if (zDataModel.parentGUID == null || zDataModel.parentGUID == string.Empty)
                {
                    parentGuid = Guid.Empty;
                }
                else
                {
                    parentGuid = Guid.Parse(zDataModel.parentGUID);
                }

                // Create the ZoneData from zone config at the node model.
                ZoneData newZoneData = new ZoneData
                (
                    guid,
                    zDataModel.zoneId,
                    parentGuid,
                    zDataModel.areaRatio,
                    presetArea,
                    zDataModel.hasOutsideDoor,
                    zDataModel.HasOutsideWindows
                );

                // Add the current zone data to the dictionary using the GUID AS KEY.
                zonesConfigs.Add(new Guid(zDataModel.guid), newZoneData);

                // the Check for adjacency rules.
                if (zDataModel.adjacenciesGUIDs != null && zDataModel.adjacenciesGUIDs.Count > 0)
                {
                    Guid[] adjacencies = new Guid[zDataModel.adjacenciesGUIDs.Count];

                    for (int i = 0; i < adjacencies.Length; i++)
                    {
                        adjacencies[i] = new Guid(zDataModel.adjacenciesGUIDs[i]);
                    }

                    // Add the current zone data to the dictionary using the GUID AS KEY.
                    adjacencyRules.Add(new Guid(zDataModel.guid), adjacencies);
                }
            }

            // Create a FloorPlanData.
            return new FloorPlanData(planId, planDimensions, zonesConfigs, adjacencyRules);
        }
    }
}
using UnityEngine;
using System.Collections.Generic;
using System;

namespace BuildingGenerator
{
    public struct FloorPlanData
    {
        public Guid FloorPlanGUID;
        public string FloorPlanId;
        public Vector2Int GridDimensions;
        public Dictionary<Guid, ZoneData> ZonesConfigs;
        public Dictionary<Guid, Guid[]> Adjacencies;

        public bool IsValid()
        {
            if (GridDimensions.x <= 0 || GridDimensions.y <= 0) return false;
            if (ZonesConfigs == null || ZonesConfigs.Count == 0) return false;
            if (Adjacencies == null || Adjacencies.Count == 0) return false;

            return true;
        }

        public FloorPlanData(string floorPlanId, Vector2Int gridDimensions, Dictionary<Guid, ZoneData> zonesConfigs, Dictionary<Guid, Guid[]> adjacencies)
        {
            FloorPlanGUID = Guid.NewGuid();
            FloorPlanId = floorPlanId;
            GridDimensions = gridDimensions;
            ZonesConfigs = zonesConfigs;
            Adjacencies = adjacencies;
        }
    }
}
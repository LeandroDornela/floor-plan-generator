using System;
using System.Collections.Generic;
using UnityEngine;

namespace BuildingGenerator
{
    // TODO: encapsulation.
    [System.Serializable]
    public class DataNodeModel
    {
        public string guid;
        public Vector2 position;

        public string zoneId;
        public float areaRatio = 1;
        public float desiredAspectRatio = 1;
        public bool hasOutsideDoor;
        public bool HasOutsideWindows;
        public Texture2D presetAreaTexture;

        public string parentGUID;
        public List<string> childrenGUIDs = new();
        public List<string> adjacenciesGUIDs = new();
    }
}
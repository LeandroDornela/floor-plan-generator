using System;
using UnityEngine;

namespace BuildingGenerator
{
    public struct ZoneData
    {
        private Guid _guid;
        private string _zoneID;
        private Guid _parentZoneGUID;
        private float _areaRatio;
        private float _desiredAspectRatio;
        private int[] _presetArea;
        private bool _hasOutsideDoor;
        private bool _hasWindows;

        public Guid GUID => _guid;
        public string ZoneID => _zoneID;
        public Guid ParentZoneGUID => _parentZoneGUID;
        public float AreaRatio => _areaRatio;
        public float DesiredAspectRatio => _desiredAspectRatio;
        public int[] PresetArea => _presetArea;
        public bool HasOutsideDoor => _hasOutsideDoor;
        public bool HasWindows => _hasWindows;

        public bool HasPresetArea => _presetArea?.Length > 0;


        public ZoneData(Guid guid, string zoneID, Guid parentZoneGUID, float areaRatio, float desiredAspectRatio, int[] presetArea, bool hasOutsideDoor, bool hasWindows)
        {
            if (guid == Guid.Empty)
            {
                Debug.LogError("Invalid zone GUID.");
            }

            _guid = guid;
            _zoneID = zoneID;
            _parentZoneGUID = parentZoneGUID;
            _areaRatio = areaRatio;
            _desiredAspectRatio = desiredAspectRatio;
            _presetArea = presetArea;
            _hasOutsideDoor = hasOutsideDoor;
            _hasWindows = hasWindows;
        }
    }
}
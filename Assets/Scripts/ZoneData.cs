namespace BuildingGenerator
{
    public struct ZoneData
    {
        private string _zoneID;
        private string _parentZoneGUID;
        private float _areaRatio;
        private int[] _presetArea;
        private bool _hasOutsideDoor;
        private bool _hasWindows;

        public string ZoneID => _zoneID;
        public string ParentZoneGUID => _parentZoneGUID;
        public float AreaRatio => _areaRatio;
        public int[] PresetArea => _presetArea;
        public bool HasOutsideDoor => _hasOutsideDoor;
        public bool HasWindows => _hasWindows;

        public bool HasPresetArea => _presetArea?.Length > 0;


        public ZoneData(string zoneID, string parentZoneGUID, float areaRatio, int[] presetArea, bool hasOutsideDoor, bool hasWindows)
        {
            _parentZoneGUID = parentZoneGUID;
            _zoneID = zoneID;
            _areaRatio = areaRatio;
            _presetArea = presetArea;
            _hasOutsideDoor = hasOutsideDoor;
            _hasWindows = hasWindows;
        }
    }
}
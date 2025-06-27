namespace BuildingGenerator
{
public struct ZoneData
{
    private string _parentZoneId;
    private float _areaRatio;
    private int[] _presetArea;
    private bool _hasOutsideDoor;
    private bool _hasWindows;

    public string ParentZoneId => _parentZoneId;
    public float AreaRatio => _areaRatio;
    public int[] PresetArea => _presetArea;
    public bool HasOutsideDoor => _hasOutsideDoor;
    public bool HasWindows => _hasWindows;

    public bool HasPresetArea => _presetArea?.Length > 0;
    

    public ZoneData(string parentZoneId, float areaRatio, int[] presetArea, bool hasOutsideDoor, bool hasWindows)
    {
        _parentZoneId = parentZoneId;
        _areaRatio = areaRatio;
        _presetArea = presetArea;
        _hasOutsideDoor = hasOutsideDoor;
        _hasWindows = hasWindows;
    }
}
}
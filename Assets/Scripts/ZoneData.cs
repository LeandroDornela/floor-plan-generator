using UnityEngine;

public struct ZoneData
{
    private string _parentZoneId;
    private float _areaRatio;
    private int[] _presetArea;

    public string ParentZoneId => _parentZoneId;
    public float AreaRatio => _areaRatio;
    public int[] PresetArea => _presetArea;
    

    public ZoneData(string parentZoneId, float areaRatio, int[] presetArea)
    {
        _parentZoneId = parentZoneId;
        _areaRatio = areaRatio;
        _presetArea = presetArea;
    }
}
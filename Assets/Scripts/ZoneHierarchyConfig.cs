using AYellowpaper.SerializedCollections;
using com.cyborgAssets.inspectorButtonPro;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ZoneConfig
{
    //[SerializeField] private string _zoneId;
    //[SerializeField] [Range(0, 1)] private float _relativeArea;
    [SerializeField] private string _parentZoneId;

    //public string ZoneId => _zoneId;
    //public float RelativeArea => _relativeArea;
    public string ParentZoneId => _parentZoneId;



    // TODO: move the jason to a class that hold all the settings for creation.
    /*
    public string ExportToJSON()
    {
        return JsonUtility.ToJson(this);
    }

    public void OverrideFromJSON(string source)
    {
        JsonUtility.FromJsonOverwrite(source, this);
    }
    */
}


[CreateAssetMenu(fileName = "ZoneHierarchyConfig", menuName = "Scriptable Objects/ZoneHierarchyConfig")]
public class ZoneHierarchyConfig : ScriptableObject
{
    [SerializedDictionary("ZoneId", "Settings")]
    public SerializedDictionary<string, ZoneConfig> _zonesConfigs;
    [SerializedDictionary("ZoneId", "Adjacencies")]
    public SerializedDictionary<string, string[]> _adjacencies;

    //[SerializeField] private TextAsset _hierarchyConfigJSON;

    public Dictionary<string, Zone> zonesInstances;

    public List<Zone> GetZoneHierarchy()
    {
        zonesInstances = new Dictionary<string, Zone>();

        // Create all zones.
        foreach(var zone in _zonesConfigs)
        {
            zonesInstances.Add(zone.Key, new Zone(zone.Key));
        }

        // Set the parents and children of the zones.
        foreach(var zone in zonesInstances)
        {
            string parentZoneId = _zonesConfigs[zone.Key].ParentZoneId;

            if (parentZoneId != string.Empty)
            {
                Zone parentZone = zonesInstances[parentZoneId];
                zone.Value.SetParentZone(parentZone);
                parentZone.AddChildZone(zone.Value);
            }
        }

        // Set the adjacent zones.
        foreach(var zoneId in _adjacencies)
        {
            foreach(var adjacentZoneId in zoneId.Value)
            {
                zonesInstances[zoneId.Key].AddAdjacentZone(zonesInstances[adjacentZoneId]);
                zonesInstances[adjacentZoneId].AddAdjacentZone(zonesInstances[zoneId.Key]);
            }
        }

        // Assuming the can start with multple zones, skipping the first zone that shold be the full terrain.
        List<Zone> rootZones = new List<Zone>();

        foreach(var zone in zonesInstances)
        {
            // If is a root.
            if(zone.Value._parentZone == null)
            {
                rootZones.Add(zone.Value);
            }
        }

        return rootZones;
    }
}

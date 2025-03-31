using System;
using System.Collections.Generic;
using UnityEngine;

/*
    Estrutura hierarquica atual.

    -Root0
        -Exterior
        -Interior
            -Publico
                -Sala
                -Cozinha
            -Privado
                -Quarto
                -Banheiro
    ------------------------------------
    -Root0
        -Interior
            -Publico
                -Sala
                -Cozinha
            -Privado
                -Quarto
                -Banheiro
    -Root1
        -Exterior
            -Piscina
            -Jardim
    */

public struct FloorPlanConfig
{
    public Vector2Int GridDimensions;
    public Dictionary<string, ZoneConfig> ZonesConfigs;
    public Dictionary<string, string[]> Adjacencies;

    public bool IsValid()
    {
        if(GridDimensions.x <= 0 || GridDimensions.y <= 0) return false;
        if(ZonesConfigs == null || ZonesConfigs.Count == 0) return false;
        if(Adjacencies == null || Adjacencies.Count == 0) return false;

        return true;
    }
}

[Serializable]
public struct ZoneConfig
{
    public string ParentZoneId;
    [Range(0.01f, 1)] public float AreaRatio;
    
    [Obsolete]
    [SerializeField] private string _presetArea;

    public bool IsValid()
    {
        return ParentZoneId != string.Empty;
    }

    [Obsolete]
    public int[] PresetArea()
    {
        string[] area = _presetArea.Split(',');
        int[] presetArea = new int[area.Length];

        for(int i = 0; i < area.Length; i++)
        {
            presetArea[i] = int.Parse(area[i]);
        }

        return presetArea;
    }
}

public class FloorPlanManager
{
    private CellsGrid _cellsGrid;
    // RUNTIME DATA
    [Obsolete]private List<Zone> _rootZones;// TODO Não sei qual melhor opção para a raiz, mas ter apenas 1 root, correspondente a area total parece ser uma opção melhor.
                                            // <!--
                                            // order:-20
                                            // -->
    private Zone _rootZone;
    private Dictionary<string, Zone> _zonesInstances;
    private bool _initialized = false;


    public CellsGrid CellsGrid => _cellsGrid;
    [Obsolete]public List<Zone> RootZones => _rootZones;
    /// <summary>
    /// The util floor plan zone, grid's cells outside this zone will not be used by the algorith.
    /// </summary>
    public Zone RootZone => _rootZone;
    public Dictionary<string, Zone> ZonesInstances => _zonesInstances;


    public bool Init(FloorPlanConfig floorPlanConfig)
    {
        Debug.Log("Initializing floor plan manager.");

        if(!floorPlanConfig.IsValid())
        {
            Debug.LogError("Invalid general floor plan config.");
            return false;
        }
        

        _rootZones = new List<Zone>(); // a list of all the first zones of the hierarchy.
        _zonesInstances = new Dictionary<string, Zone>(); // a list/dictionary of all the zones instances, identified by the zone id.

        _cellsGrid = new CellsGrid(floorPlanConfig.GridDimensions);

        CreateZonesHierarchy(floorPlanConfig.ZonesConfigs, floorPlanConfig.Adjacencies);

        _initialized = true;

        return _initialized;
    }


    void CreateZonesHierarchy(Dictionary<string, ZoneConfig> zonesConfigs, Dictionary<string, string[]> adjacencies)
    {
        _zonesInstances = new Dictionary<string, Zone>();

        // Create all zones.
        foreach(var zone in zonesConfigs)
        {
            _zonesInstances.Add(zone.Key, new Zone(zone.Key, zone.Value.AreaRatio));
        }

        // Set the parents and children of the zones.
        foreach(var zone in _zonesInstances)
        {
            string parentZoneId = zonesConfigs[zone.Key].ParentZoneId;

            if (parentZoneId != string.Empty)
            {
                Zone parentZone = _zonesInstances[parentZoneId];
                zone.Value.SetParentZone(parentZone);
                parentZone.AddChildZone(zone.Value);
            }
        }

        // For each zone with adjacencies configured, set the adjacent zones.
        foreach(var zoneId in adjacencies)
        {
            foreach(var adjacentZoneId in zoneId.Value)
            {
                _zonesInstances[zoneId.Key].AddAdjacentZone(_zonesInstances[adjacentZoneId]);
                _zonesInstances[adjacentZoneId].AddAdjacentZone(_zonesInstances[zoneId.Key]);
            }
        }

        // TODO Assuming the can start with multple zones, skipping the first zone that shold be the full terrain.
        // <!--
        // order:-30
        // -->
        foreach(var zone in _zonesInstances)
        {
            // If is a root.
            if(zone.Value.ParentZone == null)
            {
                _rootZones.Add(zone.Value);

                if(_rootZone == null)
                {
                    _rootZone = zone.Value;
                }
                else
                {
                    Debug.LogError("Only one root zone is allowed. Add a parent to all zones except the root.");
                }
            }
        }
    }
}

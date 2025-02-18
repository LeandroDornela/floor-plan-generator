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

    public bool IsValid()
    {
        return ParentZoneId != string.Empty;
    }
}

public class FloorPlanManager
{
    private CellsGrid _cellsGrid;
    // RUNTIME DATA
    private List<Zone> _rootZones;// TODO: Não sei qual melhor opção para a raiz, mas ter apenas 1 root, correspondente a area total parece ser uma opção melhor.
    private Dictionary<string, Zone> _zonesInstances;
    private bool _initialized = false;


    public CellsGrid CellsGrid => _cellsGrid;
    public List<Zone> RootZones => _rootZones;
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
            _zonesInstances.Add(zone.Key, new Zone(zone.Key));
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

        // TODO: Assuming the can start with multple zones, skipping the first zone that shold be the full terrain.
        foreach(var zone in _zonesInstances)
        {
            // If is a root.
            if(zone.Value._parentZone == null)
            {
                _rootZones.Add(zone.Value);
            }
        }
    }
}

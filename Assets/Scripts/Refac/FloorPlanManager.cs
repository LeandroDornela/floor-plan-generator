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

//[System.Serializable]
public class FloorPlanManager
{
    // store the grid
    // store the hierarchy
    // store zones/rooms list

    // create the data structures from config

    // trigger data ready to be read by other classes, like visual debug

    // methods for grid manipulation

    // TODO: possibilitar a entrada de dados de outra forma. Ex gerados em outra classe.
    [SerializeField] private GeneratorConfig _generatorConfig;
    [SerializeField] private ZoneHierarchyConfig _hierarchyConfig;

    private CellsGrid _cellsGrid;

    // TODO: Não sei qual melhor opção para a raiz, mas ter apenas 1 root, correspondente a area total
    // parece ser uma opção melhor.
    private List<Zone> _rootZones;
    private Dictionary<string, Zone> _zonesInstances;

    private bool _initialized = false;


    public void Init()
    {
        Debug.Log("Initializing floor plan manager.");

        if(_generatorConfig == null)
        {
            Debug.LogError("Generator config not set.");
            return;
        }

        if(_hierarchyConfig == null)
        {
            Debug.LogError("Hierarchy config not set.");
            return;
        }

        _rootZones = new List<Zone>();
        _zonesInstances = new Dictionary<string, Zone>();

        _cellsGrid = new CellsGrid(_generatorConfig.GridDimensions);

        CreateZonesHierarchy(_hierarchyConfig);

        _initialized = true;
    }


    void CreateZonesHierarchy(ZoneHierarchyConfig config)
    {
        _zonesInstances = new Dictionary<string, Zone>();

        // Create all zones.
        foreach(var zone in config.ZonesConfigs)
        {
            _zonesInstances.Add(zone.Key, new Zone(zone.Key, zone.Value.Color));
        }

        // Set the parents and children of the zones.
        foreach(var zone in _zonesInstances)
        {
            string parentZoneId = config.ZonesConfigs[zone.Key].ParentZoneId;

            if (parentZoneId != string.Empty)
            {
                Zone parentZone = _zonesInstances[parentZoneId];
                zone.Value.SetParentZone(parentZone);
                parentZone.AddChildZone(zone.Value);
            }
        }

        // For each zone with adjacencies configured, set the adjacent zones.
        foreach(var zoneId in config.Adjacencies)
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


    // EXEMPLO
    //public void GenerateFloorPlan(ZoneConfig[] zoneConfigs, string[][] adjacencies, initialGrid){}
}

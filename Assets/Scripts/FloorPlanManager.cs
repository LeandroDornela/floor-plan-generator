using System;
using System.Collections.Generic;
using UnityEngine;

/*
    Estrutura hierarquica atual.

    -Root
        -Exterior
        -Interior
            -Publico
                -Sala
                -Cozinha
            -Privado
                -Quarto
                -Banheiro
*/


/// <summary>
/// All floor plan grid changes must pass trough this class.
/// </summary>
public class FloorPlanManager
{
    private CellsGrid _cellsGrid;
    [Obsolete]private List<Zone> _rootZones;// TODO Não sei qual melhor opção para a raiz, mas ter apenas 1 root, correspondente a area total parece ser uma opção melhor
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


    public bool Init(FloorPlanData floorPlanConfig)
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

        CreateZonesHierarchy(floorPlanConfig.ZonesConfigs, floorPlanConfig.Adjacencies, _cellsGrid);

        _initialized = true;

        return _initialized;
    }


    void CreateZonesHierarchy(Dictionary<string, ZoneData> zonesConfigs, Dictionary<string, string[]> adjacencies, CellsGrid cellsGrid)
    {
        _zonesInstances = new Dictionary<string, Zone>();

        // Create all zones.
        foreach(var zone in zonesConfigs)
        {
            _zonesInstances.Add(zone.Key, new Zone(this, zone.Key, zone.Value.AreaRatio));
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


        // After all zones created and parents assigned, check for preset area.
        foreach(var zone in _zonesInstances)
        {
            CheckAndAssignPresetArea(zone.Value, zonesConfigs[zone.Key], cellsGrid);
        }
    }


    void CheckAndAssignPresetArea(Zone zone, ZoneData zoneData, CellsGrid cellsGrid)
    {
        if(zone == null || cellsGrid == null)
        {
            Debug.LogError("Zone or grid unassigned.");
        }


        if(zoneData.HasPresetArea)
        {
            // Validate preset area size.
            if(zoneData.PresetArea.Length != cellsGrid.Cells.Length)
            {
                Debug.LogError("Preset area number of cells don't match the grid number of cells.");
                return;
            }

            // Assign the correct cell grid cells to the zone.
            for(int y = 0; y < cellsGrid.Dimensions.y; y++)
            {
                for(int x = 0; x < cellsGrid.Dimensions.x; x++)
                {
                    if(zoneData.PresetArea[Utils.MatrixToArrayIndex(x, y, cellsGrid.Dimensions.x)] == 1)
                    {
                        AssignCellToZone(x, y, zone);
                    }
                }
            }

            zone.Bake();
        }
        else // If zone don't have a preset area and is the root. Assign all cells by default.
        {
            // Validate root zone.
            if(_rootZone == null)
            {
                Debug.LogError("Root zone undefined.");
                return;
            }

            // Check if is root and assign cells.
            if(zone == _rootZone)
            {
                foreach(Cell cell in cellsGrid.Cells)
                {
                    AssignCellToZone(cell, zone);
                }

                zone.Bake();
            }
        }
    }


    public bool AssignCellToZone(int x, int y, Zone zone)
    {
        if(zone == null)
        {
            Debug.LogError("Invalid zone.");
            return false;
        }

        if(_cellsGrid.GetCell(x, y, out Cell cell))
        {
            // Remove previous set cell zone.
            if(cell.Zone != null)
            {
                cell.Zone.RemoveCell(cell);
            }

            zone.AddCell(cell);
            cell.SetZone(zone);

            return true;
        }
        else
        {
            return false;
        }
    }


    public bool AssignCellToZone(Cell cell, Zone zone)
    {
        if(cell == null || zone == null)
        {
            Debug.LogError("Invalid cell or zone.");
            return false;
        }

        // Remove previous set cell zone.
        if(cell.Zone != null)
        {
            cell.Zone.RemoveCell(cell);
        }

        zone.AddCell(cell);
        cell.SetZone(zone);

        return true;
    }
}

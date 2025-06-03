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

namespace BuildingGenerator
{
/// <summary>
/// All floor plan grid changes must pass trough this class.
/// </summary>
public class FloorPlanManager
{
    private string _floorPlanId;
    private CellsGrid _cellsGrid;
    
    private Zone _rootZone;
    private Dictionary<string, Zone> _zonesInstances;
    private bool _initialized = false;

    //Storing adjacencies to facilitate the adj. checking without redundance.
    private Dictionary<string, string[]> _adjacencies;


    public string FloorPlanId => _floorPlanId;
    public CellsGrid CellsGrid => _cellsGrid;
    /// <summary>
    /// The util floor plan zone, grid's cells outside this zone will not be used by the algorith.
    /// </summary>
    public Zone RootZone => _rootZone;
    public Dictionary<string, Zone> ZonesInstances => _zonesInstances;
    public Dictionary<string, string[]> AdjacencyRules => _adjacencies;


    public FloorPlanManager(FloorPlanData floorPlanConfig)
    {
        Init(floorPlanConfig);
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="floorPlanConfig"></param>
    /// <returns></returns>
    bool Init(FloorPlanData floorPlanConfig)
    {
        //Debug.Log("Initializing floor plan manager.");

        if(!floorPlanConfig.IsValid())
        {
            Debug.LogError("Invalid general floor plan config.");
            return false;
        }
        
        _floorPlanId = floorPlanConfig.FloorPlanId;

        _zonesInstances = new Dictionary<string, Zone>(); // a list/dictionary of all the zones instances, identified by the zone id.

        _cellsGrid = new CellsGrid(floorPlanConfig.GridDimensions);

        _adjacencies = floorPlanConfig.Adjacencies;

        CreateZonesHierarchy(floorPlanConfig.ZonesConfigs, floorPlanConfig.Adjacencies, _cellsGrid);

        _initialized = true;

        return _initialized;
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="zonesConfigs"></param>
    /// <param name="adjacencies"></param>
    /// <param name="cellsGrid"></param>
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


    /// <summary>
    /// 
    /// </summary>
    /// <param name="zone"></param>
    /// <param name="zoneData"></param>
    /// <param name="cellsGrid"></param>
    void CheckAndAssignPresetArea(Zone zone, ZoneData zoneData, CellsGrid cellsGrid)
    {
        if(zone == null || cellsGrid == null)
        {
            Debug.LogError("Zone or grid unassigned.");
            return;
        }


        if(zoneData.HasPresetArea)
        {
            // Check if the zone can be predefined.
            // This validation can be done before, checking the in data.
            // To simplify and avoid problems, only the root zone or his children can be predefined.
            // The children predefined area must be inside the root.
            // Why? Think on the scenario, the area A with no predefined area have a child B with a predefined area
            // at the moment of grow area A it must expand to include the cells set to B, what is not guaranteed. One
            // way to do it would be recursively assign the cells of B to the parent A and its parents and permit A
            // expansion since expansion is initially blocked to predefined areas.
            if(zone.ParentZone != _rootZone && zone != _rootZone)
            {
                Debug.LogError($"Only the root zone and it's children can be predefined. Zone: {zone.ZoneId}");
                return;
            }

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
                        // Assign the cell to the zone, but only if it is inside the zone's parent. Avoid adding cells
                        // outside the current context.
                        TryAssignCellToZone(x, y, zone);
                    }
                }
            }

            zone.Bake();
        }
        else // ROOT DEFAULT. If zone don't have a preset area and is the root. Assign all cells by default.
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


    /// <summary>
    /// 
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="zone"></param>
    /// <returns></returns>
    public bool AssignCellToZone(int x, int y, Zone zone)
    {
        if(zone == null)
        {
            Debug.LogError("Invalid zone.");
            return false;
        }

        if(_cellsGrid.GetCell(x, y, out Cell cell))
        {
            // TODO: Replace by "return AssignCellToZone(cell, zone);"
            
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


    /// <summary>
    /// 
    /// </summary>
    /// <param name="cell"></param>
    /// <param name="zone"></param>
    /// <returns></returns>
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


    /// <summary>
    /// The same as AssignCellToZone but check if the cell belongs to zone's parent.
    /// </summary>
    /// <param name="cell"></param>
    /// <param name="zone"></param>
    /// <returns></returns>
    public bool TryAssignCellToZone(int x, int y, Zone zone)
    {
        if(_cellsGrid.GetCell(x, y, out Cell cell))
        {
            if(cell.Zone == zone.ParentZone)
            {
                return AssignCellToZone(cell, zone);
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }


    [Obsolete]
    public bool AreAllAdjacenciesMeet()
    {
        foreach(var adjArray in _adjacencies)
        {
            string currentZoneId = adjArray.Key;

            foreach(string adjZoneId in adjArray.Value)
            {
                if(!_zonesInstances[currentZoneId].IsAdjacentTo(_cellsGrid, _zonesInstances[adjZoneId]))
                {
                    Debug.LogWarning($"Adjacency constraint not meetfor zone {currentZoneId} and {adjZoneId}");
                    return false;
                }
            }

            Debug.LogWarning($"All adjacency constraints meet for zone {currentZoneId}");
        }
        
        return true;
    }


    public void PrintFloorPlan()
    {
        string result = string.Empty;

        foreach(var cell in CellsGrid.Cells)
        {
            result += '|';

            if(cell.Zone == null)
            {
                result += "---";
            }
            else
            {
                if(cell.Zone.ZoneId.Length >= 3)
                    result += $"{cell.Zone.ZoneId.Substring(0,3)}";
                else
                    result += $"{cell.Zone.ZoneId}";
            }

            if(cell.GridPosition.x == CellsGrid.Dimensions.x - 1)
            {
                result += '\n';
            }
        }

        Debug.Log(result);
    }
}
}
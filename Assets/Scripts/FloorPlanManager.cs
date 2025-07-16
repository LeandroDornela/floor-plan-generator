using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
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
        private Dictionary<Guid, Zone> _zonesInstances;
        private bool _initialized = false;

        //Storing adjacencies to facilitate the adj. checking without redundance.
        private Dictionary<Guid, Guid[]> _adjacencies;
        private List<CellsTuple> _wallCellsTuples;

        
        public string FloorPlanId => _floorPlanId;
        public CellsGrid CellsGrid => _cellsGrid;
        public List<CellsTuple> WallCellsTuples => _wallCellsTuples;

        
        /// <summary>
        /// The util floor plan zone, grid's cells outside this zone will not be used by the algorith.
        /// </summary>
        public Zone RootZone => _rootZone;
        public Dictionary<Guid, Zone> ZonesInstances => _zonesInstances;
        public Dictionary<Guid, Guid[]> Adjacencies => _adjacencies;

        private Guid _rootGuid;


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
            if(!floorPlanConfig.IsValid())
            {
                Utils.Debug.DevError("Invalid general floor plan config.");
                return false;
            }
            
            _floorPlanId = floorPlanConfig.FloorPlanId;

            _zonesInstances = new Dictionary<Guid, Zone>(); // a list/dictionary of all the zones instances, identified by the zone id.

            _cellsGrid = new CellsGrid(floorPlanConfig.GridDimensions);

            _adjacencies = floorPlanConfig.Adjacencies;

            _wallCellsTuples = new List<CellsTuple>();

            CreateZonesHierarchy(floorPlanConfig.ZonesConfigs, floorPlanConfig.Adjacencies, _cellsGrid);

            _initialized = true;

            return _initialized;
        }


        /// <summary>
        /// TODO: use zone guid instead of id to avoid duplicated, zone.key is guid
        /// </summary>
        /// <param name="zonesConfigs"></param>
        /// <param name="adjacencies"></param>
        /// <param name="cellsGrid"></param>
        void CreateZonesHierarchy(Dictionary<Guid, ZoneData> zonesConfigs, Dictionary<Guid, Guid[]> adjacencies, CellsGrid cellsGrid)
        {
            _zonesInstances = new Dictionary<Guid, Zone>();

            // Create all zones.
            foreach (var zone in zonesConfigs)
            {
                _zonesInstances.Add(zone.Key, new Zone(this, zone.Key, zone.Value.ZoneID, zone.Value.AreaRatio, zone.Value.HasOutsideDoor, zone.Value.HasWindows));
            }

            // Set the parents and children of the zones.
            foreach (var zone in _zonesInstances)
            {
                //string parentZoneId = zonesConfigs[zone.Key].ParentZoneId;
                Guid parentZoneGUID = zonesConfigs[zone.Key].ParentZoneGUID;

                if (parentZoneGUID != Guid.Empty) // Empty parent Guid means it is a root.
                {
                    Zone parentZone = _zonesInstances[parentZoneGUID];
                    zone.Value.SetParentZone(parentZone);
                    parentZone.AddChildZone(zone.Value);
                }
            }

            // For each zone with adjacencies configured, set the adjacent zones.
            foreach (var zoneGUID in adjacencies)
            {
                foreach (var adjacentZoneGUID in zoneGUID.Value)
                {
                    _zonesInstances[zoneGUID.Key].AddAdjacentZone(_zonesInstances[adjacentZoneGUID]);
                    _zonesInstances[adjacentZoneGUID].AddAdjacentZone(_zonesInstances[zoneGUID.Key]);
                }
            }

            // TODO Assuming the can start with multple zones, skipping the first zone that shold be the full terrain.
            // <!--
            // order:-30
            // -->
            foreach (var zone in _zonesInstances)
            {
                // If is a root.
                if (zone.Value.ParentZone == null)
                {
                    if (_rootZone == null)
                    {
                        _rootZone = zone.Value;
                        _rootGuid = zone.Key;
                    }
                    else
                    {
                        Utils.Debug.DevError("Only one root zone is allowed. Add a parent to all zones except the root.");
                    }
                }
            }


            // After all zones created and parents assigned, check for preset area.
            // first assign to root, then assign to the other zones.
            CheckAndAssignPresetArea(_rootZone, zonesConfigs[_rootGuid], cellsGrid);
            foreach (var zone in _zonesInstances)
            {
                if (zone.Key != _rootGuid) CheckAndAssignPresetArea(zone.Value, zonesConfigs[zone.Key], cellsGrid);
            }
            /*
            // Bake the preset zones.
            foreach (Zone zone in ZonesInstances.Values)
            {
                if (zone.IsDirty)
                {
                    Utils.Debug.DevLog($"{zone.ZoneId} is dirty. Re-baking...");
                    zone.Unbake();
                    zone.Bake();
                }
            }
            */
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="zone"></param>
        /// <param name="zoneData"></param>
        /// <param name="cellsGrid"></param>
        void CheckAndAssignPresetArea(Zone zone, ZoneData zoneData, CellsGrid cellsGrid)
        {
            if (zone == null || cellsGrid == null)
            {
                Utils.Debug.DevError("Zone or grid unassigned.");
                return;
            }


            if (zoneData.HasPresetArea)
            {
                // Check if the zone can be predefined.
                // This validation can be done before, checking the in data.
                // To simplify and avoid problems, only the root zone or his children can be predefined.
                // The children predefined area must be inside the root.
                // Why? Think on the scenario, the area A with no predefined area have a child B with a predefined area
                // at the moment of grow area A it must expand to include the cells set to B, what is not guaranteed. One
                // way to do it would be recursively assign the cells of B to the parent A and its parents and permit A
                // expansion since expansion is initially blocked to predefined areas.
                if (zone.ParentZone != _rootZone && zone != _rootZone)
                {
                    Utils.Debug.DevError($"Only the root zone and it's children can be predefined. Zone: {zone.ZoneId}");
                    return;
                }

                // Validate preset area size.
                if (zoneData.PresetArea.Length != cellsGrid.Cells.Length)
                {
                    Utils.Debug.DevError("Preset area number of cells don't match the grid number of cells.");
                    return;
                }

                // Assign the correct cell grid cells to the zone.
                for (int y = 0; y < cellsGrid.Dimensions.y; y++)
                {
                    for (int x = 0; x < cellsGrid.Dimensions.x; x++)
                    {
                        if (zoneData.PresetArea[Utils.MatrixToArrayIndex(x, y, cellsGrid.Dimensions.x)] == 1)
                        {
                            // Assign the cell to the zone, but only if it is inside the zone's parent. Avoid adding cells
                            // outside the current context.
                            // TODO: ERRO, if root and a child are preset but root runs after the child, it will replace the child preset.
                            if (!TryAssignPresetCellToZone(x, y, zone))
                            {
                                Utils.Debug.DevError($"Failed to assign preset cell to zone {zoneData.ZoneID}");
                            }
                        }
                    }
                }

                
                if (zone != _rootZone && _rootZone.IsBaked)
                {
                    _rootZone.Unbake();
                    _rootZone.Bake();
                }

                zone.Bake();
            }
            else // ROOT DEFAULT. If zone don't have a preset area and is the root. Assign all cells by default.
            {
                // Validate root zone.
                if (_rootZone == null)
                {
                    Utils.Debug.DevError("Root zone undefined.");
                    return;
                }

                // Check if is root and assign cells.
                if (zone == _rootZone)
                {
                    foreach (Cell cell in cellsGrid.Cells)
                    {
                        AssignCellToZone(cell, zone);
                    }

                    zone.Bake();
                }
            }
            
            /*
            foreach (Zone z in ZonesInstances.Values)
            {
                if (z.IsDirty)
                {
                    Utils.Debug.DevLog($"{z.ZoneId} is dirty. Re-baking...");
                    z.Unbake();
                    z.Bake();
                }
            }
            */
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
                Utils.Debug.DevError("Invalid zone.");
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
            if(cell == null)
            {
                Utils.Debug.DevError("Invalid cell.");
                return false;
            }

            // Remove previous set cell zone.
            if (cell.Zone != null)
            {
                cell.Zone.RemoveCell(cell);
            }

            if (zone != null)
            {
                zone.AddCell(cell);
            }
            
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

        public bool TryAssignPresetCellToZone(int x, int y, Zone zone)
        {
            if (_cellsGrid.GetCell(x, y, out Cell cell))
            {
                if (cell.Zone != null)
                {
                    Utils.Debug.DevWarning($"Assign preset cell that already belong to a zone. Cell belong to {cell.Zone.ZoneId}");
                }
                
                return AssignCellToZone(cell, zone);
            }
            else
            {
                return false;
            }
        }


        public float DesiredAreaIndex()
        {
            float distancesAreaSum = 0;
            int zonesCount = 0;

            foreach (Zone zone in _zonesInstances.Values)
            {
                if (!zone.IsLeaf)
                {
                    continue;
                }

                zonesCount++;

                distancesAreaSum += Mathf.Clamp(MathF.Abs(zone.Area / zone.DesiredArea), 0, 1);
            }

            return distancesAreaSum / zonesCount;
        }


        public float DesiredAspectIndex()
        {
            float aspDistSum = 0;
            int totalZonesCounter = 0;

            foreach (Zone zone in _zonesInstances.Values)
            {
                if (!zone.IsLeaf)
                {
                    continue;
                }

                totalZonesCounter++;

                if (zone.IsLShaped)
                {
                    // L shaped is maximum aspect distance. 0 points.
                }
                else
                {
                    aspDistSum += MathF.Abs(zone.DesiredAspect - zone.GetZoneAspectOrientIndependent());
                }
            }

            return 1.0f - (aspDistSum / totalZonesCounter);
        }


        /// <summary>
        /// Return the percentage of leaf zones(rooms) that are rectangular.
        /// 1 means all leaf zones are rectangular.
        /// </summary>
        /// <returns></returns>
        public float RectZonesIndex()
        {
            int rectZonesCounter = 0;
            int totalZonesCounter = 0;

            foreach (Zone zone in _zonesInstances.Values)
            {
                // Ignore the zone if its not a leaf,or a "room".
                if (!zone.IsLeaf)
                {
                    continue;
                }

                totalZonesCounter++;

                // TODO: can give wrong results when unassigned cells are given to the zone.
                // Maybe need to add a tag for it.
                // If the zone is rectangular.
                if (!zone.IsLShaped)
                {
                    rectZonesCounter++;
                }
            }

            return (float)rectZonesCounter / totalZonesCounter;
        }


        [Obsolete]
        public int RegularZonesCount()
        {
            int counter = 0;

            foreach (Zone zone in _zonesInstances.Values)
            {
                // Ignore the zone if its not a leaf,or a "room".
                if (!zone.IsLeaf)
                {
                    continue;
                }

                if (!zone.IsLShaped)
                {
                    counter++;
                }
            }

            return counter;
        }

        public float TotalDistanceFromDesiredAreas()
        {
            // Ignore negative values(areas bigger than the desired value) to avoid results where you have too
            // big and too small areas nulling themselves making the result look optimal.

            float totalDistance = 0;

            foreach (Zone zone in _zonesInstances.Values)
            {
                // Ignore the zone if its not a leaf,or a "room".
                if (!zone.IsLeaf)
                {
                    continue;
                }

                float areaDistance = zone.DistanceFromDesiredArea();

                if (areaDistance > 0)
                {
                    totalDistance += areaDistance;
                }
            }

            return totalDistance;
        }


        public void PrintFloorPlan()
        {
            string result = string.Empty;

            foreach (var cell in CellsGrid.Cells)
            {
                result += '|';

                if (cell.Zone == null)
                {
                    result += "---";
                }
                else
                {
                    if (cell.Zone.ZoneId.Length >= 3)
                        result += $"{cell.Zone.ZoneId.Substring(0, 3)}";
                    else
                        result += $"{cell.Zone.ZoneId}";
                }

                if (cell.GridPosition.x == CellsGrid.Dimensions.x - 1)
                {
                    result += '\n';
                }
            }

            Debug.Log(result);
        }
    }
}
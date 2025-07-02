using System;
using System.Collections.Generic;
using UnityEngine;

/*------------------>x
|       Top
V   Left[o][>][>][o]Right
y       [v][-][-][v]
        [v][-][-][v]
        [o][>][>][>]
        Bottom
*/

namespace BuildingGenerator
{
    /// <summary>
    /// 
    /// </summary>
    public class Zone // similar a uma estrutura de nos em arvore
    {
        public enum Side
        {
            Top = 0,
            Bottom = 1,
            Left = 2,
            Right = 3
        }

        // PRIVATE
        private FloorPlanManager _floorPlanManager;


        private string _zoneId;
        private float _areaRatio;
        private float _desiredAspect = 1; // 1 is square
        private Zone _parentZone;
        private Dictionary<string, Zone> _childZones;
        private Dictionary<string, Zone> _adjacentZones;
        private bool _hasOutsideDoor;
        private bool _hasWindows;


        public List<Cell> _cellsList; // Celulas atualmente associadas a zona.
        private Dictionary<Side, CellsLineDescription> _zoneBorders;
        private CellsLineDescription _lBorderCells;
        private Cell[] _cellsArray; // Baked
        private Cell[] _borderCells; // Baked

        private bool _isLShaped = false;
        private bool _isBaked = false;
        private bool _isDirty = false; // When a zone is dirty it needs to be baked again.

        // Coord transformation matrix, used to compact the methods that have different behavior base on the border of the shape.
        private readonly Dictionary<Side, Vector4> _coordTransMatrices = new Dictionary<Side, Vector4>{
    {Side.Top, new Vector4(1,0,0,-1)},
    {Side.Bottom, new Vector4(1,0,0,1)},
    {Side.Left, new Vector4(0,1,-1,0)},
    {Side.Right, new Vector4(0,1,1,0)}};


        // PUBLIC PROP
        public string ZoneId => _zoneId;
        public float AreaRatio => _areaRatio;
        public float DesiredAspect => _desiredAspect;
        public int Area => _cellsList.Count; // Get the zone area in cells units.
        public Cell OriginCell => (_cellsList != null && _cellsList.Count > 0) ? _cellsList[0] : null;
        public Zone ParentZone => _parentZone;
        public Dictionary<string, Zone> ChildZones => _childZones;
        public Dictionary<string, Zone> AdjacentZones => _adjacentZones;
        public bool HasOutsideDoor => _hasOutsideDoor;
        public bool HasWindows => _hasWindows;
        /// <summary>
        /// Warning. When the zone is subdivided it losses all the cells.
        /// TODO: When zone is subdivide, return the cells from child zones.
        /// </summary>
        public Cell[] Cells => _cellsArray;
        public Cell[] BorderCells
        {
            get
            {
                if (_borderCells == null || _borderCells.Length == 0) Utils.ConsoleDebug.DevError("Border Cells is not set.");
                return _borderCells;
            }
        }
        public bool IsLShaped => _isLShaped;
        public bool IsBaked => _isBaked;
        public bool HasChildrenZones => _childZones?.Count > 0;
        public bool IsLeaf => _childZones?.Count == 0;
        public bool IsDirty => _isDirty;


        public Zone(FloorPlanManager floorPlanManager, string zoneId, float areaRatio, bool hasOutsideDoor, bool hasWindows)
        {
            _floorPlanManager = floorPlanManager;
            _zoneId = zoneId;
            _areaRatio = areaRatio;
            _parentZone = null;
            _cellsList = new List<Cell>();
            _childZones = new Dictionary<string, Zone>();
            _adjacentZones = new Dictionary<string, Zone>();
            _hasOutsideDoor = hasOutsideDoor;
            _hasWindows = hasWindows;
        }

        #region =========== CELLS AND FAMILY ZONES SETTING ===========
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cell"></param>
        /// <returns></returns>
        public bool AddCell(Cell cell)
        {
            if (cell == null)
            {
                Utils.ConsoleDebug.DevError("Trying to add a null cell.");
                return false;
            }

            if (_isBaked && !_isDirty)
            {
                Utils.ConsoleDebug.DevWarning("Adding cell to baked Zone.");
                _isDirty = true;
            }

            if (_cellsList.Contains(cell))
            {
                Utils.ConsoleDebug.DevError("Trying to add an exiting cell.");
                return false;
            }

            // Set up the borders.
            if (_cellsList.Count == 0) // First cell
            {
                _zoneBorders = new Dictionary<Side, CellsLineDescription>
            {
                { Side.Top, new CellsLineDescription(cell.GridPosition.x, cell.GridPosition.y, 1, Side.Top) },
                { Side.Bottom, new CellsLineDescription(cell.GridPosition.x, cell.GridPosition.y, 1, Side.Bottom) },
                { Side.Left, new CellsLineDescription(cell.GridPosition.x, cell.GridPosition.y, 1, Side.Left) },
                { Side.Right, new CellsLineDescription(cell.GridPosition.x, cell.GridPosition.y, 1, Side.Right) }
            };
            }

            _cellsList.Add(cell);
            return true;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="cell"></param>
        /// <returns></returns>
        public bool RemoveCell(Cell cell)
        {
            if (cell == null)
            {
                Utils.ConsoleDebug.DevError("Trying to remove a null cell.");
                return false;
            }

            if (_isBaked && !_isDirty)
            {
                Utils.ConsoleDebug.DevWarning($"Removing cell from baked Zone. Zone: {_zoneId}");
                _isDirty = true;
            }

            if (_cellsList.Contains(cell))
            {
                return _cellsList.Remove(cell);
            }
            else
            {
                Utils.ConsoleDebug.DevError("The cell isn't in the zone.");
                return false;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="childZone"></param>
        /// <returns></returns>
        public bool AddChildZone(Zone childZone)
        {
            if (childZone == null)
            {
                Utils.ConsoleDebug.DevError("Trying to add a null child.");
                return false;
            }

            return _childZones.TryAdd(childZone.ZoneId, childZone);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="adjacentZone"></param>
        /// <returns></returns>
        public bool AddAdjacentZone(Zone adjacentZone)
        {
            if (adjacentZone == null)
            {
                Utils.ConsoleDebug.DevError("Trying to add a null adjacent zone.");
                return false;
            }

            return _adjacentZones.TryAdd(adjacentZone.ZoneId, adjacentZone);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="parentZone"></param>
        /// <returns></returns>
        public bool SetParentZone(Zone parentZone)
        {
            if (_parentZone == null)
            {
                _parentZone = parentZone;
                return true;
            }
            else
            {
                Utils.ConsoleDebug.DevError("Parent zone can't be overridden.");
                return false;
            }
        }

        #endregion


        #region =========== GETS/CHECKS ===========

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cellsGrid"></param>
        /// <returns></returns>
        public bool HasDesiredArea()
        {
            return Area >= _areaRatio * _floorPlanManager.CellsGrid.Area;
        }


        public float DistanceFromDesiredArea()
        {
            float desiredArea = _areaRatio * _floorPlanManager.CellsGrid.Area;
            return desiredArea - Area;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="zoneToTest"></param>
        /// <returns></returns>
        public bool MustBeAdjacentTo(Zone zoneToTest)
        {
            return _adjacentZones.ContainsKey(zoneToTest.ZoneId);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="zoneToTest"></param>
        /// <returns></returns>
        public bool IsSister(Zone zoneToTest)
        {
            return _parentZone == zoneToTest.ParentZone;
        }


        /// <summary>
        /// (top cells) / (left cells);
        /// (if smaller 1): zone is taller than wide;
        /// (if larger 1): zone is wider than tall;
        /// (if equal 1): zone is square;
        /// Will not work well in 'L' shaped zones.
        /// </summary>
        /// <returns>Current zone aspect ratio.</returns>
        public float GetZoneAspect()
        {
            return (float)_zoneBorders[Side.Top].NumberOfCells / _zoneBorders[Side.Left].NumberOfCells;
        }

        #endregion


        #region =========== GENERA SETS/STATE CHANGE

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lBorder"></param>
        /// <returns></returns>
        public bool SetAsLShaped(CellsLineDescription lBorder)
        {
            if (lBorder == null)
            {
                Utils.ConsoleDebug.DevError("L border can't be null");
                return false;
            }

            if (_isLShaped)
            {
                Utils.ConsoleDebug.DevError("Zone is already L shaped.");
                return false;
            }

            _lBorderCells = lBorder;
            _isLShaped = true;

            return true;
        }

        #endregion


        #region =========== BAKING ===========

        /// <summary>
        /// When zone has the final shape, convert lists to arrays, update and set values.
        /// </summary>
        /// <param name="cellsGrid"></param>
        public void Bake()
        {
            if (_isBaked)
            {
                Utils.ConsoleDebug.DevError("Zone is already baked. If need to bake again use Unbake first.");
                return;
            }

            _cellsArray = _cellsList.ToArray();

            SetBorderCells();

            _isBaked = true;
            _isDirty = false;

            Utils.ConsoleDebug.DevLog($"{_zoneId} baked.");
        }

        /// <summary>
        /// Unbake the zone before modify the cells.
        /// </summary>
        public void Unbake()
        {
            _isBaked = false;
            Array.Clear(_cellsArray, 0, _cellsArray.Length);
            _cellsArray = null;
            Array.Clear(_borderCells, 0, _borderCells.Length);
            _borderCells = null;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="cellsGrid"></param>
        void SetBorderCells()
        {
            if (_borderCells != null && _borderCells.Length > 0)
            {
                Utils.ConsoleDebug.DevError("Border cells is already defined.");
                return;
            }

            // NOTE: Why find the cells instead of using the sides to get the border?
            // Cause whe L grow the side stop to represent the borders precisely.
            _borderCells = FindBorderCells(_floorPlanManager.CellsGrid);
        }


        /// <summary>
        /// Expensive. Cache the return when possible.
        /// "Find" to make clear it will look for the border cells.
        /// TODO: Optimize.
        /// </summary>
        /// <returns></returns>
        Cell[] FindBorderCells(CellsGrid cellsGrid)
        {
            if (_cellsArray == null || _cellsArray.Length == 0)
            {
                return default;
            }

            List<Cell> borderCells = new List<Cell>();

            foreach (Cell cell in _cellsArray)
            {
                Cell neighborCell;

                bool cellAdded = false;

                // Check the neighbors
                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        if (x == 0 && y == 0) continue;

                        if (!cellsGrid.GetCell(cell.GridPosition.x + x, cell.GridPosition.y + y, out neighborCell) ||
                          (neighborCell?.Zone != this && neighborCell?.Zone?._parentZone != this))
                        {
                            cell.SetIsBorderCell(true);
                            borderCells.Add(cell);
                            cellAdded = true;
                            break;
                        }
                    }

                    if (cellAdded) break; // Break the iteration to void adding the same cell multiple times.
                }
            }

            return borderCells.ToArray();
        }

        #endregion


        #region ================================================== PUBLIC EXPANSION METHODS ==================================================
        /// <summary>
        /// Total space = distance x freeLineDescription.numberOfCells
        /// </summary>
        /// <param name="side"></param>
        /// <param name="cellsGrid"></param>
        /// <param name="fullSpaceSearch"></param>
        /// <returns></returns>
        public (CellsLineDescription freeLineDescription, bool isFullLine, int distance) GetExpansionSpaceRect(Side side, bool fullSpaceSearch) // TODO: talvez mudar apra "get RECTangular exp space"
        {
            if (_isLShaped)
            {
                Utils.ConsoleDebug.DevError("Don't use when L-shaped.");
                return default;
            }

            return GetExpansionSpace(_zoneBorders[side], _floorPlanManager.CellsGrid, fullSpaceSearch);
        }


        // the return can be a possible L shape border.
        public (CellsLineDescription freeLineDescription, bool isFullLine, int distance) GetLargestExpansionSpaceRect(bool fullSpaceSearch)
        {
            if (_isLShaped)
            {
                Utils.ConsoleDebug.DevError("Don't use when L-shaped.");
                return default;
            }

            (CellsLineDescription line, bool isFullLine, int distance) largestFreeSide = (null, false, 0);

            foreach (var border in _zoneBorders)
            {
                var newSide = GetExpansionSpace(border.Value, _floorPlanManager.CellsGrid, fullSpaceSearch);

                if (newSide.freeLineDescription != null)
                {
                    if (largestFreeSide.line == null)
                    {
                        largestFreeSide = newSide;
                    }
                    else
                    {
                        int newSideTotalSpace = newSide.freeLineDescription.NumberOfCells * newSide.distance;
                        int largestFreeSideTotalSpace = largestFreeSide.line.NumberOfCells * largestFreeSide.distance;

                        if (newSideTotalSpace > largestFreeSideTotalSpace)
                        {
                            largestFreeSide = newSide;
                        }
                        else if (newSideTotalSpace == largestFreeSideTotalSpace && Utils.Random.RandomBool())
                        {
                            largestFreeSide = newSide;
                        }
                        // else, is smaller, do nothing.
                    }
                }
            }

            return largestFreeSide;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="side"></param>
        /// <param name="cellsGrid"></param>
        /// <returns></returns>
        public bool TryExpandShapeRect(Side side)
        {
            if (_isLShaped)
            {
                Utils.ConsoleDebug.DevError("Trying to use rectangular expansion in L-Shaped zone.");
                return false;
            }

            return TryExpand(_zoneBorders[side]);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="cellsGrid"></param>
        /// <param name="checkSpace"></param>
        /// <returns></returns>
        public bool TryExpandShapeL(bool checkSpace)
        {
            if (checkSpace)
            {
                var expSpace = GetExpansionSpace(_lBorderCells, _floorPlanManager.CellsGrid, false);

                if (expSpace.isFullLine)
                {
                    return TryExpand(_lBorderCells);
                }
                else return false;
            }
            else
            {
                return TryExpand(_lBorderCells);
            }
        }

        #endregion ==================================================



        // TODO: talvez checar expansão simples nos 2 sentidos prieiro e depois checar a profundidade
        // TODO: ao inves de ter retornos em varios pontos, guardar resulados de cada avaliação para comparar no final
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cellsLineDesc"></param>
        /// <param name="cellsGrid"></param>
        /// <param name="fullSpaceSearch"></param>
        /// <returns></returns>
        (CellsLineDescription freeLineDescription, bool isFullLine, int distance) GetExpansionSpace(CellsLineDescription cellsLineDesc, CellsGrid cellsGrid, bool fullSpaceSearch)
        {
            //[mx][mz] * [i]
            //[my][mw]   [1]
            // x = i * mx + 1 * mz
            // y = I * my + 1 * mw
            // (x: direction of cells in X,
            //  y: direction of cells in Y,
            //  z: direction of expansion in X,
            //  w: direction of expansion in Y)

            if (cellsLineDesc.NumberOfCells == 0)
            {
                // [-][-][-][-][-] free line
                // [o][o][o][o][o] original line
                return (null, false, 0);
            }

            int firstToLast_Count = 0; // from first to last cell in line.
            int lastToFirst_Count = 0; // from last to first cell in line.
            int maxExpansionSpace = 0;
            // The return free line is to be used only as container of information about the line to be expanded, don't refer to real borders to avoid external modification.
            CellsLineDescription freeLine = new CellsLineDescription(cellsLineDesc.FirstCellCoord.x, cellsLineDesc.FirstCellCoord.y, 0, cellsLineDesc.Side);
            Vector4 tMatrix = _coordTransMatrices[cellsLineDesc.Side]; // Transformation matrix


            // >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>> Check from FIRST to LAST cell of the line 

            // =============== Check the first line on the side of orin line
            // Do the first iteration from the first to the last cell on the side/border.
            // Return immediately a failure result if find a invalid cell(a cell out of the grid).
            // Or just stop iteration when find a cell that is not available to change zone, since
            // it MUST be a continuos line.
            for (int i = 0; i < cellsLineDesc.NumberOfCells; i++)
            {
                if (cellsGrid.GetCell(cellsLineDesc.FirstCellCoord.x + i * (int)tMatrix.x + (int)tMatrix.z,
                                     cellsLineDesc.FirstCellCoord.y + i * (int)tMatrix.y + (int)tMatrix.w,
                                     out Cell cell))
                {
                    if (CellIsAvailable(cell))
                    {
                        firstToLast_Count++;
                    }
                    else // First invalid cell, keep a continuos expansion line.
                    {
                        break;
                    }
                }
                else // Invalid grid pos.
                {
                    // [-][-][-][-][-] free line
                    // [o][o][o][o][o] original line
                    return (null, false, 0);
                }
            }

            // =============== Check partial result
            // After checking the from the first cell, check if have any free valid cell the
            // expand and update the possible return values with it.
            if (firstToLast_Count > 0)
            {
                // Have space to expand on the direction.
                freeLine.NumberOfCells = firstToLast_Count;
                maxExpansionSpace = 1; // At least one line free.
            }

            // =============== Check the all other valid lines on the sequence if requested.
            // Iterate on the lines "on top" of the free line to check how many of the same size are free to expansion.
            if (fullSpaceSearch && freeLine.NumberOfCells > 0)
            {
                int currentLineNumCells;
                int safeCounter = 0; // Redundance to avoid infinity loops.
                int maxIterations = cellsGrid.LargestDimension;
                bool done = false;

                while (!done && safeCounter < maxIterations)
                {
                    currentLineNumCells = 0;

                    for (int i = 0; i < freeLine.NumberOfCells; i++)
                    {
                        if (cellsGrid.GetCell(cellsLineDesc.FirstCellCoord.x + i * (int)tMatrix.x + (maxExpansionSpace + 1) * (int)tMatrix.z,
                                             cellsLineDesc.FirstCellCoord.y + i * (int)tMatrix.y + (maxExpansionSpace + 1) * (int)tMatrix.w,
                                             out Cell cell))
                        {
                            if (CellIsAvailable(cell))
                            {
                                currentLineNumCells++;
                            }
                            else // First invalid cell of current line.
                            {
                                break;
                            }
                        }
                        else // Invalid grid pos. Sure to not having more cells on this direction.
                        {
                            done = true;
                            break; // Break the for loop.
                        }
                    }

                    if (done) break; // Break the while loop.

                    if (currentLineNumCells == freeLine.NumberOfCells)
                    {
                        maxExpansionSpace++;
                    }
                    else
                    {
                        break;
                    }

                    safeCounter++;
                }
            }

            // =============== Check partial result, possible return.
            // Is a full line so the other direction will not be larger. Return the result.
            // If not, check if the other side is larger.
            if (firstToLast_Count == cellsLineDesc.NumberOfCells)
            {
                // [o][o][o][o][o] free line
                // [o][o][o][o][o] original line
                return (freeLine, true, maxExpansionSpace);
                // return (cellsLineDesc, true, maxExpansionSpace); retorna a linha original completa, talvez n seja uma boa expor ela
            }


            // <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<< Check from LAST to FIRST cell of the line 

            // =============== Check the first line on the side of orin line
            // Do the first iteration from the LAST to FIRST cell on the side/border.
            // Return immediately a failure result if find a invalid cell(a cell out of the grid).
            // Or just stop iteration when find a cell that is not available to change zone, since
            // it MUST be a continuos line.
            for (int i = cellsLineDesc.NumberOfCells - 1; i >= 0; i--)
            {
                if (cellsGrid.GetCell(cellsLineDesc.FirstCellCoord.x + i * (int)tMatrix.x + (int)tMatrix.z,
                                     cellsLineDesc.FirstCellCoord.y + i * (int)tMatrix.y + (int)tMatrix.w,
                                     out Cell cell))
                {
                    if (CellIsAvailable(cell))
                    {
                        lastToFirst_Count++;
                    }
                    else // First invalid cell.
                    {
                        break;
                    }
                }
                else // Invalid grid pos.
                {
                    // OBS: Probably never will reach this point.
                    // [-][-][-][-][-] free line
                    // [o][o][o][o][o] original line
                    return (null, false, 0);
                }
            }

            // =============== Check partial result, possible return result.
            // If the free section on this direction is smaller than the first, or if is equal randomize if will send it.
            // return the first.
            // TODO: when equal and deciding at this point will skip the deep check, making possible a higher space not been check.
            if (lastToFirst_Count < firstToLast_Count || (lastToFirst_Count == firstToLast_Count && Utils.Random.RandomBool())) // lastToFirst_Count == 0 CAN PASS IT, checking bellow.
            {
                // [o][o][-][-][-] free line
                // [o][o][o][o][o] original line
                return (freeLine, false, maxExpansionSpace);
            }

            // =============== Check partial result
            // lastToFirst_Count is bigger than firstToLast_Count and bigger than 0 continue. if not there is no free cells.
            // Since lastToFirst_Count at this point is >= to firstToLast_Count.
            if (lastToFirst_Count == 0)
            {
                // [-][-][-][-][-] free line
                // [o][o][o][o][o] original line
                return (null, false, 0);
            }

            freeLine.NumberOfCells = lastToFirst_Count;
            maxExpansionSpace = 1;

            // =============== Check the all other valid lines on the sequence if requested.
            // Iterate on the lines "on top" of the free line to check how many of the same size are free to expansion.
            if (fullSpaceSearch && freeLine.NumberOfCells > 0)
            {
                int currentLineNumCells;
                int safeCounter = 0; // Redundance to avoid infinity loops.
                int maxIterations = cellsGrid.LargestDimension;
                bool done = false;

                while (!done && safeCounter < maxIterations)
                {
                    currentLineNumCells = 0;

                    for (int i = cellsLineDesc.NumberOfCells - 1; i >= cellsLineDesc.NumberOfCells - freeLine.NumberOfCells; i--)
                    {
                        if (cellsGrid.GetCell(cellsLineDesc.FirstCellCoord.x + i * (int)tMatrix.x + (maxExpansionSpace + 1) * (int)tMatrix.z,
                                             cellsLineDesc.FirstCellCoord.y + i * (int)tMatrix.y + (maxExpansionSpace + 1) * (int)tMatrix.w,
                                             out Cell cell))
                        {
                            if (CellIsAvailable(cell))
                            {
                                currentLineNumCells++;
                            }
                            else // First invalid cell, can try next line on next iteration.
                            {
                                break;
                            }
                        }
                        else // Invalid grid pos. Sure to not having more cells on this direction.
                        {
                            done = true;
                            break;
                        }
                    }

                    if (done) break;

                    if (currentLineNumCells == freeLine.NumberOfCells)
                    {
                        maxExpansionSpace++;
                    }
                    else
                    {
                        break;
                    }

                    safeCounter++;
                }
            }

            // After doing all possible checks, return the largest free line. At this point
            // it will be a line smaller than original line and stating from the last cell.
            // Update the first cell since at this point it still the first from the original nile, and now should be a cell in the middle.
            if (freeLine.Side == Side.Top || freeLine.Side == Side.Bottom)
            {
                freeLine.FirstCellCoord = new Vector2Int(cellsLineDesc.FirstCellCoord.x + cellsLineDesc.NumberOfCells - freeLine.NumberOfCells,
                                                         freeLine.FirstCellCoord.y);
            }
            else
            {
                freeLine.FirstCellCoord = new Vector2Int(freeLine.FirstCellCoord.x,
                                                         cellsLineDesc.FirstCellCoord.y + cellsLineDesc.NumberOfCells - freeLine.NumberOfCells);
            }
            // [-][-][-][o][o] free line
            // [o][o][o][o][o] original line
            return (freeLine, false, maxExpansionSpace);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="cellsLineDesc"></param>
        /// <param name="cellsGrid"></param>
        /// <returns></returns>
        bool TryExpand(CellsLineDescription cellsLineDesc)
        {
            if (_isBaked)
            {
                // Baked zones can't be expanded.
                return false;
            }

            int amount = 1;
            Vector4 tMatrix = _coordTransMatrices[cellsLineDesc.Side]; // Transformation matrix

            // Assign the cells to the zone.
            for (int i = 0; i < cellsLineDesc.NumberOfCells; i++)
            {
                int x = cellsLineDesc.FirstCellCoord.x + i * (int)tMatrix.x + (int)tMatrix.z;
                int y = cellsLineDesc.FirstCellCoord.y + i * (int)tMatrix.y + (int)tMatrix.w;

                if (_floorPlanManager.CellsGrid.GetCell(x, y, out Cell cell))
                {
                    if (!CellIsAvailable(cell))
                    {
                        Utils.ConsoleDebug.DevWarning($"Cell already have a zone in same hierarchy level. Cell zone: {cell.Zone}");
                    }
                }
                else
                {
                    Utils.ConsoleDebug.DevError($"Trying to assign a cell in a invalid Grid position.({x},{y})");
                    return false;
                }

                if (!_floorPlanManager.AssignCellToZone(cell, this))
                {
                    return false;
                }
            }


            switch (cellsLineDesc.Side)
            {
                case Side.Top:
                    if (_isLShaped) _lBorderCells.MoveUp(amount);
                    _zoneBorders[Side.Top].MoveUp(amount);
                    _zoneBorders[Side.Left].MoveUp(amount);
                    _zoneBorders[Side.Right].MoveUp(amount);
                    _zoneBorders[Side.Left].AddCells(amount);
                    _zoneBorders[Side.Right].AddCells(amount);
                    break;
                case Side.Bottom:
                    if (_isLShaped) _lBorderCells.MoveDown(amount);
                    _zoneBorders[Side.Bottom].MoveDown(amount);
                    _zoneBorders[Side.Left].AddCells(amount);
                    _zoneBorders[Side.Right].AddCells(amount);
                    break;
                case Side.Left:
                    if (_isLShaped) _lBorderCells.MoveLeft(amount);
                    _zoneBorders[Side.Left].MoveLeft(amount);
                    _zoneBorders[Side.Top].MoveLeft(amount);
                    _zoneBorders[Side.Bottom].MoveLeft(amount);
                    _zoneBorders[Side.Top].AddCells(amount);
                    _zoneBorders[Side.Bottom].AddCells(amount);
                    break;
                case Side.Right:
                    if (_isLShaped) _lBorderCells.MoveRight(amount);
                    _zoneBorders[Side.Right].MoveRight(amount);
                    _zoneBorders[Side.Top].AddCells(amount);
                    _zoneBorders[Side.Bottom].AddCells(amount);
                    break;
                default: // Top
                    if (_isLShaped) _lBorderCells.MoveUp(amount);
                    _zoneBorders[Side.Top].MoveUp(amount);
                    _zoneBorders[Side.Left].MoveUp(amount);
                    _zoneBorders[Side.Right].MoveUp(amount);
                    _zoneBorders[Side.Left].AddCells(amount);
                    _zoneBorders[Side.Right].AddCells(amount);
                    break;
            }

            return true;
        }


        #region ================================================== AUX FUNCS ==================================================

        bool CellIsAvailable(Cell cell)
        {
            return cell.Zone == _parentZone;
        }

        #endregion

        #region ================================================== DEBUG ==================================================
        public void Debug_Print_GetExpansionSpace_ForAllSides(CellsGrid cellsGrid)
        {
            Debug.Log($"{ZoneId}--------------------------------");
            foreach (var border in _zoneBorders)
            {
                var result = GetExpansionSpace(border.Value, cellsGrid, true);
                Debug.Log($"zid:{ZoneId} side:{result.freeLineDescription?.Side} space:{result.distance} cells:{result.freeLineDescription?.NumberOfCells}");
            }
            Debug.Log("--------------------------------");
        }

        public bool CheckZoneCellsConsistency()
        {
            Utils.ConsoleDebug.DevLog($"{ZoneId}: [BakedCells:{_cellsArray.Length}] / [NonBaked: {_cellsList.Count}]. Border cells count:{_borderCells?.Length}");
            return _cellsArray.Length == _cellsList.Count;
        }
        #endregion
    }
}
using System.Collections.Generic;

namespace BuildingGenerator
{
    public partial class MethodGrowth
    {
        /// <summary>
        /// Only for leaf zones. To support divisible zones check need to change the condition to unassigned cell.
        /// </summary>
        /// <param name="floorPlanManager"></param>
        /// <returns></returns>
        bool AssignMissingCells(FloorPlanManager floorPlanManager)
        {
            CellsGrid grid = floorPlanManager.CellsGrid;
            bool[,] visited = new bool[grid.Dimensions.x, grid.Dimensions.y];
            List<List<Cell>> cellsChunks = new List<List<Cell>>();
            int unassignedCellCount = 0;

            // Create the free cells chunks.
            foreach (var cell in grid.Cells)
            {
                if (!visited[cell.GridPosition.x, cell.GridPosition.y])
                {
                    // FLOOD FILL
                    if (cell.Zone != null && !cell.IsAssignedToLeafZone) // Check if the cell is unassigned.
                    {
                        Stack<Cell> cellsToCheck = new Stack<Cell>();
                        List<Cell> newCellsChunk = new List<Cell>();

                        unassignedCellCount++;
                        newCellsChunk.Add(cell);
                        cellsToCheck.Push(cell);

                        while (cellsToCheck.Count > 0)
                        {
                            Cell currentFillCell = cellsToCheck.Pop();
                            visited[currentFillCell.GridPosition.x, currentFillCell.GridPosition.y] = true;

                            Cell neighbor = currentFillCell.GetTopNeighbor(grid);
                            TryAddNeighborToChunk(neighbor, visited, ref unassignedCellCount, newCellsChunk, cellsToCheck);
                            neighbor = currentFillCell.GetRightNeighbor(grid);
                            TryAddNeighborToChunk(neighbor, visited, ref unassignedCellCount, newCellsChunk, cellsToCheck);
                            neighbor = currentFillCell.GetBottomNeighbor(grid);
                            TryAddNeighborToChunk(neighbor, visited, ref unassignedCellCount, newCellsChunk, cellsToCheck);
                            neighbor = currentFillCell.GetLeftNeighbor(grid);
                            TryAddNeighborToChunk(neighbor, visited, ref unassignedCellCount, newCellsChunk, cellsToCheck);
                        }

                        cellsChunks.Add(newCellsChunk);
                    }
                    // else, go to next cell.
                }
            }

            // For each chunk, check the best neighbor zone the receive the cells, then assign the cells.
            foreach (var chunk in cellsChunks)
            {
                DictionaryList<string, Cell> neighborZonesCells = new DictionaryList<string, Cell>();
                Zone zoneWithBiggestDesiredAreaDif = null;
                Zone selectedZone = null;
                foreach (Cell cell in chunk)
                {
                    Cell neighbor = cell.GetTopNeighbor(grid);
                    FindNeighborZonesCells(neighbor, neighborZonesCells, ref zoneWithBiggestDesiredAreaDif);
                    neighbor = cell.GetRightNeighbor(grid);
                    FindNeighborZonesCells(neighbor, neighborZonesCells, ref zoneWithBiggestDesiredAreaDif);
                    neighbor = cell.GetBottomNeighbor(grid);
                    FindNeighborZonesCells(neighbor, neighborZonesCells, ref zoneWithBiggestDesiredAreaDif);
                    neighbor = cell.GetLeftNeighbor(grid);
                    FindNeighborZonesCells(neighbor, neighborZonesCells, ref zoneWithBiggestDesiredAreaDif);
                }

                Zone zoneWithMoreNeighbors = null;
                int neighborCount = 0;
                foreach (var val in neighborZonesCells.Dictionary)
                {
                    if (neighborCount < val.Value.Count)
                    {
                        zoneWithMoreNeighbors = val.Value[0].Zone;
                        neighborCount = val.Value.Count;
                    }
                }

                switch (_settings.UnassignedCellsAction)
                {
                    case MethodGrowthSettings.UnassignedCellsActionEnum.Nullify:
                        selectedZone = null;
                        break;
                    case MethodGrowthSettings.UnassignedCellsActionEnum.ToDesAreaDifference:
                        selectedZone = zoneWithBiggestDesiredAreaDif;
                        break;
                    case MethodGrowthSettings.UnassignedCellsActionEnum.ToNeighborCellCount:
                        selectedZone = zoneWithMoreNeighbors;
                        break;
                }


                if (_settings.UnassignedCellsAction != MethodGrowthSettings.UnassignedCellsActionEnum.none)
                {
                    foreach (Cell cell in chunk)
                    {
                        floorPlanManager.AssignCellToZone(cell, selectedZone);
                    }
                }
            }


            // After changing the zones they need to be baked again.
            // Its is done using dirty tagging since the zones which cells get removed
            // give more work to identify and store, requiring a new list of zones.
            // So to don't just rebake the zones selected to "selectedZone" it will
            // bake all modified zones after the process. The zones that are modified that wasn't
            // "selectedZone" usually are zones that got a cell removed and are higher in zones
            // hierarchy.
            ReBakeDirtyZones(floorPlanManager);

            return true;
        }


        /// <summary>
        /// Check if the neighbor of the cell is not assigned to a leaf zone, if not add to the current cells chunk, increment
        /// the unassigned cells counter and add it to cells to check stack;
        /// </summary>
        /// <param name="neighbor"></param>
        /// <param name="visited"></param>
        /// <param name="unassignedCellCount"></param>
        /// <param name="newCellsChunk"></param>
        /// <param name="cellsToCheck"></param>
        void TryAddNeighborToChunk(Cell neighbor, bool[,] visited, ref int unassignedCellCount, List<Cell> newCellsChunk, Stack<Cell> cellsToCheck)
        {
            // If the cell zone have children its not assign to undivisible zone.
            if (neighbor != null && neighbor.Zone != null && !neighbor.IsAssignedToLeafZone && !visited[neighbor.GridPosition.x, neighbor.GridPosition.y])
            {
                unassignedCellCount++;
                newCellsChunk.Add(neighbor);
                cellsToCheck.Push(neighbor);
            }
        }


        void FindNeighborZonesCells(Cell neighbor, DictionaryList<string, Cell> neighborZonesCells, ref Zone zoneWithBiggestDesiredAreaDif)
        {
            if (neighbor != null && neighbor.Zone != null && neighbor.IsAssignedToLeafZone)
            {
                if (zoneWithBiggestDesiredAreaDif == null || neighbor.Zone.DistanceFromDesiredArea() > zoneWithBiggestDesiredAreaDif.DistanceFromDesiredArea())
                {
                    zoneWithBiggestDesiredAreaDif = neighbor.Zone;
                }

                neighborZonesCells.AddValue(neighbor.Zone.ZoneId, neighbor);
            }
        }
    }
}

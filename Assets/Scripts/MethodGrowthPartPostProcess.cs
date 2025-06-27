using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BuildingGenerator
{
    public partial class MethodGrowth
    {
        [Header("Post process")]
        [Range(0, 4)] public int _maxNeighborsToHaveDoor = 2;

        bool SetWalls(FloorPlanManager floorPlanManager)
        {
            // Passar por todas as celulas
            // se id x e y > checar as da direita e baixo apenas
            // se id x || y == 0 checa tbm em cima e esquerda

            CellsGrid grid = floorPlanManager.CellsGrid;

            // - All tuples that belong to two adjacent zones. this is used to only modify the hasDoor property of tuples
            // and to check if all zones have doors candidates for all required adjacencies.
            // - Initialize the dictionary using the adjacency lists so both can have similar structures, making easier tho
            // check adjacency meet at the end.
            DictionaryLists<string, CellsTuple> doorsCandidates = new DictionaryLists<string, CellsTuple>(floorPlanManager.Adjacencies.Keys.ToArray());
            //DictionaryLists<string, CellsTuple> doorsCandidates = new DictionaryLists<string, CellsTuple>();

            for (int y = 0; y < grid.Dimensions.y; y++)
            {
                for (int x = 0; x < grid.Dimensions.x; x++)
                {
                    // ==================== Cell check ====================
                    Cell currentCell;
                    if (!grid.GetCell(x, y, out currentCell))
                    {
                        Debug.LogError("Invalid dimensions.");
                        return false;
                    }
                    // By default should not happen since the grid is initialized with new cells instances.
                    if (currentCell == null)
                        currentCell = new Cell(x, y, null);
                    if (currentCell.Zone != null && !currentCell.IsBorderCell)
                        continue;
                    if (currentCell.Zone != null && currentCell.Zone.HasChildrenZones)
                        continue;

                    // ==================== End cell check

                    // ==================== Check the neighbors ====================
                    // Will take a neighbor cell and if the zone of the neighbor is different from current zone, is a wall.
                    Cell neighborCell;

                    // Check on TOP only when on TOP matrix threshold.
                    if (y == 0 && currentCell.Zone != null) // Cell is border at matrix left threshold.
                    {
                        // Neighbor cell is on top out.
                        var newTuple = new CellsTuple(currentCell, new Cell(currentCell.GridPosition.x, currentCell.GridPosition.y - 1, null), false);
                        newTuple.SetOutsideBorder(true);
                        floorPlanManager.WallCellsTuples.Add(newTuple);

                        if (currentCell.Zone.HasOutsideDoor && currentCell.NumNeighborsInSameZone(grid) <= _maxNeighborsToHaveDoor)
                        {
                            doorsCandidates.AddValue(currentCell.Zone.ZoneId, "outside", newTuple);
                        }
                    }

                    // Check BOTTOM
                    grid.GetCell(currentCell.GridPosition.x, currentCell.GridPosition.y + 1, out neighborCell);
                    if (neighborCell == null)
                    {
                        // Neighbor is on bottom out.
                        neighborCell = new Cell(currentCell.GridPosition.x, currentCell.GridPosition.y + 1, null);
                    }

                    CreateWallTupleForInternalMatrixCells(currentCell, neighborCell, grid, doorsCandidates, floorPlanManager);

                    //========================================================================= Horizontal check

                    // Check on LEFT only when on LEFT matrix threshold.
                    if (x == 0 && currentCell.Zone != null) // Cell is border at matrix left threshold.
                    {
                        // Neighbor cell is on left out.
                        var newTuple = new CellsTuple(currentCell, new Cell(currentCell.GridPosition.x - 1, currentCell.GridPosition.y, null), false);
                        newTuple.SetOutsideBorder(true);
                        floorPlanManager.WallCellsTuples.Add(newTuple);
                        if (currentCell.Zone.HasOutsideDoor &&
                            currentCell.NumNeighborsInSameZone(grid) <= _maxNeighborsToHaveDoor)
                        {
                            doorsCandidates.AddValue(currentCell.Zone.ZoneId, "outside", newTuple);
                        }
                    }

                    // Check RIGHT
                    grid.GetCell(currentCell.GridPosition.x + 1, currentCell.GridPosition.y, out neighborCell);
                    if (neighborCell == null)
                    {
                        // Neighbor cell is on right out.
                        neighborCell = new Cell(currentCell.GridPosition.x + 1, currentCell.GridPosition.y, null);
                    }

                    CreateWallTupleForInternalMatrixCells(currentCell, neighborCell, grid, doorsCandidates, floorPlanManager);
                    
                    // ==================== End check neighbors
                }
            }

            RandomDoorSelection(doorsCandidates);

            // =============================== ADJACENCY CHECK ==================================
            if (!AreAdjacencyConstsMeet(floorPlanManager.Adjacencies, doorsCandidates))
            {
                return false;
            }

            return true;
        }


        void CreateWallTupleForInternalMatrixCells(Cell currentCell, Cell neighborCell, CellsGrid grid, DictionaryLists<string, CellsTuple> doorsCandidates, FloorPlanManager floorPlanManager)
        {
            if (neighborCell?.Zone != currentCell?.Zone)
            {
                CellsTuple newTuple = new CellsTuple(currentCell, neighborCell, false);

                if (currentCell.Zone != null && neighborCell.Zone != null && currentCell.Zone.MustBeAdjacentTo(neighborCell.Zone))
                {
                    if (currentCell.NumNeighborsInSameZone(grid) < 3 || neighborCell.NumNeighborsInSameZone(grid) < 3)
                        doorsCandidates.AddValue(currentCell.Zone.ZoneId, neighborCell.Zone.ZoneId, newTuple);
                }

                if (CanTupleHaveADoor(currentCell, neighborCell, grid))
                {
                    if (currentCell.Zone != null && neighborCell.Zone == null && currentCell.Zone.HasOutsideDoor)
                    {
                        newTuple.SetOutsideBorder(true);
                        doorsCandidates.AddValue(currentCell.Zone.ZoneId, "outside", newTuple);
                    }
                    else if (currentCell.Zone == null && neighborCell.Zone != null && neighborCell.Zone.HasOutsideDoor)
                    {
                        newTuple.SetOutsideBorder(true);
                        doorsCandidates.AddValue("outside", neighborCell.Zone.ZoneId, newTuple);
                    }
                }

                floorPlanManager.WallCellsTuples.Add(newTuple);
            }
        }


        bool CanTupleHaveADoor(Cell cellA, Cell cellB, CellsGrid grid)
        {
            // One cell have a number of neighbors between a min and max value
            // Or the cell have a number of neighbors between a min and max value
            return cellA.NumNeighborsInSameZone(grid) <= _maxNeighborsToHaveDoor ||
                   cellB.NumNeighborsInSameZone(grid) <= _maxNeighborsToHaveDoor;
        }


        /// <summary>
        /// Randonly select doors from door candidates pairs to mark it as having a door.
        /// </summary>
        /// <param name="doorsCandidates"></param>
        void RandomDoorSelection(DictionaryLists<string, CellsTuple> doorsCandidates)
        {
            foreach (var dictionary in doorsCandidates.Dictionary.Values)
            {
                foreach (var list in dictionary.Dictionary.Values)
                {
                    int id = Utils.Random.RandomRange(0, list.Count - 1);
                    list[id].SetHasDoor(true);
                }
            }
        }


        bool AreAdjacencyConstsMeet(Dictionary<string, string[]> adjacencyRules, DictionaryLists<string, CellsTuple> doorsCandidates)
        {
            // Compare the adjacency rules with the door candidates, if all adjacency rules have at least one valid candidate door
            // it means all zones that shold be connected(have a door) actually have a door.
            foreach (var adjRule in adjacencyRules)
            {
                string zone = adjRule.Key;

                foreach (var adjacentZone in adjRule.Value)
                {
                    Debug.Log($"Checking adjacency for <b>{zone}</b> and <b>{adjacentZone}</b>...");

                    // This section documents how adjacency between zones relates to door placement candidates.
                    //
                    // Zone adjacency:
                    // [zone1] -> [zone2], [zone3]
                    // [zone2] -> [zone4]
                    //
                    // Door candidate mapping:
                    // [zone1]
                    //   ├─ [zone2] -> [candTuple1], [candTuple2]
                    //   └─ [zone3] -> [candTuple3], [candTuple4], [candTuple5]
                    // [zone2]
                    //   └─ [zone4] -> [candTuple6], [candTuple7]
                    //
                    // Example:
                    // Checks whether [zone3], adjacent to [zone1], has at least one valid <candTuple>.

                    if (doorsCandidates.TryGetValue(zone, adjacentZone, out var listOfDoorCandidates))
                    {
                        if (listOfDoorCandidates.Count == 0)
                        {
                            // At least one adjacency constraint not meet.
                            Debug.LogError($"Post process: Adjacency not meet for <b>{zone}</b> and <b>{adjacentZone}</b>");
                            return false;
                        }
                    }
                    else
                    {
                        Debug.LogError($"<color=red>Adjacency not meet</color> for <b>{zone}</b> and <b>{adjacentZone}</b>");
                        return false;
                    }
                    Debug.Log($"<color=green>Adjacencies ok for the pair.</color>");
                }
            }

            return true;
        }
        

        void Debug_SetAllDoorCandidatesAsDoors(DictionaryLists<string, CellsTuple> doorsCandidates)
        {
            foreach (var dictionary in doorsCandidates.Dictionary.Values)
            {
                foreach (var list in dictionary.Dictionary.Values)
                {
                    foreach (var tuple in list)
                    {
                        tuple.SetHasDoor(true);
                    }
                }
            }
        }
    }
}

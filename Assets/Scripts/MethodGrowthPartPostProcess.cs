using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Animations;

namespace BuildingGenerator
{
    public partial class MethodGrowth
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="floorPlanManager"></param>
        /// <returns></returns>
        bool PlaceWallsAndCheckConnectivity(FloorPlanManager floorPlanManager)
        {
            // Passar por todas as celulas
            // se id x e y > checar as da direita e baixo apenas
            // se id x || y == 0 checa tbm em cima e esquerda

            CellsGrid grid = floorPlanManager.CellsGrid;

            // - All tuples that belong to two adjacent zones. this is used to only modify the hasDoor property of tuples
            // and to check if all zones have doors candidates for all required adjacencies.
            // - Initialize the dictionary using the adjacency lists so both can have similar structures, making easier tho
            // check adjacency meet at the end.
            DictionaryDictionaryList<Guid, CellsTuple> doorsCandidates = new DictionaryDictionaryList<Guid, CellsTuple>(floorPlanManager.Adjacencies.Keys.ToArray());
            //DictionaryLists<string, CellsTuple> doorsCandidates = new DictionaryLists<string, CellsTuple>();

            for (int y = 0; y < grid.Dimensions.y; y++)
            {
                for (int x = 0; x < grid.Dimensions.x; x++)
                {
                    // Validate Current Cell
                    Cell currentCell;
                    if (!grid.GetCell(x, y, out currentCell))
                    {
                        Utils.Debug.DevError("Invalid dimensions.");
                        return false;
                    }
                    // By default should not happen since the grid is initialized with new cells instances.
                    if (currentCell == null)
                        currentCell = new Cell(x, y, null);
                    if (currentCell.Zone != null && !currentCell.IsBorderCell)
                        continue;
                    if (currentCell.Zone != null && !currentCell.Zone.IsLeaf)
                        continue;


                    // ==================== Check the neighbors ====================
                    // Will take a neighbor cell and if the zone of the neighbor is different from current zone, is a wall.
                    // TODO: this section can be more simplifeild, create method for "check bottom and right" and the put
                    // and 3 methods call in one new method.
                    Cell neighborCell;

                    // Evaluate TOP matrix threshold.
                    EvaluateMatrixTopLeftThresholds(UniGridPosModifiers.TOP_MOD, y, currentCell, floorPlanManager, doorsCandidates);

                    // Check BOTTOM
                    //grid.GetCell(currentCell.GridPosition.x, currentCell.GridPosition.y + 1, out neighborCell);
                    neighborCell = currentCell.BottomNeighbor;
                    if (neighborCell == null)
                    {
                        // Neighbor is on bottom out.
                        neighborCell = new Cell(currentCell.GridPosition.x + UniGridPosModifiers.BOTTOM_MOD.x, currentCell.GridPosition.y + UniGridPosModifiers.BOTTOM_MOD.y, null);
                    }

                    CreateWallTupleForInternalMatrixCells(currentCell, neighborCell, doorsCandidates, floorPlanManager);

                    //========================================================================= Horizontal check

                    // Evaluate LEFT matrix threshold.
                    EvaluateMatrixTopLeftThresholds(UniGridPosModifiers.LEFT_MOD, x, currentCell, floorPlanManager, doorsCandidates);

                    // Check RIGHT
                    //grid.GetCell(currentCell.GridPosition.x + 1, currentCell.GridPosition.y, out neighborCell);
                    neighborCell = currentCell.RightNeighbor;
                    if (neighborCell == null)
                    {
                        // Neighbor cell is on right out.
                        neighborCell = new Cell(currentCell.GridPosition.x + UniGridPosModifiers.RIGHT_MOD.x, currentCell.GridPosition.y + UniGridPosModifiers.RIGHT_MOD.y, null);
                    }

                    CreateWallTupleForInternalMatrixCells(currentCell, neighborCell, doorsCandidates, floorPlanManager);
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


        /// <summary>
        /// For checking the neighbor cells on top and left of current cells, since the algorithm only checks the
        /// right and bottom ones to avoid redundance. Will execute only if coordinate is 0 and the current cell is in a zone.
        /// </summary>
        /// <param name="axis">Axis.X or Axis.Y</param>
        /// <param name="axisCoord">Must be 0.</param>
        /// <param name="currentCell"></param>
        /// <param name="grid"></param>
        /// <param name="floorPlanManager"></param>
        /// <param name="doorsCandidates"></param>
        void EvaluateMatrixTopLeftThresholds(Vector2Int coordModifier, int axisCoord, Cell currentCell, FloorPlanManager floorPlanManager, DictionaryDictionaryList<Guid, CellsTuple> doorsCandidates)
        {
            if (axisCoord == 0 && currentCell.Zone != null) // Cell is border at matrix left threshold.
            {
                // Neighbor cell is on top out. Create a fake cell for the neighbor cell.
                var newTuple = new CellsTuple(currentCell, new Cell(currentCell.GridPosition.x + coordModifier.x, currentCell.GridPosition.y + coordModifier.y, null), false);
                newTuple.SetOutsideBorder(true);
                floorPlanManager.WallCellsTuples.Add(newTuple);

                if (currentCell.Zone.HasOutsideDoor && currentCell.NumNeighborsInSameZone() <= _settings.MaxNeighborsToHaveDoor)
                {
                    doorsCandidates.AddValue(currentCell.Zone.GUID, _outsideZoneId, newTuple);
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="currentCell"></param>
        /// <param name="neighborCell"></param>
        /// <param name="grid"></param>
        /// <param name="doorsCandidates"></param>
        /// <param name="floorPlanManager"></param>
        void CreateWallTupleForInternalMatrixCells(Cell currentCell, Cell neighborCell, DictionaryDictionaryList<Guid, CellsTuple> doorsCandidates, FloorPlanManager floorPlanManager)
        {
            if (neighborCell?.Zone != currentCell?.Zone)
            {
                CellsTuple newTuple = new CellsTuple(currentCell, neighborCell, false);

                if (currentCell.Zone != null && neighborCell.Zone != null && currentCell.Zone.MustBeAdjacentTo(neighborCell.Zone))
                {
                    if (currentCell.NumNeighborsInSameZone() < 3 || neighborCell.NumNeighborsInSameZone() < 3)
                        doorsCandidates.AddValue(currentCell.Zone.GUID, neighborCell.Zone.GUID, newTuple);
                }

                if (CanTupleHaveADoor(currentCell, neighborCell))
                {
                    if (currentCell.Zone != null && neighborCell.Zone == null && currentCell.Zone.HasOutsideDoor)
                    {
                        newTuple.SetOutsideBorder(true);
                        doorsCandidates.AddValue(currentCell.Zone.GUID, _outsideZoneId, newTuple);
                    }
                    else if (currentCell.Zone == null && neighborCell.Zone != null && neighborCell.Zone.HasOutsideDoor)
                    {
                        newTuple.SetOutsideBorder(true);
                        doorsCandidates.AddValue(_outsideZoneId, neighborCell.Zone.GUID, newTuple);
                    }
                }

                floorPlanManager.WallCellsTuples.Add(newTuple);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="cellA"></param>
        /// <param name="cellB"></param>
        /// <param name="grid"></param>
        /// <returns></returns>
        bool CanTupleHaveADoor(Cell cellA, Cell cellB)
        {
            // One cell have a number of neighbors between a min and max value
            // Or the cell have a number of neighbors between a min and max value
            return cellA.NumNeighborsInSameZone() <= _settings.MaxNeighborsToHaveDoor ||
                   cellB.NumNeighborsInSameZone() <= _settings.MaxNeighborsToHaveDoor;
        }


        /// <summary>
        /// Randonly select doors from door candidates pairs to mark it as having a door.
        /// </summary>
        /// <param name="doorsCandidates"></param>
        void RandomDoorSelection(DictionaryDictionaryList<Guid, CellsTuple> doorsCandidates)
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


        /// <summary>
        /// 
        /// </summary>
        /// <param name="adjacencyRules"></param>
        /// <param name="doorsCandidates"></param>
        /// <returns></returns>
        bool AreAdjacencyConstsMeet(Dictionary<Guid, Guid[]> adjacencyRules, DictionaryDictionaryList<Guid, CellsTuple> doorsCandidates)
        {
            // Compare the adjacency rules with the door candidates, if all adjacency rules have at least one valid candidate door
            // it means all zones that shold be connected(have a door) actually have a door.
            foreach (var adjRule in adjacencyRules)
            {
                Guid zone = adjRule.Key;

                foreach (var adjacentZone in adjRule.Value)
                {
                    //Debug.Log($"Checking adjacency for <b>{zone}</b> and <b>{adjacentZone}</b>...");

                    // This section documents how adjacency between zones relates to door placement candidates.
                    //
                    // Zone adjacency:
                    // [zone1] -> [zone2], [zone3]
                    // [zone2] -> [zone4]
                    //
                    // Door candidate mapping:
                    // [zone1]
                    //   ├─ [zone2] -> [candTuple1], [candTuple2]
                    // [zone2]
                    //   └─ [zone4] -> [candTuple6], [candTuple7]
                    // [Zone3]
                    //   └─ [Zone1] -> [candTuple3], [candTuple4], [candTuple5]
                    //
                    // Notice that, due to the way the elements are inserted in the dictionary they are not
                    // identical(the 'outside' zone is the main reason), and for verification we will have
                    // a PERMUTATION doing for ex. Zone1,Zone3 and Zone3,Zone1.
                    //
                    // Example:
                    // Checks whether [zone3], adjacent to [zone1], has at least one valid <candTuple>.

                    if (doorsCandidates.TryGetValue(zone, adjacentZone, out var listOfDoorCandidates))
                    {
                        if (listOfDoorCandidates.Count == 0)
                        {
                            // At least one adjacency constraint not meet.
                            Utils.Debug.DevError($"Post process: Adjacency not meet for <b>{zone}</b> and <b>{adjacentZone}</b>");
                            return false;
                        }
                    }
                    else
                    {
                        Utils.Debug.DevError($"<color=red>Adjacency not meet</color> for <b>{zone}</b> and <b>{adjacentZone}</b>");
                        return false;
                    }
                    //Debug.Log($"<color=green>Adjacencies ok for the pair.</color>");
                }
            }

            return true;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="doorsCandidates"></param>
        void Debug_SetAllDoorCandidatesAsDoors(DictionaryDictionaryList<string, CellsTuple> doorsCandidates)
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

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
    public class Cell // dando preferencia para classes para passar por ref
    {
        private Zone _zone;
        private Vector2Int _gridPosition;
        private bool _isBorderCell;

        // TODO: cache neighbors

        public Dictionary<string, string> atributos;

        public Zone Zone => _zone;
        public Vector2Int GridPosition => _gridPosition;
        public bool IsBorderCell => _isBorderCell;

        public bool IsAssignedToLeafZone => (_zone != null)? _zone.IsLeaf : false; // Is assigned to a leaf zone if this zone don't have children.

        public string _TESTVAR = "-";


        public Cell(int gridPositionX, int gridPositionY, Zone zone = null)
        {
            _gridPosition = new Vector2Int(gridPositionX, gridPositionY);
            _zone = zone;

            atributos = new Dictionary<string, string>();
        }


        public Cell GetTopNeighbor(CellsGrid cellsGrid)
        {
            return GetNeighbor(cellsGrid, 0, -1);
        }

        public Cell GetBottomNeighbor(CellsGrid cellsGrid)
        {
            return GetNeighbor(cellsGrid, 0, 1);
        }

        public Cell GetLeftNeighbor(CellsGrid cellsGrid)
        {
            return GetNeighbor(cellsGrid, -1, 0);
        }

        public Cell GetRightNeighbor(CellsGrid cellsGrid)
        {
            return GetNeighbor(cellsGrid, 1, 0);
        }

        private Cell GetNeighbor(CellsGrid cellsGrid, int xMod, int yMod)
        {
            cellsGrid.GetCell(_gridPosition.x + xMod, _gridPosition.y + yMod, out Cell neighbor);
            return neighbor;
        }


        // TODO: after having direct ref to neigbors, change the way to calculate.
        public int NumNeighborsInSameZone(CellsGrid cellsGrid)
        {
            int counter = 0;

            if (GetTopNeighbor(cellsGrid)?.Zone == _zone) counter++;
            if (GetBottomNeighbor(cellsGrid)?.Zone == _zone) counter++;
            if (GetLeftNeighbor(cellsGrid)?.Zone == _zone) counter++;
            if (GetRightNeighbor(cellsGrid)?.Zone == _zone) counter++;

            return counter;
        }


        public void SetZone(Zone newZone)
        {
            _zone = newZone;
        }

        public void SetIsBorderCell(bool value)
        {
            _isBorderCell = value;
        }

        public bool IsInZone(Zone zoneToCheck)
        {
            return zoneToCheck == _zone;
        }

        public List<Zone> GetParentZonesHierarchy()
        {
            List<Zone> parentZones = new List<Zone>();

            Zone currentZone = _zone;
            while (currentZone != null)
            {
                parentZones.Add(currentZone);
                currentZone = currentZone.ParentZone;
            }

            return parentZones;
        }
    }
}
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
    [System.Serializable]
    public class Cell // dando preferencia para classes para passar por ref
    {
        private Zone _zone;
        private Vector2Int _gridPosition;
        private bool _isBorderCell;

        private Cell _topNeighbor = null;
        private Cell _topRightNeighbor = null;
        private Cell _rightNeighbor = null;
        private Cell _rightBottomNeighbor = null;
        private Cell _bottomNeighbor = null;
        private Cell _bottomLeftNeighbor = null;
        private Cell _leftNeighbor = null;
        private Cell _leftTopNeighbor = null;

        public Zone Zone => _zone;
        public Vector2Int GridPosition => _gridPosition;
        public bool IsBorderCell => _isBorderCell;
        public bool IsAssignedToLeafZone => (_zone != null) ? _zone.IsLeaf : false; // Is assigned to a leaf zone if this zone don't have children.

        public Cell TopNeighbor { set { _topNeighbor = value; } get { return _topNeighbor; } }
        public Cell TopRightNeighbor { set { _topRightNeighbor = value; } get { return _topRightNeighbor; } }
        public Cell RightNeighbor { set { _rightNeighbor = value; } get { return _rightNeighbor; } }
        public Cell RightBottomNeighbor { set { _rightBottomNeighbor = value; } get { return _rightBottomNeighbor; } }
        public Cell BottomNeighbor { set { _bottomNeighbor = value; } get { return _bottomNeighbor; } }
        public Cell BottomLeftNeighbor { set { _bottomLeftNeighbor = value; } get { return _bottomLeftNeighbor; } }
        public Cell LeftNeighbor { set { _leftNeighbor = value; } get { return _leftNeighbor; } }
        public Cell LeftTopNeighbor { set { _leftTopNeighbor = value; } get { return _leftTopNeighbor; } }



        public Cell(int gridPositionX, int gridPositionY, Zone zone = null)
        {
            _gridPosition = new Vector2Int(gridPositionX, gridPositionY);
            _zone = zone;
        }

        /*
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
        */

        // TODO: after having direct ref to neigbors, change the way to calculate.
        public int NumNeighborsInSameZone()
        {
            int counter = 0;

            if (TopNeighbor?.Zone == _zone) counter++;
            if (BottomNeighbor?.Zone == _zone) counter++;
            if (LeftNeighbor?.Zone == _zone) counter++;
            if (RightNeighbor?.Zone == _zone) counter++;

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
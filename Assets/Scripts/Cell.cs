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

        public Dictionary<string, string> atributos;

        public Zone Zone => _zone;
        public Vector2Int GridPosition => _gridPosition;


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


        public void SetZone(Zone newZone)
        {
            _zone = newZone;
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
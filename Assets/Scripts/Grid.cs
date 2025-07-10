using UnityEngine;

/*

Expected Grid orientation.

  ----------------------->X
  |[][][][][][][][][][][]
  |[][][][][][][][][][][]
  |[][][][][][][][][][][]
  |[][][][][][][][][][][]
  |[][][][][][][][][][][]
  |[][][][][][][][][][][]
  |[][][][][][][][][][][]
  |[][][][][][][][][][][]
  |[][][][][][][][][][][]
Y V

 */



namespace BuildingGenerator
{
    /// <summary>
    /// Use to get a grid position modifier, generally used to get the neighbors of grid cells.
    /// Using this make possible to change the grid orientation. 
    /// </summary>
    public class UniGridPosModifiers
    {
        public static readonly Vector2Int TOP_MOD = new Vector2Int(0, -1);
        public static readonly Vector2Int RIGHT_MOD = new Vector2Int(1, 0);
        public static readonly Vector2Int BOTTOM_MOD = new Vector2Int(0, 1);
        public static readonly Vector2Int LEFT_MOD = new Vector2Int(-1, 0);
    }
    

    /// <summary>
    /// 
    /// </summary>
    public class CellsGrid // using class to facilitate passing values by reference
    {
        private readonly Vector2Int _dimensions; // dimen��es da grade.
        private Cell[] _cells; // array pois o tamanho das grids n deve mudar.
        private readonly int _largestDimension;
        private readonly float _diagonalMagnitudeRounded;


        public Vector2Int Dimensions => _dimensions;
        public Cell[] Cells => _cells;
        public int LargestDimension => _largestDimension;
        public float DiagonalMagnitudeRounded => _diagonalMagnitudeRounded;
        public int Area => _cells.Length;

        public int TopIndex => 0;
        public int RightIndex => _dimensions.x - 1;
        public int BottomIndex => _dimensions.y - 1;
        public int LeftIndex => 0;


        public CellsGrid(Vector2Int dimensions)
        {
            _dimensions = new Vector2Int(dimensions.x, dimensions.y);
            _largestDimension = _dimensions.x > _dimensions.y ? _dimensions.x : _dimensions.y;
            _diagonalMagnitudeRounded = Mathf.Round(_dimensions.magnitude);
            _cells = new Cell[dimensions.x * dimensions.y];

            int index = 0;

            // Create all cells.
            for (int y = 0; y < dimensions.y; y++)
            {
                for (int x = 0; x < dimensions.x; x++)
                {
                    _cells[index] = new Cell(x, y);
                    index++;
                }
            }

            // Set the cells neighbors. Avoiding try add neighbor outside the matrix.
            for (int i = 0; i < _cells.Length; i++)
            {
                Cell cell = _cells[i];

                // Set top.
                if (cell.GridPosition.y > TopIndex)// if (cell.GridPosition.y > 0)
                {
                    cell.TopNeighbor = _cells[Utils.MatrixToArrayIndex(cell.GridPosition.x + UniGridPosModifiers.TOP_MOD.x,
                                                                       cell.GridPosition.y + UniGridPosModifiers.TOP_MOD.y,
                                                                       _dimensions.x)];
                }
                // Set top right.
                if (cell.GridPosition.y > TopIndex && cell.GridPosition.x < RightIndex) // if (cell.GridPosition.y > 0 && cell.GridPosition.x < _dimensions.x - 1)
                {
                    cell.TopRightNeighbor = _cells[Utils.MatrixToArrayIndex(cell.GridPosition.x + UniGridPosModifiers.TOP_MOD.x + UniGridPosModifiers.RIGHT_MOD.x,
                                                                            cell.GridPosition.y + UniGridPosModifiers.TOP_MOD.y + UniGridPosModifiers.RIGHT_MOD.y,
                                                                            _dimensions.x)];
                }
                // Set right.
                if (cell.GridPosition.x < RightIndex) // if (cell.GridPosition.x < _dimensions.x - 1)
                {
                    cell.RightNeighbor = _cells[Utils.MatrixToArrayIndex(cell.GridPosition.x + UniGridPosModifiers.RIGHT_MOD.x,
                                                                         cell.GridPosition.y + UniGridPosModifiers.RIGHT_MOD.y,
                                                                         _dimensions.x)];
                }
                // Set right bottom.
                if (cell.GridPosition.x < RightIndex && cell.GridPosition.y < BottomIndex) // if (cell.GridPosition.x < _dimensions.x - 1 && cell.GridPosition.y < _dimensions.y - 1)
                {
                    cell.RightBottomNeighbor = _cells[Utils.MatrixToArrayIndex(cell.GridPosition.x + UniGridPosModifiers.RIGHT_MOD.x + UniGridPosModifiers.BOTTOM_MOD.x,
                                                                               cell.GridPosition.y + UniGridPosModifiers.RIGHT_MOD.y + UniGridPosModifiers.BOTTOM_MOD.y,
                                                                               _dimensions.x)];
                }
                // Set bottom.
                if (cell.GridPosition.y < BottomIndex) // if (cell.GridPosition.y < _dimensions.y - 1)
                {
                    cell.BottomNeighbor = _cells[Utils.MatrixToArrayIndex(cell.GridPosition.x + UniGridPosModifiers.BOTTOM_MOD.x,
                                                                          cell.GridPosition.y + UniGridPosModifiers.BOTTOM_MOD.y,
                                                                          _dimensions.x)];
                }
                // Set bottom left.
                if (cell.GridPosition.y < BottomIndex && cell.GridPosition.x > LeftIndex) // if (cell.GridPosition.y < _dimensions.y - 1 && cell.GridPosition.x > 0)
                {
                    cell.BottomLeftNeighbor = _cells[Utils.MatrixToArrayIndex(cell.GridPosition.x + UniGridPosModifiers.BOTTOM_MOD.x + UniGridPosModifiers.LEFT_MOD.x,
                                                                              cell.GridPosition.y + UniGridPosModifiers.BOTTOM_MOD.y + UniGridPosModifiers.LEFT_MOD.y,
                                                                              _dimensions.x)];
                }
                // Set left.
                if (cell.GridPosition.x > LeftIndex) // if (cell.GridPosition.x > 0)
                {
                    cell.LeftNeighbor = _cells[Utils.MatrixToArrayIndex(cell.GridPosition.x + UniGridPosModifiers.LEFT_MOD.x,
                                                                        cell.GridPosition.y + UniGridPosModifiers.LEFT_MOD.y,
                                                                        _dimensions.x)];
                }
                // Set left top.
                if (cell.GridPosition.x > LeftIndex && cell.GridPosition.y > TopIndex) // if (cell.GridPosition.x > 0 && cell.GridPosition.y > 0)
                {
                    cell.LeftTopNeighbor = _cells[Utils.MatrixToArrayIndex(cell.GridPosition.x + UniGridPosModifiers.LEFT_MOD.x + UniGridPosModifiers.TOP_MOD.x,
                                                                           cell.GridPosition.y + UniGridPosModifiers.LEFT_MOD.y + UniGridPosModifiers.TOP_MOD.y,
                                                                           _dimensions.x)];
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public bool IsValidPosition(int x, int y)
        {
            return x >= 0 && x < _dimensions.x && y >= 0 && y < _dimensions.y;
        }


        /// <summary>
        /// Return true if is a valid grid position but, the out cell still can be null.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="cell"></param>
        /// <returns></returns>
        public bool GetCell(int x, int y, out Cell cell)
        {
            if (IsValidPosition(x, y))
            {
                cell = _cells[Utils.MatrixToArrayIndex(x, y, _dimensions.x)];
                return true;
            }
            else
            {
                //Debug.LogWarning($"Invalid grid position:{x},{y}");
                cell = null;
                return false;
            }
        }


        #region DEBUG

        /// <summary>
        /// 
        /// </summary>
        public void PrintGrid()
        {
            Debug.Log(GridToString());
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string GridToString()
        {
            string result = "\n";
            for (int i = 0; i < _dimensions.y; i++)
            {
                //result += i.ToString() + ":";
                for (int j = 0; j < _dimensions.x; j++)
                {
                    Zone zone = _cells[i * _dimensions.x + j].Zone;
                    string num = "--";
                    if (zone != null)
                    {
                        //num = Int32.Parse(zone.ZoneId).ToString("D2");
                        num = zone.ZoneId;
                    }
                    result += $"[{num}]";
                }
                result += "\n";
            }
            return result;
        }

        #endregion
    }
}
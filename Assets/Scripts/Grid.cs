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


    public CellsGrid(Vector2Int dimensions)
    {
        _dimensions = new Vector2Int(dimensions.x, dimensions.y);
        _largestDimension = _dimensions.x > _dimensions.y? _dimensions.x : _dimensions.y;
        _diagonalMagnitudeRounded = Mathf.Round(_dimensions.magnitude);
        _cells = new Cell[dimensions.x * dimensions.y];

        int index = 0;

        for (int y = 0; y < dimensions.y; y++)
        {
            for(int x = 0; x < dimensions.x; x++)
            {
                _cells[index] = new Cell(x, y);
                index++;
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
    /// 
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="cell"></param>
    /// <returns></returns>
    public bool GetCell(int x, int y, out Cell cell)
    {
        if(IsValidPosition(x, y))
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
        for(int i = 0; i < _dimensions.y; i++)
        {
            //result += i.ToString() + ":";
            for(int j = 0; j < _dimensions.x; j++)
            {
                Zone zone = _cells[i * _dimensions.x + j].Zone;
                string num = "--";
                if(zone != null)
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
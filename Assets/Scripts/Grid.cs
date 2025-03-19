using System;
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


public class CellsGrid // using class to facilitate passing values by reference
{
    public Vector2Int _dimensions; // dimen��es da grade.
    public Cell[] _cells; // array pois o tamanho das grids n deve mudar.

    public Vector2Int Dimensions => _dimensions;
    public int LargestDimension => _dimensions.x > _dimensions.y? _dimensions.x : _dimensions.y;
    public Cell[] Cells => _cells;


    public CellsGrid(Vector2Int dimensions)
    {
        _dimensions = new Vector2Int(dimensions.x, dimensions.y);
        _cells = new Cell[dimensions.x * dimensions.y];

        int index = 0;

        for (int y = 0; y < dimensions.y; y++)
        {
            for(int x = 0; x < dimensions.x; x++)
            {
                _cells[index] = new Cell(x, y);
                if(x == 8 || x == 12)
                {
                    _cells[index].atributos.Add("parede", "sim");
                }

                index++;
            }
        }
    }


    public bool IsValidPosition(int x, int y)
    {
        return x >= 0 && x < _dimensions.x && y >= 0 && y < _dimensions.y;
    }

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

    public void SetCell(int x, int y, Cell value)
    {
        Cell cell;
        if (GetCell(x, y, out cell))
        {
            cell = value;
        }
    }

    public void PrintGrid()
    {
        Debug.Log(GridToString());
    }

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


    public bool AssignCellToZone(int x, int y, Zone zone)
    {
        if(GetCell(x, y, out Cell cell))
        {
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


    // O uso de Actions para isso reduz performance, prefira itera��o direta. TODO verificar se � possivel passar a expres�o lambda direto sem uso de Actions e se isso � mais rapido.
    // USE DE REF COM ACTIONS N�O � PERMITIDO
    // <!--
    // order:-40
    // -->
    /*
    public void ForEachCellOnGrid(Action<T> action)
    {
        for(int i = 0; i < _cells.Length; i++)
        {
            action(ref _cells[i]);
        }
    }
    */
}
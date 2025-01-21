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
    public Vector2Int _dimmensions; // dimen��es da grade.
    public Cell[] _cells; // array pois o tamanho das grids n deve mudar.

    public Vector2Int Dimmensions => _dimmensions;


    public CellsGrid(Vector2Int dimmensions)
    {
        _dimmensions = new Vector2Int(dimmensions.x, dimmensions.y);
        _cells = new Cell[dimmensions.x * dimmensions.y];

        int index = 0;

        for (int y = 0; y < dimmensions.y; y++)
        {
            for(int x = 0; x < dimmensions.x; x++)
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
        return x >= 0 && x < _dimmensions.x && y >= 0 && y < _dimmensions.y;
    }

    public bool GetCell(int x, int y, out Cell cell)
    {
        if(IsValidPosition(x, y))
        {
            cell = _cells[MatrixToArrayIndex(x, y, _dimmensions.x)];
            return true;
        }
        else
        {
            Debug.LogWarning($"Invalid grid position:{x},{y}");
            cell = default;
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

    /*
    public void PrintGrid()
    {
        string result = "\n";
        for(int i = 0; i < _dimmensions.y; i++)
        {
            result += i.ToString() + ":";
            for(int j = 0; j < _dimmensions.x; j++)
            {
                result += "[ " + _cells[i * _dimmensions.x + j] + " ]";
            }
            result += "\n";
        }
        Debug.Log(result);
    }
    */

    public static Vector2Int ArrayIndexToMatrix(int index, int size_x)
    {
        return new Vector2Int(index / size_x, index % size_x);
    }

    public static int MatrixToArrayIndex(int x, int y, int size_x)
    {
        return size_x * y + x;
    }


    // O uso de Actions para isso reduz performance, prefira itera��o direta. TODO: verificar se � possivel passar a expres�o lambda direto sem uso de Actions e se isso � mais rapido.
    // USE DE REF COM ACTIONS N�O � PERMITIDO
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
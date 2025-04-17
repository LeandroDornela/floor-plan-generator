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
/// <summary>
/// OBS: Seguindo o padão da grid, leitura esquerda->direita, cima->baixo
/// </summary>
public class CellsLineDescription
{
    private Vector2Int _firstCellCoord;
    private int _numberOfCells;
    private Zone.Side _side;
    private int _totalDistanceExpanded;


    public Vector2Int FirstCellCoord { get => _firstCellCoord; set => _firstCellCoord = value; }
    public int NumberOfCells { get => _numberOfCells; set => _numberOfCells = value; }
    public Zone.Side Side => _side;
    public int TotalDistanceExpanded => _totalDistanceExpanded;


    public CellsLineDescription(int firstCellX, int firstCellY, int numberOfCells, Zone.Side side)
    {
        _firstCellCoord = new Vector2Int(firstCellX, firstCellY);
        _numberOfCells = numberOfCells;
        _side = side;
        _totalDistanceExpanded = 0;
    }

    public void AddCells(int amount)
    {
        _numberOfCells += amount;
    }

    public void MoveUp(int amount)
    {
        _firstCellCoord.y -= amount;

        if(_side == Zone.Side.Top)
        {
            _totalDistanceExpanded++;
        }
        else if(_side == Zone.Side.Bottom)
        {
            _totalDistanceExpanded--;
        }
    }

    public void MoveDown(int amount)
    {
        _firstCellCoord.y += amount;

        if(_side == Zone.Side.Top)
        {
            _totalDistanceExpanded--;
        }
        else if(_side == Zone.Side.Bottom)
        {
            _totalDistanceExpanded++;
        }
    }

    public void MoveLeft(int amount)
    {
        _firstCellCoord.x -= amount;

        if(_side == Zone.Side.Left)
        {
            _totalDistanceExpanded++;
        }
        else if(_side == Zone.Side.Right)
        {
            _totalDistanceExpanded--;
        }
    }

    public void MoveRight(int amount)
    {
        _firstCellCoord.x += amount;

        if(_side == Zone.Side.Left)
        {
            _totalDistanceExpanded--;
        }
        else if(_side == Zone.Side.Right)
        {
            _totalDistanceExpanded++;
        }
    }
}
}
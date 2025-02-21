using System.Collections.Generic;
using System.Runtime.Serialization.Formatters;
using UnityEngine;

/*
    Top Cells
    [][][][]
Left[][][][]Right
    [][][][]
    [][][][]
    Botton Cells
*/

// OBS: Seguindo o padão da grid, leitura esquerda->direita, cima->baixo
struct ZoneBorder
{
    public int firstCellX;
    public int firstCellY;
    public int numberOfCells;

    public ZoneBorder(int firstCellX, int firstCellY, int numberOfCells)
    {
        this.firstCellX = firstCellX;
        this.firstCellY = firstCellY;
        this.numberOfCells = numberOfCells;
    }
}


public class Zone // similar a uma estrutura de nos em arvore
{
    // Runtime
    public List<Cell> _cells; // Celulas atualmente associadas a zona.
    // Runtime const
    private string _zoneId;
    public Zone _parentZone; // A zona m�e pode ser usada para verificar se uma celula est� na mesma zona que outra.
    public List<Zone> _childZones;
    public List<Zone> _adjacentZones;

    // (first cell X, first cell Y, number of cells)
    private ZoneBorder _topCells;
    private ZoneBorder _bottomCells;
    private ZoneBorder _leftCells;
    private ZoneBorder _rightCells;

    private float _desiredAspect = 1; // 1 is square
    public float DesiredAspect => _desiredAspect;

  

    public string ZoneId => _zoneId;

    public Zone(string zoneId)
    {
        _zoneId = zoneId;
        _parentZone = null;
        _cells = new List<Cell>();
        _childZones = new List<Zone>();
        _adjacentZones = new List<Zone>();
    }


    public Zone(string zoneId, Zone parentZone)
    {
        _zoneId = zoneId;
        _parentZone = parentZone;

        _cells = new List<Cell>();
    }


    public void AddCell(Cell cell)
    {
        if(cell == null)
        {
            Debug.LogError("Tryng to add a null cell.");
            return;
        }

        // TODO evitar essa verificação, pode ser custoso. Fazer de forma que a culala não esteja na zona.
        // <!--
        // order:-70
        // -->
        if(_cells.Contains(cell))
        {
            Debug.LogError("Tryng to add an exiting cell.");
            return;
        }

        if(_cells.Count == 0) // First cell
        {
            _topCells = new ZoneBorder(cell.GridPosition.x, cell.GridPosition.y, 1);
            _bottomCells = new ZoneBorder(cell.GridPosition.x, cell.GridPosition.y, 1);
            _leftCells = new ZoneBorder(cell.GridPosition.x, cell.GridPosition.y, 1);
            _rightCells = new ZoneBorder(cell.GridPosition.x, cell.GridPosition.y, 1);
        }

        _cells.Add(cell);
        cell.SetZone(this);
    }

    public void RemoveCell(Cell cell)
    {
        if(cell == null)
        {
            Debug.LogError("Tryng to remove a null cell.");
            return;
        }

        if(_cells.Contains(cell))
        {
            _cells.Remove(cell);
            cell.SetZone(null);
        }
        else
        {
            Debug.LogError("The cell isn't in the zone.");
        }
    }


    public void AddChildZone(Zone childZone)
    {
        if(childZone == null)
        {
            Debug.LogError("Tryng to add a null child.");
            return;
        }

        if(!_childZones.Contains(childZone))
        {
            _childZones.Add(childZone);
        }
        else
        {
            Debug.LogWarning("Tryng to add an exiting child.");
        }
    }


    public void AddAdjacentZone(Zone adjacentZone)
    {
        //TODO Check if adj. zone is already set.
        // <!--
        // order:-80
        // -->
        _adjacentZones.Add(adjacentZone);
    }


    public void SetParentZone(Zone parentZone)
    {
        if(_parentZone == null)
        {
            _parentZone = parentZone;
        }
        else
        {
            Debug.LogError("Parent zone can't be overridden.");
        }
    }

    /// <summary>
    /// If < 1: zone is taller than wide
    /// If > 1: zone is wider than tall
    /// If == 1: zone is square
    /// Will not work well in 'L' shaped zones
    /// </summary>
    /// <returns></returns>
    public float GetZoneAspect()
    {
        return (float)_topCells.numberOfCells / _leftCells.numberOfCells;
    }

    public bool TryGrowTop(CellsGrid cellsGrid)
    {
        if(_cells.Count == 0)
        {
            Debug.LogError("No cells to grow.");
        }

        // Check if all cells on top of the top cells are available.
        for(int x = 0; x < _topCells.numberOfCells; x++)
        {
            if(cellsGrid.GetCell(_topCells.firstCellX + x, _topCells.firstCellY - 1, out Cell cell))
            {
                if(cell.Zone != _parentZone)
                {
                    return false;
                }
            }
            else
            {
                // Invalid grid pos.
                return false;
            }
        }

        // Passed the check, so can safelly assign the cells.
        for(int x = 0; x < _topCells.numberOfCells; x++)
        {
            cellsGrid.AssignCellToZone(_topCells.firstCellX + x, _topCells.firstCellY - 1, this);
        }
        _topCells.firstCellY--;
        _leftCells.firstCellY--;
        _rightCells.firstCellY--;
        _leftCells.numberOfCells++;
        _rightCells.numberOfCells++;

        return true;
    }


    public bool TryGrowBottom(CellsGrid cellsGrid)
    {
        if(_cells.Count == 0)
        {
            Debug.LogError("No cells to grow.");
        }

        // Check if all cells on top of the top cells are available.
        for(int x = 0; x < _bottomCells.numberOfCells; x++)
        {
            if(cellsGrid.GetCell(_bottomCells.firstCellX + x, _bottomCells.firstCellY + 1, out Cell cell))
            {
                if(cell.Zone != _parentZone)
                {
                    return false;
                }
            }
            else
            {
                // Invalid grid pos.
                return false;
            }
        }

        // Passed the check, so can safelly assign the cells.
        for(int x = 0; x < _bottomCells.numberOfCells; x++)
        {
            cellsGrid.AssignCellToZone(_bottomCells.firstCellX + x, _bottomCells.firstCellY + 1, this);
        }
        _bottomCells.firstCellY++;
        _leftCells.numberOfCells++;
        _rightCells.numberOfCells++;

        return true;
    }


    public bool TryGrowRight(CellsGrid cellsGrid)
    {
        if(_cells.Count == 0)
        {
            Debug.LogError("No cells to grow.");
        }

        // Check if all cells on top of the top cells are available.
        for(int y = 0; y < _rightCells.numberOfCells; y++)
        {
            if(cellsGrid.GetCell(_rightCells.firstCellX + 1, _rightCells.firstCellY + y, out Cell cell))
            {
                if(cell.Zone != _parentZone)
                {
                    return false;
                }
            }
            else
            {
                // Invalid grid pos.
                return false;
            }
        }

        // Passed the check, so can safelly assign the cells.
        for(int y = 0; y < _rightCells.numberOfCells; y++)
        {
            cellsGrid.AssignCellToZone(_rightCells.firstCellX + 1, _rightCells.firstCellY + y, this);
        }
        _rightCells.firstCellX++;
        _topCells.numberOfCells++;
        _bottomCells.numberOfCells++;

        return true;
    }

    public bool TryGrowLeft(CellsGrid cellsGrid)
    {
        if(_cells.Count == 0)
        {
            Debug.LogError("No cells to grow.");
        }

        // Check if all cells on top of the top cells are available.
        for(int y = 0; y < _leftCells.numberOfCells; y++)
        {
            if(cellsGrid.GetCell(_leftCells.firstCellX - 1, _leftCells.firstCellY + y, out Cell cell))
            {
                if(cell.Zone != _parentZone)
                {
                    return false;
                }
            }
            else
            {
                // Invalid grid pos.
                return false;
            }
        }

        // Passed the check, so can safelly assign the cells.
        for(int y = 0; y < _leftCells.numberOfCells; y++)
        {
            cellsGrid.AssignCellToZone(_leftCells.firstCellX - 1, _leftCells.firstCellY + y, this);
        }
        _leftCells.firstCellX--;
        _topCells.numberOfCells++;
        _topCells.firstCellX--;
        _bottomCells.numberOfCells++;
        _bottomCells.firstCellX--;

        return true;
    }
}

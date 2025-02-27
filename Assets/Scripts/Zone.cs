using System;
using System.Collections.Generic;
using UnityEngine;

/*
    Top Cells
    [o][-][-][o]
Left[-][-][-][-]Right
    [-][-][-][-]
    [o][-][-][-]
    Botton Cells
*/

// OBS: Seguindo o padão da grid, leitura esquerda->direita, cima->baixo
public struct ZoneBorder
{
    public int firstCellX;
    public int firstCellY;
    public int numberOfCells;

    public Zone.Side side;

    public ZoneBorder(int firstCellX, int firstCellY, int numberOfCells, Zone.Side side = Zone.Side.Top)
    {
        this.firstCellX = firstCellX;
        this.firstCellY = firstCellY;
        this.numberOfCells = numberOfCells;
        this.side = side;
    }
}

struct CoordModifier
{
    public int x_a, x_b, y_a, y_b;
    public CoordModifier(int xa, int xb, int ya, int yb)
    {
        x_a = xa;
        x_b = xb;
        y_a = ya;
        y_b = yb;
    }
}


public class Zone // similar a uma estrutura de nos em arvore
{
    public enum Side
    {
        Top = 0,
        Bottom = 1,
        Left = 2,
        Right = 3
        // TODO: add "none" and move to Zone.
    }

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

    private bool _isLShaped = false;
    public bool IsLShaped => _isLShaped;
    private ZoneBorder _lBorderCells;
  

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
#region Private Vertical grow functions
    bool TryGrowVertically(CellsGrid cellsGrid, Side side)
    {
        if(SpaceToGrowVertically(cellsGrid, side) == 0)
        {
            return false;
        }

        return GrowVertically(cellsGrid, side);
    }

    int SpaceToGrowVertically(CellsGrid cellsGrid, Side side) // retorna quantas fileiras livres ha na direcção, 0 se não ha como expandir
    {
        if(_cells == null || _cells?.Count == 0)
        {
            Debug.LogError("No cells to grow.");
        }

        // Setup the direction of growth.
        int sideDir;
        ZoneBorder zoneBorder;
        if(side == Side.Top)
        {
            sideDir = -1;
            zoneBorder = _topCells;
        }
        else if(side == Side.Bottom)
        {
            sideDir = 1;
            zoneBorder = _bottomCells;
        }
        else
        {
            Debug.LogError($"Invalid side. {side}");
            return 0;
        }
        

        // Check if all cells on top of the top cells are available.
        for(int x = 0; x < zoneBorder.numberOfCells; x++)
        {
            if(cellsGrid.GetCell(zoneBorder.firstCellX + x, zoneBorder.firstCellY + sideDir, out Cell cell))
            {
                if(cell.Zone != _parentZone)
                {
                    return 0;
                }
            }
            else
            {
                // Invalid grid pos.
                return 0;
            }
        }

        return 1;
    }

    bool GrowVertically(CellsGrid cellsGrid, Side side) // unsafe, do a space to check before or use trygrowtop
    {
        if(_cells == null || _cells?.Count == 0)
        {
            Debug.LogError("No cells to grow.");
            return false;
        }

        // Setup the direction of growth.
        int sideDir;
        ZoneBorder zoneBorder;
        if(side == Side.Top)
        {
            sideDir = -1;
            zoneBorder = _topCells;
        }
        else if(side == Side.Bottom)
        {
            sideDir = 1;
            zoneBorder = _bottomCells;
        }
        else
        {
            Debug.LogError($"Invalid side. {side}");
            return false;
        }


        // Assign the cells to the zone.
        for(int x = 0; x < zoneBorder.numberOfCells; x++)
        {
            cellsGrid.AssignCellToZone(zoneBorder.firstCellX + x, zoneBorder.firstCellY + sideDir, this);
        }


        // Update the sides descriptions.
        if(side == Side.Top)
        {
            _topCells.firstCellY--;
            _leftCells.firstCellY--;
            _rightCells.firstCellY--;
        }
        else // bottom
        {
            _bottomCells.firstCellY++;
        }
        _leftCells.numberOfCells++;
        _rightCells.numberOfCells++;


        return true;
    }
#endregion

#region Private Horizontal grow functions
    bool TryGrowHorizontally(CellsGrid cellsGrid, Side side)
    {
        if(SpaceToGrowHorizontally(cellsGrid, side) == 0)
        {
            return false;
        }

        return GrowHorizontally(cellsGrid, side);
    }

    int SpaceToGrowHorizontally(CellsGrid cellsGrid, Side side) // retorna quantas fileiras livres ha na direcção, 0 se não ha como expandir
    {
        if(_cells == null || _cells?.Count == 0)
        {
            Debug.LogError("No cells to grow.");
        }

        // Setup the direction of growth.
        int sideDir;
        ZoneBorder zoneBorder;
        if(side == Side.Left)
        {
            sideDir = -1;
            zoneBorder = _leftCells;
        }
        else if(side == Side.Right)
        {
            sideDir = 1;
            zoneBorder = _rightCells;
        }
        else
        {
            Debug.LogError($"Invalid side. {side}");
            return 0;
        }
        

        // Check if all cells on top of the top cells are available.
        for(int y = 0; y < zoneBorder.numberOfCells; y++)
        {
            if(cellsGrid.GetCell(zoneBorder.firstCellX + sideDir, zoneBorder.firstCellY + y, out Cell cell))
            {
                if(cell.Zone != _parentZone)
                {
                    return 0;
                }
            }
            else
            {
                // Invalid grid pos.
                return 0;
            }
        }

        return 1;
    }

    bool GrowHorizontally(CellsGrid cellsGrid, Side side) // unsafe, do a space to check before or use trygrowtop
    {
        if(_cells == null || _cells?.Count == 0)
        {
            Debug.LogError("No cells to grow.");
            return false;
        }

        // Setup the direction of growth.
        int sideDir;
        ZoneBorder zoneBorder;
        if(side == Side.Left)
        {
            sideDir = -1;
            zoneBorder = _leftCells;
        }
        else if(side == Side.Right)
        {
            sideDir = 1;
            zoneBorder = _rightCells;
        }
        else
        {
            Debug.LogError($"Invalid side. {side}");
            return false;
        }


        // Assign the cells to the zone.
        for(int y = 0; y < zoneBorder.numberOfCells; y++)
        {
            cellsGrid.AssignCellToZone(zoneBorder.firstCellX + sideDir, zoneBorder.firstCellY + y, this);
        }


        // Update the sides descriptions.
        if(side == Side.Left)
        {
            _leftCells.firstCellX--;
            _topCells.firstCellX--;
            _bottomCells.firstCellX--;
        }
        else // right
        {
            _rightCells.firstCellX++;
        }
        _topCells.numberOfCells++;
        _bottomCells.numberOfCells++;

        return true;
    }
#endregion

#region Public Top growth
    public bool TryGrowTop(CellsGrid cellsGrid)
    {
        return TryGrowVertically(cellsGrid, Side.Top);
    }

    public int SpaceToGrowTop(CellsGrid cellsGrid) // retorna quantas fileiras livres ha na direcção, 0 se não ha como expandir
    {
        return SpaceToGrowVertically(cellsGrid, Side.Top);
    }

    public bool GrowTop(CellsGrid cellsGrid) // unsafe, do a space to check before or use trygrowtop
    {
        return GrowVertically(cellsGrid, Side.Top);
    }
#endregion

#region Public Bottom growth
    public bool TryGrowBottom(CellsGrid cellsGrid)
    {
        return TryGrowVertically(cellsGrid, Side.Bottom);
    }

    public int SpaceToGrowBottom(CellsGrid cellsGrid) // retorna quantas fileiras livres ha na direcção, 0 se não ha como expandir
    {
        return SpaceToGrowVertically(cellsGrid, Side.Bottom);
    }

    public bool GrowBottom(CellsGrid cellsGrid) // unsafe, do a space to check before or use trygrowtop
    {
        return GrowVertically(cellsGrid, Side.Bottom);
    }
#endregion

#region Public Left growth
    public bool TryGrowLeft(CellsGrid cellsGrid)
    {
        return TryGrowHorizontally(cellsGrid, Side.Left);
    }

    public int SpaceToGrowLeft(CellsGrid cellsGrid) // retorna quantas fileiras livres ha na direcção, 0 se não ha como expandir
    {
        return SpaceToGrowHorizontally(cellsGrid, Side.Left);
    }

    public bool GrowLeft(CellsGrid cellsGrid) // unsafe, do a space to check before or use trygrowtop
    {
        return GrowHorizontally(cellsGrid, Side.Left);
    }
#endregion

#region Public Right growth
    public bool TryGrowRight(CellsGrid cellsGrid)
    {
        return TryGrowHorizontally(cellsGrid, Side.Right);
    }

    public int SpaceToGrowRight(CellsGrid cellsGrid) // retorna quantas fileiras livres ha na direcção, 0 se não ha como expandir
    {
        return SpaceToGrowHorizontally(cellsGrid, Side.Right);
    }

    public bool GrowRight(CellsGrid cellsGrid) // unsafe, do a space to check before or use trygrowtop
    {
        return GrowHorizontally(cellsGrid, Side.Right);
    }
#endregion

    public ZoneBorder GetLargestTopLine(CellsGrid cellsGrid)
    {
        int leftRightCount = 0;
        int rightLeftCount = 0;

        for(int x = 0; x < _topCells.numberOfCells; x++)
        {
            // Left-Right (->)
            if(cellsGrid.GetCell(_topCells.firstCellX + x, _topCells.firstCellY - 1, out Cell cell))
            {
                if(cell.Zone == _parentZone) // cell is free
                {
                    leftRightCount++;
                }
                else
                {
                    // first invalid cell
                    break;
                }
            }
            else
            {
                // Invalid grid pos.
                return default;
            }
        }

        for(int x = _topCells.numberOfCells - 1; x >= 0; x--)
        {
            // Right-Left (<-)
            if(cellsGrid.GetCell(_topCells.firstCellX + x, _topCells.firstCellY - 1, out Cell cell))
            {
                if(cell.Zone == _parentZone) // cell is free
                {
                    rightLeftCount++;
                }
                else
                {
                    // first invalid cell
                    break;
                }
            }
            else
            {
                // Invalid grid pos.
                return default;
            }
        }

        if(leftRightCount > rightLeftCount)
        {
            //Debug.LogError($"{_zoneId} left right {leftRightCount}, {_topCells.firstCellX} {_topCells.firstCellY}");
            return new ZoneBorder(_topCells.firstCellX, _topCells.firstCellY, leftRightCount);
        }
        else if(leftRightCount < rightLeftCount)
        {
            //Debug.LogError($"{_zoneId} right left {rightLeftCount}, {_topCells.firstCellX} {_topCells.firstCellY}");
            int first = _topCells.firstCellX + _topCells.numberOfCells - rightLeftCount;
            return new ZoneBorder(first, _topCells.firstCellY, rightLeftCount);
        }
        else // equal
        {
            //Debug.LogError($"{_zoneId} equal {leftRightCount} {rightLeftCount}, {_topCells.firstCellX} {_topCells.firstCellY}");
            return new ZoneBorder(_topCells.firstCellX, _topCells.firstCellY, leftRightCount);;
        }
    }

    public bool SetLBorder(CellsGrid cellsGrid)
    {
        int largestSide = 0;
        ZoneBorder border;

        foreach(Side side in Enum.GetValues(typeof(Side)))
        {
            border = GetLargestSideLine(cellsGrid, side);
            if(border.numberOfCells > largestSide)
            {
                _lBorderCells = border;
                _isLShaped = true;
                return true;
            }
        }

        return false; // cant grow in L.
    }

    public ZoneBorder GetLargestSideLine(CellsGrid cellsGrid, Side side)
    {
        int leftRightCount = 0;
        int rightLeftCount = 0;
        ZoneBorder zoneBorder;
        CoordModifier coordModifier;

        switch(side)
        {
            // X = i * coordMod_X_A + 1 * coordMod_X_B
            // Y = i * coordMod_Y_A + 1 * coordMod_Y_B
            case Side.Top: // zoneBorder.firstCellX + i, zoneBorder.firstCellY - 1
                zoneBorder = _topCells;
                coordModifier = new CoordModifier(1,0,0,-1);
                break;
            case Side.Bottom: // zoneBorder.firstCellX + i, zoneBorder.firstCellY + 1
                zoneBorder = _bottomCells;
                coordModifier = new CoordModifier(1,0,0,1);
                break;
            case Side.Left: // zoneBorder.firstCellX - 1, zoneBorder.firstCellY + i
                zoneBorder = _leftCells;
                coordModifier = new CoordModifier(0,-1,1,0);
                break;
            case Side.Right: // zoneBorder.firstCellX + 1, zoneBorder.firstCellY + i
                zoneBorder = _rightCells;
                coordModifier = new CoordModifier(0,1,1,0);
                break;
            default:
                zoneBorder = _topCells;
                coordModifier = new CoordModifier(1,0,0,-1);
                break;
        }

        // check from Left to Right(Top to botton)(> ^)
        for(int i = 0; i < zoneBorder.numberOfCells; i++)
        {
            if(cellsGrid.GetCell(zoneBorder.firstCellX + i*coordModifier.x_a + 1*coordModifier.x_b,
                                 zoneBorder.firstCellY + i*coordModifier.y_a + 1*coordModifier.y_b,
                                 out Cell cell))
            {
                if(cell.Zone == _parentZone) // cell is free
                {
                    leftRightCount++;
                }
                else // first invalid cell
                {
                    break;
                }
            }
            else // Invalid grid pos.
            {
                return default;
            }
        }

        // Check from Right to Left(Botton to Top)(< v)
        for(int i = zoneBorder.numberOfCells - 1; i >= 0; i--)
        {
            if(cellsGrid.GetCell(zoneBorder.firstCellX + i*coordModifier.x_a + 1*coordModifier.x_b,
                                 zoneBorder.firstCellY + i*coordModifier.y_a + 1*coordModifier.y_b,
                                 out Cell cell))
            {
                if(cell.Zone == _parentZone) // cell is free
                {
                    rightLeftCount++;
                }
                else // first invalid cell
                {
                    break;
                }
            }
            else // Invalid grid pos.
            {
                return default;
            }
        }

        // Return the largest cells sequence as ZoneBorder.
        if(leftRightCount > rightLeftCount)
        {
            return new ZoneBorder(zoneBorder.firstCellX, zoneBorder.firstCellY, leftRightCount, side);
        }
        else if(leftRightCount < rightLeftCount)
        {
            return new ZoneBorder(zoneBorder.firstCellX + (zoneBorder.numberOfCells - rightLeftCount)*coordModifier.x_a,
                                  zoneBorder.firstCellY + (zoneBorder.numberOfCells - rightLeftCount)*coordModifier.y_a,
                                  rightLeftCount,
                                  side);
        }
        else // equal
        {
            return new ZoneBorder(zoneBorder.firstCellX, zoneBorder.firstCellY, leftRightCount, side);
        }
    }

    public bool GrowLSide(CellsGrid cellsGrid)
    {
        if(!_isLShaped)
        {
            Debug.LogError("It's not L shaped.");
            return false;
        }

        switch(_lBorderCells.side)
        {
            case Side.Top:
                return TryGrowUp(cellsGrid, _lBorderCells);
            case Side.Bottom:
                return TryGrowUp(cellsGrid, _lBorderCells);
            case Side.Left:
                return TryGrowUp(cellsGrid, _lBorderCells);
            case Side.Right:
                return TryGrowUp(cellsGrid, _lBorderCells);
        }

        return false;
    }


    bool ZoneIsValid()
    {
        if(_cells == null || _cells?.Count == 0)
        {
            Debug.LogError("No cells to grow.");
            return false;
        }

        return true;
    }

    bool TryGrowUp(CellsGrid cellsGrid, ZoneBorder cellsToGrow)
    {
        if(SpaceToGrowUp(cellsGrid, cellsToGrow) > 0)
        {
            return GrowUp(cellsGrid, cellsToGrow);
        }
        
        return false;
    }

    int SpaceToGrowUp(CellsGrid cellsGrid, ZoneBorder cellsToGrow)
    {

        // Check if all cells on top of the top cells are available.
        for(int x = 0; x < cellsToGrow.numberOfCells; x++)
        {
            if(cellsGrid.GetCell(cellsToGrow.firstCellX + x, cellsToGrow.firstCellY - 1, out Cell cell))
            {
                if(cell.Zone != _parentZone)
                {
                    return 0;
                }
            }
            else
            {
                // Invalid grid pos.
                return 0;
            }
        }

        return 1;
    }

    bool GrowUp(CellsGrid cellsGrid, ZoneBorder cellsToGrow)
    {

        for(int x = 0; x < cellsToGrow.numberOfCells; x++)
        {
            cellsGrid.AssignCellToZone(cellsToGrow.firstCellX + x, cellsToGrow.firstCellY - 1, this);
        }

        _lBorderCells.firstCellY--;
        // In L grow all this can start to represent cells that are not in the zone.
        _leftCells.firstCellY--;
        _rightCells.firstCellY--;
        _leftCells.numberOfCells++;
        _rightCells.numberOfCells++;

        return true;
    }
}

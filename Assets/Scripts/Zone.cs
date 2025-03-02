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

// OBS: Seguindo o padão da grid, leitura esquerda->direita, cima->baixo
public class CellsLineDescription
{
    public Vector2Int firstCellCoord;
    public int numberOfCells;
    public Zone.Side side;

    public CellsLineDescription(int firstCellX, int firstCellY, int numberOfCells, Zone.Side side)
    {
        this.firstCellCoord = new Vector2Int(firstCellX, firstCellY);
        this.numberOfCells = numberOfCells;
        this.side = side;
    }

    public void AddCells(int amount)
    {
        numberOfCells += amount;
    }

    public void MoveUp(int amount)
    {
        firstCellCoord.y -= amount;
    }

    public void MoveDown(int amount)
    {
        firstCellCoord.y += amount;
    }

    public void MoveLeft(int amount)
    {
        firstCellCoord.x -= amount;
    }

    public void MoveRight(int amount)
    {
        firstCellCoord.x += amount;
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
    private CellsLineDescription _topCells;
    private CellsLineDescription _bottomCells;
    private CellsLineDescription _leftCells;
    private CellsLineDescription _rightCells;

    private float _desiredAspect = 1; // 1 is square
    public float DesiredAspect => _desiredAspect;

    private bool _isLShaped = false;
    public bool IsLShaped => _isLShaped;
    private CellsLineDescription _lBorderCells;
  

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
            _topCells = new CellsLineDescription(cell.GridPosition.x, cell.GridPosition.y, 1, Side.Top);
            _bottomCells = new CellsLineDescription(cell.GridPosition.x, cell.GridPosition.y, 1, Side.Bottom);
            _leftCells = new CellsLineDescription(cell.GridPosition.x, cell.GridPosition.y, 1, Side.Left);
            _rightCells = new CellsLineDescription(cell.GridPosition.x, cell.GridPosition.y, 1, Side.Right);
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
        CellsLineDescription zoneBorder;
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
            if(cellsGrid.GetCell(zoneBorder.firstCellCoord.x + x, zoneBorder.firstCellCoord.y + sideDir, out Cell cell))
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
        CellsLineDescription zoneBorder;
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
            cellsGrid.AssignCellToZone(zoneBorder.firstCellCoord.x + x, zoneBorder.firstCellCoord.y + sideDir, this);
        }


        // Update the sides descriptions.
        if(side == Side.Top)
        {
            _topCells.MoveUp(1);
            _leftCells.MoveUp(1);
            _rightCells.MoveUp(1);
        }
        else // bottom
        {
            _bottomCells.MoveDown(1);
        }
        _leftCells.AddCells(1);
        _rightCells.AddCells(1);


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
        CellsLineDescription zoneBorder;
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
            if(cellsGrid.GetCell(zoneBorder.firstCellCoord.x + sideDir, zoneBorder.firstCellCoord.y + y, out Cell cell))
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
        CellsLineDescription zoneBorder;
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
            cellsGrid.AssignCellToZone(zoneBorder.firstCellCoord.x + sideDir, zoneBorder.firstCellCoord.y + y, this);
        }


        // Update the sides descriptions.
        if(side == Side.Left)
        {
            _leftCells.MoveLeft(1);
            _topCells.MoveLeft(1);
            _bottomCells.MoveLeft(1);
        }
        else // right
        {
            _rightCells.MoveRight(1);
        }
        _topCells.AddCells(1);
        _bottomCells.AddCells(1);

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

    public CellsLineDescription GetLargestTopLine(CellsGrid cellsGrid)
    {
        int leftRightCount = 0;
        int rightLeftCount = 0;

        for(int x = 0; x < _topCells.numberOfCells; x++)
        {
            // Left-Right (->)
            if(cellsGrid.GetCell(_topCells.firstCellCoord.x + x, _topCells.firstCellCoord.y - 1, out Cell cell))
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
            if(cellsGrid.GetCell(_topCells.firstCellCoord.x + x, _topCells.firstCellCoord.y - 1, out Cell cell))
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
            return new CellsLineDescription(_topCells.firstCellCoord.x, _topCells.firstCellCoord.y, leftRightCount, Side.Top);
        }
        else if(leftRightCount < rightLeftCount)
        {
            //Debug.LogError($"{_zoneId} right left {rightLeftCount}, {_topCells.firstCellX} {_topCells.firstCellY}");
            int first = _topCells.firstCellCoord.x + _topCells.numberOfCells - rightLeftCount;
            return new CellsLineDescription(first, _topCells.firstCellCoord.y, rightLeftCount, Side.Top);
        }
        else // equal
        {
            //Debug.LogError($"{_zoneId} equal {leftRightCount} {rightLeftCount}, {_topCells.firstCellX} {_topCells.firstCellY}");
            return new CellsLineDescription(_topCells.firstCellCoord.x, _topCells.firstCellCoord.y, leftRightCount, Side.Top);;
        }
    }

    public bool SetLBorder(CellsGrid cellsGrid)
    {
        int largestSide = 0;
        CellsLineDescription border;

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

    public CellsLineDescription GetLargestSideLine(CellsGrid cellsGrid, Side side)
    {
        // TODO: Se o tamanho do seguimento for igual ao tamanho total do lado n precisa checar o outro lado
        int leftRightCount = 0;
        int rightLeftCount = 0;
        CellsLineDescription zoneBorder;
        Vector4 m; // Transformation matrix


        // criar zona resultado
        // modificar em execução, simplifica o retorno


        
        //[mx][mz] * [i]
        //[my][mw]   [1]
        // x = i * mx + 1 * mz
        // y = I * my + 1 * mw
        // (direcao das celulas em X, direcao das celulas em Y, direcao de expansão em X, direcao de expansao em Y)
        // T = (1,0,0,-1)
        // B = (1,0,0,1)
        // L = (0,1,-1,0)
        // R = (0,1,1,0)

        switch(side)
        {
            case Side.Top:
                zoneBorder = _topCells;
                m = new Vector4(1,0,0,-1);
                break;
            case Side.Bottom:
                zoneBorder = _bottomCells;
                m = new Vector4(1,0,0,1);
                break;
            case Side.Left:
                zoneBorder = _leftCells;
                m = new Vector4(0,1,-1,0);
                break;
            case Side.Right:
                zoneBorder = _rightCells;
                m = new Vector4(0,1,1,0);
                break;
            default:
                zoneBorder = _topCells;
                m = new Vector4(1,0,0,-1);
                break;
        }

        // check from Left to Right(Top to botton)(> ^)
        for(int i = 0; i < zoneBorder.numberOfCells; i++)
        {
            if(cellsGrid.GetCell(zoneBorder.firstCellCoord.x + i * (int)m.x + (int)m.z,
                                 zoneBorder.firstCellCoord.y + i * (int)m.y + (int)m.w,
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

        // TODO: if have a return value ready, can just return from the if to skip the for loop.
        // If the line has the maximum side, don't need to check the other direction.
        if(leftRightCount != zoneBorder.numberOfCells)
        {
            // Check from Right to Left(Botton to Top)(< v)
            for(int i = zoneBorder.numberOfCells - 1; i >= 0; i--)
            {
                if(cellsGrid.GetCell(zoneBorder.firstCellCoord.x + i * (int)m.x + (int)m.z,
                                     zoneBorder.firstCellCoord.y + i * (int)m.y + (int)m.w,
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
        }

        // Return the largest cells sequence as ZoneBorder.
        if(leftRightCount > rightLeftCount)
        {
            return new CellsLineDescription(zoneBorder.firstCellCoord.x, zoneBorder.firstCellCoord.y, leftRightCount, side);
        }
        else if(leftRightCount < rightLeftCount)
        {
            return new CellsLineDescription(zoneBorder.firstCellCoord.x + (zoneBorder.numberOfCells - rightLeftCount) * (int)m.x,
                                            zoneBorder.firstCellCoord.y + (zoneBorder.numberOfCells - rightLeftCount) * (int)m.y,
                                            rightLeftCount,
                                            side);
        }
        else // equal
        {
            return new CellsLineDescription(zoneBorder.firstCellCoord.x, zoneBorder.firstCellCoord.y, leftRightCount, side);
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

    bool TryGrowUp(CellsGrid cellsGrid, CellsLineDescription cellsToGrow)
    {
        if(SpaceToGrowUp(cellsGrid, cellsToGrow) > 0)
        {
            return GrowUp(cellsGrid, cellsToGrow);
        }
        
        return false;
    }

    int SpaceToGrowUp(CellsGrid cellsGrid, CellsLineDescription cellsToGrow)
    {

        // Check if all cells on top of the top cells are available.
        for(int x = 0; x < cellsToGrow.numberOfCells; x++)
        {
            if(cellsGrid.GetCell(cellsToGrow.firstCellCoord.x + x, cellsToGrow.firstCellCoord.y - 1, out Cell cell))
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

    bool GrowUp(CellsGrid cellsGrid, CellsLineDescription cellsToGrow)
    {

        for(int x = 0; x < cellsToGrow.numberOfCells; x++)
        {
            cellsGrid.AssignCellToZone(cellsToGrow.firstCellCoord.x + x, cellsToGrow.firstCellCoord.y - 1, this);
        }

        _lBorderCells.MoveUp(1);
        // In L grow all this can start to represent cells that are not in the zone.
        _leftCells.MoveUp(1);
        _rightCells.MoveUp(1);
        _leftCells.AddCells(1);
        _rightCells.AddCells(1);

        return true;
    }

    // GetUpperLine(bool findTheFarthestLine) retorna a maior linha de cima, pode ser a linha completa ou parcial vindo da direita ou esquerda
    // while(get line valida){GetUpperLine()} permite contar o numero linhas completas vagas
    // pode ser retornada uma tupla ou apenas a ref da linha. {Classe.IsFullLine, Class.Line, Class.Distance}


    public void Print_GetExpansionSpace_ForAllSides(CellsGrid cellsGrid)
    {
        Debug.Log("-----------------------------------------------------------------------------");
        var a = GetExpansionSpace(_topCells, cellsGrid, true);
        Debug.Log($"zid:{ZoneId} side:{a.line.side} space:{a.space} cells:{a.line.numberOfCells}");
        a = GetExpansionSpace(_bottomCells, cellsGrid, true);
        Debug.Log($"zid:{ZoneId} side:{a.line.side} space:{a.space} cells:{a.line.numberOfCells}");
        a = GetExpansionSpace(_leftCells, cellsGrid, true);
        Debug.Log($"zid:{ZoneId} side:{a.line.side} space:{a.space} cells:{a.line.numberOfCells}");
        a = GetExpansionSpace(_rightCells, cellsGrid, true);
        Debug.Log($"zid:{ZoneId} side:{a.line.side} space:{a.space} cells:{a.line.numberOfCells}");
    }

    // TODO: talvez checar expansão siomples nos 2 sentidos prieiro e depois checar a profundidade
    // TODO: ao inves de ter retornos em varios pontos, guardar resulados de cada avaliação para comparar no final
    public (CellsLineDescription line, bool isFullLine, int space) GetExpansionSpace(CellsLineDescription cellsLineDesc, CellsGrid cellsGrid, bool fullSpaceSearch = false)
    {
        // TODO: Se o tamanho do seguimento for igual ao tamanho total do lado n precisa checar o outro lado
        int leftRightCount = 0;
        int rightLeftCount = 0;
        int maxExpansionSpace = 0;
        CellsLineDescription freeLine = new CellsLineDescription(cellsLineDesc.firstCellCoord.x, cellsLineDesc.firstCellCoord.y, 0, cellsLineDesc.side);
        Vector4 m; // Transformation matrix
        
        //[mx][mz] * [i]
        //[my][mw]   [1]
        // x = i * mx + 1 * mz
        // y = I * my + 1 * mw
        // (x: direction of cells in X,
        //  y: direction of cells in Y,
        //  z: direction of expansion in X,
        //  w: direction of expansion in Y)

        switch(cellsLineDesc.side)
        {
            case Side.Top:
                m = new Vector4(1,0,0,-1);
                break;
            case Side.Bottom:
                m = new Vector4(1,0,0,1);
                break;
            case Side.Left:
                m = new Vector4(0,1,-1,0);
                break;
            case Side.Right:
                m = new Vector4(0,1,1,0);
                break;
            default:
                m = new Vector4(1,0,0,-1);
                break;
        }


        // =================== check from Left to Right(Top to botton)(> ^)
        for(int i = 0; i < cellsLineDesc.numberOfCells; i++)
        {
            if(cellsGrid.GetCell(cellsLineDesc.firstCellCoord.x + i * (int)m.x + (int)m.z,
                                 cellsLineDesc.firstCellCoord.y + i * (int)m.y + (int)m.w,
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
                return (freeLine, false, 0);
            }
        }

        if(leftRightCount > 0)
        {
            freeLine.numberOfCells = leftRightCount;
            maxExpansionSpace = 1; // Fisrt cell can be valid(exist on grid) but is from another parent zone.
        }

        if(fullSpaceSearch && freeLine.numberOfCells > 0)
        {
            int currentLineNumCells;
            bool done = false;
            
            while(!done)
            {
                currentLineNumCells = 0;

                for(int i = 0; i < freeLine.numberOfCells; i++)
                {
                    if(cellsGrid.GetCell(cellsLineDesc.firstCellCoord.x + i * (int)m.x + (maxExpansionSpace + 1)*(int)m.z,
                                         cellsLineDesc.firstCellCoord.y + i * (int)m.y + (maxExpansionSpace + 1)*(int)m.w,
                                         out Cell cell))
                    {
                        if(cell.Zone == _parentZone) // cell is free
                        {
                            currentLineNumCells++;
                        }
                        else // first invalid cell, can try next line on next iteration.
                        {
                            break;
                        }
                    }
                    else // Invalid grid pos. Sure to not having more cells on this direction.
                    {
                        done = true;
                        break;
                    }
                }

                if(currentLineNumCells == freeLine.numberOfCells)
                {
                    maxExpansionSpace++;
                }
                else
                {
                    done = true;
                    //break;
                }
            }
        }


        // Poderia ser ignorado se for levado em conta uma area total maior se crescer mais longe sendo mais curto
        if(leftRightCount == cellsLineDesc.numberOfCells)
        {
            return (freeLine, true, maxExpansionSpace);
        }
        // else is a partial side, check if the other is larger

        // ================  Check from Right to Left(Botton to Top)(< v)
        for(int i = cellsLineDesc.numberOfCells - 1; i >= 0; i--)
        {
            if(cellsGrid.GetCell(cellsLineDesc.firstCellCoord.x + i * (int)m.x + (int)m.z,
                                 cellsLineDesc.firstCellCoord.y + i * (int)m.y + (int)m.w,
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
                // OBS: PROVAVELMENTE N NESCESSARIO, SE O LOOP ANTERIOR DETECTAR NÃO CHECA AQUI
                return (freeLine, false, 0);
            }
        }

        // From right to left is smaller so return the previous result.
        if(rightLeftCount <= leftRightCount)
        {
            return (freeLine, false, maxExpansionSpace);
        }
        // else, continue.

        freeLine.numberOfCells = rightLeftCount;
        maxExpansionSpace = 1;

        if(fullSpaceSearch && freeLine.numberOfCells > 0)
        {
            int currentLineNumCells;
            bool done = false;
            
            while(!done)
            {
                currentLineNumCells = 0;

                for(int i = cellsLineDesc.numberOfCells - 1; i >= cellsLineDesc.numberOfCells - freeLine.numberOfCells; i--)
                {
                    Cell cell;
                    if(cellsGrid.GetCell(cellsLineDesc.firstCellCoord.x + i * (int)m.x + (maxExpansionSpace + 1)*(int)m.z,
                                         cellsLineDesc.firstCellCoord.y + i * (int)m.y + (maxExpansionSpace + 1)*(int)m.w,
                                         out cell))
                    {
                        if(cell.Zone == _parentZone) // cell is free
                        {
                            currentLineNumCells++;
                        }
                        else // first invalid cell, can try next line on next iteration.
                        {
                            break;
                        }
                    }
                    else // Invalid grid pos. Sure to not having more cells on this direction.
                    {
                        done = true;
                        break;
                    }
                }

                if(currentLineNumCells == freeLine.numberOfCells)
                {
                    maxExpansionSpace++;
                }
                else
                {
                    done = true;
                    //break;
                }
            }
        }
        
        return (freeLine, false, maxExpansionSpace);
    }
}

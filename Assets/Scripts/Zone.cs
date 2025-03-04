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
    public CellsLineDescription _lBorderCells;

    private readonly Vector4 _TOP_MATRIX = new Vector4(1,0,0,-1);
    private readonly Vector4 _BOTTOM_MATRIX = new Vector4(1,0,0,1);
    private readonly Vector4 _LEFT_MATRIX = new Vector4(0,1,-1,0);
    private readonly Vector4 _RIGHT_MATRIX = new Vector4(0,1,1,0);
  

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

    public bool AutoSetLBorder(CellsGrid cellsGrid)
    {
        if(_isLShaped)
        {
            Debug.LogError("Can't redefine L border. A new method or modification is needed.");
            return false;
        }

        CellsLineDescription largestFreeSide = null;

        foreach(Side side in Enum.GetValues(typeof(Side)))
        {
            CellsLineDescription newSide = GetExpansionSpace(side, cellsGrid).line;

            if(newSide != null)
            {
                if(largestFreeSide == null)
                {
                    largestFreeSide = newSide;
                }
                else if(newSide.numberOfCells > largestFreeSide.numberOfCells)
                {
                    largestFreeSide = newSide;
                }
            }
        }

        if(largestFreeSide != null)
        {
            _lBorderCells = largestFreeSide;
            _isLShaped = true;
            return true;
        }

        return false; // cant grow in L.
    }

    // GetUpperLine(bool findTheFarthestLine) retorna a maior linha de cima, pode ser a linha completa ou parcial vindo da direita ou esquerda
    // while(get line valida){GetUpperLine()} permite contar o numero linhas completas vagas
    // pode ser retornada uma tupla ou apenas a ref da linha. {Classe.IsFullLine, Class.Line, Class.Distance}


    public void Debug_Print_GetExpansionSpace_ForAllSides(CellsGrid cellsGrid)
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

    public (CellsLineDescription line, bool isFullLine, int space) GetExpansionSpace(Side side, CellsGrid cellsGrid, bool fullSpaceSearch = false)
    {
        switch(side)
        {
            case Side.Top:
                return GetExpansionSpace(_topCells, cellsGrid, fullSpaceSearch);
            case Side.Bottom:
                return GetExpansionSpace(_bottomCells, cellsGrid, fullSpaceSearch);
            case Side.Left:
                return GetExpansionSpace(_leftCells, cellsGrid, fullSpaceSearch);
            case Side.Right:
                return GetExpansionSpace(_rightCells, cellsGrid, fullSpaceSearch);
            default:
                Debug.LogError($"Invalid side: {side}");
                return default;
        }
    }

    public (CellsLineDescription line, bool isFullLine, int space) GetLargestExpandableSide(CellsGrid cellsGrid, bool fullSpaceSearch = false)
    {
        (CellsLineDescription line, bool isFullLine, int space) largestFreeSide = (null, false, 0);

        foreach(Side side in Enum.GetValues(typeof(Side)))
        {
            var newSide = GetExpansionSpace(side, cellsGrid, fullSpaceSearch);

            if(newSide.line != null)
            {
                if(largestFreeSide.line == null || newSide.line.numberOfCells > largestFreeSide.line?.numberOfCells)
                {
                    largestFreeSide = newSide;
                }
            }
        }

        return largestFreeSide;
    }

    // TODO: talvez checar expansão simples nos 2 sentidos prieiro e depois checar a profundidade
    // TODO: ao inves de ter retornos em varios pontos, guardar resulados de cada avaliação para comparar no final
    public (CellsLineDescription line, bool isFullLine, int space) GetExpansionSpace(CellsLineDescription cellsLineDesc, CellsGrid cellsGrid, bool fullSpaceSearch = false)
    {
        if(cellsLineDesc.numberOfCells == 0)
        {
            return (null, false, 0);
        }

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
                m = _TOP_MATRIX;
                break;
            case Side.Bottom:
                m = _BOTTOM_MATRIX;
                break;
            case Side.Left:
                m = _LEFT_MATRIX;
                break;
            case Side.Right:
                m = _RIGHT_MATRIX;
                break;
            default:
                m = _TOP_MATRIX;
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
                return (null, false, 0);
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
                return (null, false, 0);
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

    public bool TryExpand(CellsLineDescription cellsLineDesc, CellsGrid cellsGrid)
    {
        int amount = 1;
        Vector4 m; // Transformation matrix
        
        switch(cellsLineDesc.side)
        {
            case Side.Top:
                m = _TOP_MATRIX;
                break;
            case Side.Bottom:
                m = _BOTTOM_MATRIX;
                break;
            case Side.Left:
                m = _LEFT_MATRIX;
                break;
            case Side.Right:
                m = _RIGHT_MATRIX;
                break;
            default:
                m = _TOP_MATRIX;
                break;
        }


        // Assign the cells to the zone.
        for(int i = 0; i < cellsLineDesc.numberOfCells; i++)
        {
            cellsGrid.AssignCellToZone(cellsLineDesc.firstCellCoord.x + i * (int)m.x + (int)m.z,
                                       cellsLineDesc.firstCellCoord.y + i * (int)m.y + (int)m.w,
                                       this);
        }


        switch(cellsLineDesc.side)
        {
            case Side.Top:
                if(_isLShaped) _lBorderCells.MoveUp(amount);
                _topCells.MoveUp(amount);
                _leftCells.MoveUp(amount);
                _rightCells.MoveUp(amount);
                _leftCells.AddCells(amount);
                _rightCells.AddCells(amount);
                break;
            case Side.Bottom:
                if(_isLShaped) _lBorderCells.MoveDown(amount);
                _bottomCells.MoveDown(amount);
                _leftCells.AddCells(amount);
                _rightCells.AddCells(amount);
                break;
            case Side.Left:
                if(_isLShaped) _lBorderCells.MoveLeft(amount);
                _leftCells.MoveLeft(amount);
                _topCells.MoveLeft(amount);
                _bottomCells.MoveLeft(amount);
                _topCells.AddCells(amount);
                _bottomCells.AddCells(amount);
                break;
            case Side.Right:
                if(_isLShaped) _lBorderCells.MoveRight(amount);
                _rightCells.MoveRight(amount);
                _topCells.AddCells(amount);
                _bottomCells.AddCells(amount);
                break;
            default: // Top
                if(_isLShaped) _lBorderCells.MoveUp(amount);
                _topCells.MoveUp(amount);
                _leftCells.MoveUp(amount);
                _rightCells.MoveUp(amount);
                _leftCells.AddCells(amount);
                _rightCells.AddCells(amount);
                break;
        }

        return true;
    }

    public bool CheckSpaceAndExpand(Side side, CellsGrid cellsGrid)
    {
        CellsLineDescription cellsLineDesc;

        if(_isLShaped)
        {
            cellsLineDesc = _lBorderCells;
        }
        else
        {
            switch(side)
            {
                case Side.Top:
                    cellsLineDesc = _topCells;
                    break;
                case Side.Bottom:
                    cellsLineDesc = _bottomCells;
                    break;
                case Side.Left:
                    cellsLineDesc = _leftCells;
                    break;
                case Side.Right:
                    cellsLineDesc = _rightCells;
                    break;
                default:
                    cellsLineDesc = _topCells;
                    break;
            }
        }

        if(GetExpansionSpace(cellsLineDesc, cellsGrid, false).isFullLine)
        {
            return TryExpand(cellsLineDesc, cellsGrid);
        }
        else
        {
            return false;
        }
    }

    public bool ExpandLShape(CellsGrid cellsGrid)
    {
        if(GetExpansionSpace(_lBorderCells, cellsGrid, false).isFullLine)
        {
            if(TryExpand(_lBorderCells, cellsGrid))
            {
                return true;
            }
        }
        
        return false;
    }
}

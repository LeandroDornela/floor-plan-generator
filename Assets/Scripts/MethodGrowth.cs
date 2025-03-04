using UnityEngine;
using System.Threading;
using UnityEditor;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System;
using System.Linq;
using AYellowpaper.SerializedCollections.Editor.Data;

public class MethodGrowth : FPGenerationMethod
{
    private CancellationTokenSource _cts;
    private bool _done = false;
    Zone _currentZone;
    List<Zone> zonesToSubdivide; // TODO: QUEUE
    List<Zone> zonesToGrow;
    // quando uma zona n pode mais crescer na iteração atual é armazenada aqui.
    // Depois retorna para crescer usando outra logical, 'L' ou 'free'
    List<Zone> grownZones;
    
    private float delay = 0.01f;

    private CellsLineDescription _zoneBorder_TEMP;

   

    public override bool Init(FloorPlanManager floorPlanManager)
    {
        base.Init(floorPlanManager);

        _cts = new CancellationTokenSource();


        return _initialized;
    }

    // TODO: receber a grid e tudo mais como parametros
    public override async UniTask<bool> Run()
    {
        if(!EditorApplication.isPlaying)
        {
            Debug.LogError("Don't use it outside play mode.");
            return false;
        }

        EditorApplication.playModeStateChanged += PlayModeStateChanged;

        zonesToSubdivide = new List<Zone>();
        zonesToGrow = new List<Zone>();
        grownZones = new List<Zone>();

        // TODO: temporario.
        // Setup da zona raiz.
        foreach(Cell cell in _floorPlanManager.CellsGrid._cells)
        {
            _floorPlanManager.CellsGrid.AssignCellToZone(cell.GridPosition.x, cell.GridPosition.y, _floorPlanManager.RootZone);
            //TriggerOnCellsGridChanged(_floorPlanManager.CellsGrid);
            //await UniTask.WaitForSeconds(0.005f, cancellationToken: _cts.Token);
        }

        zonesToSubdivide.Add(_floorPlanManager.RootZone);
        
        while(zonesToSubdivide.Count > 0) // A CADA EXECUÇÃO FAZ A DIVISÃO DE UMA ZONA.
        {
            // Get the child zones from the next zone to subdivide.
            zonesToGrow = GetNextZonesToGrowList();

            //TriggerOnCellsGridChanged(_floorPlanManager.CellsGrid);
            //await UniTask.WaitForSeconds(delay, cancellationToken: _cts.Token);

            // ==================== begin main grow logic
            // ==================================================================================== LOOP CRESCIMENTO RECT
            while(zonesToGrow.Count > 0)
            {
                _currentZone = GetNextZone(zonesToGrow);

                if(!GrowZoneRect(_currentZone, _floorPlanManager.CellsGrid))
                {
                    zonesToGrow.Remove(_currentZone);
                    grownZones.Add(_currentZone);
                }

                //TriggerOnCellsGridChanged(_floorPlanManager.CellsGrid);
                //await UniTask.WaitForSeconds(delay, cancellationToken: _cts.Token);
            }

            zonesToGrow = new List<Zone>(grownZones);
            grownZones.Clear();
            //await UniTask.WaitForSeconds(delay, cancellationToken: _cts.Token);
            
            // ==================================================================================== LOOP L
            /*
            while(zonesToGrow.Count > 0)
            {
                _currentZone = GetNextZone(zonesToGrow);

                if(!TryGrowFree(_currentZone, _floorPlanManager.CellsGrid))
                {
                    zonesToGrow.Remove(_currentZone);
                    grownZones.Add(_currentZone);
                }

                TriggerOnCellsGridChanged(_floorPlanManager.CellsGrid);
                await UniTask.WaitForSeconds(delay, cancellationToken: _cts.Token);
            }

            zonesToGrow = new List<Zone>(grownZones);
            grownZones.Clear();
            await UniTask.WaitForSeconds(delay, cancellationToken: _cts.Token);
            */
            // LOOP CRESCIMENTO L
            while(zonesToGrow.Count > 0)
            {
                _currentZone = GetNextZone(zonesToGrow);
                /*
                _zoneBorder_TEMP = _currentZone.GetExpansionSpace(Zone.Side.Top, _floorPlanManager.CellsGrid).line;
                if(_zoneBorder_TEMP.numberOfCells > 0) await UniTask.WaitForSeconds(0.5f, cancellationToken: _cts.Token);
                _zoneBorder_TEMP = _currentZone.GetExpansionSpace(Zone.Side.Bottom, _floorPlanManager.CellsGrid).line;
                if(_zoneBorder_TEMP.numberOfCells > 0) await UniTask.WaitForSeconds(0.5f, cancellationToken: _cts.Token);
                _zoneBorder_TEMP = _currentZone.GetExpansionSpace(Zone.Side.Left, _floorPlanManager.CellsGrid).line;
                if(_zoneBorder_TEMP.numberOfCells > 0) await UniTask.WaitForSeconds(0.5f, cancellationToken: _cts.Token);
                _zoneBorder_TEMP = _currentZone.GetExpansionSpace(Zone.Side.Right, _floorPlanManager.CellsGrid).line;
                if(_zoneBorder_TEMP.numberOfCells > 0) await UniTask.WaitForSeconds(0.5f, cancellationToken: _cts.Token);
                */
                /*
                if(!_currentZone.IsLShaped)
                {
                    if(!_currentZone.AutoSetLBorder(_floorPlanManager.CellsGrid))
                    {
                        zonesToGrow.Remove(_currentZone);
                        grownZones.Add(_currentZone);
                    }
                }
                */
                
                if(!GrowZoneLShape(_currentZone, _floorPlanManager.CellsGrid))
                {
                    zonesToGrow.Remove(_currentZone);
                    grownZones.Add(_currentZone);
                }

                //TriggerOnCellsGridChanged(_floorPlanManager.CellsGrid);
                //await UniTask.WaitForSeconds(delay, cancellationToken: _cts.Token);
            }
            // CRESCIMENTO LIVRE(espaços restantes)
            // while free spaces.
            // ==================================================================================== end main grow logic

            // Prepare the next set of zones to grow.
            foreach(Zone zone in grownZones)
            {
                if(zone._childZones?.Count > 0)
                {
                    zonesToSubdivide.Add(zone);
                }
            }

            grownZones.Clear();
        }
        

        EditorApplication.playModeStateChanged -= PlayModeStateChanged;

        TriggerOnCellsGridChanged(_floorPlanManager.CellsGrid);
        await UniTask.WaitForSeconds(delay, cancellationToken: _cts.Token);

        return true;
    }

    List<Zone> GetNextZonesToGrowList() // TODO: "set" zones to grow list
    {
        if(zonesToSubdivide.Count == 0)
        {
            return null;
        }

        var childZones = zonesToSubdivide[0]._childZones;
        zonesToSubdivide.RemoveAt(0);

        foreach(var zone in childZones)
        {
            PlotFirstZoneCell(zone);
        }

        return  childZones;
    }


    void PlotFirstZoneCell(Zone zone)
    {
        if(zone._parentZone != null)
        {
            int index = Utils.RandomRange(0, zone._parentZone._cells.Count);
            var cell = zone._parentZone._cells[index];
            _floorPlanManager.CellsGrid.AssignCellToZone(cell.GridPosition.x, cell.GridPosition.y, zone);
        }
        else
        {
            Vector2Int position = new Vector2Int(Utils.RandomRange(0, _floorPlanManager.CellsGrid.Dimmensions.x),
                                                 Utils.RandomRange(0, _floorPlanManager.CellsGrid.Dimmensions.y));
            
            _floorPlanManager.CellsGrid.AssignCellToZone(position.x, position.y, zone);
        }
    }


    Zone GetNextZone(List<Zone> zonesToGrow)
    {
        return zonesToGrow[Utils.RandomRange(0, zonesToGrow.Count)];
    }


    bool GrowZoneRect(Zone zone, CellsGrid cellsGrid)
    {
        /*
        if(TryGrowToDirection(Zone.Side.Right, zone, cellsGrid))
            return true;
        if(TryGrowToDirection(Zone.Side.Left, zone, cellsGrid))
            return true;
        if(TryGrowToDirection(Zone.Side.Top, zone, cellsGrid))
            return true;
        if(TryGrowToDirection(Zone.Side.Bottom, zone, cellsGrid))
            return true;
        return false;
        */

        float aspect = zone.GetZoneAspect();

        // Guid.NewGuid() provides a way to get unique randon numbers. Create a array with the directions
        // and sort it using the guids.
        var directions = Enum.GetValues(typeof(Zone.Side)).Cast<Zone.Side>().OrderBy(d => Guid.NewGuid());

        foreach(var direction in directions)
        {
            // All directions
            if(aspect == zone.DesiredAspect)
            {
                if(TryGrowToDirection(direction, zone, cellsGrid))
                {
                    return true;
                }
            }
            // Vertical
            else if(aspect > zone.DesiredAspect && (direction == Zone.Side.Top || direction == Zone.Side.Bottom))
            {
                if(TryGrowToDirection(direction, zone, cellsGrid))
                {
                    return true;
                }
            }
            // Horizontal
            else if(aspect < zone.DesiredAspect && (direction == Zone.Side.Left || direction == Zone.Side.Right))
            {
                if(TryGrowToDirection(direction, zone, cellsGrid))
                {
                    return true;
                }
            }
        }

        return false; // can't grow.
    }

    bool GrowZoneLShape(Zone zone, CellsGrid cellsGrid)
    {
        /*
        var directions = Enum.GetValues(typeof(Zone.Side)).Cast<Zone.Side>().OrderBy(d => Guid.NewGuid());

        foreach(var direction in directions)
        {

        }
        */

        if(zone.IsLShaped)
        {
            return zone.ExpandLShape(cellsGrid);
        }
        else
        {
            var side = zone.GetLargestExpandableSide(cellsGrid);

            if(side.isFullLine)
            {
                return zone.TryExpand(side.line, cellsGrid);
            }
            else
            {
                if(zone.AutoSetLBorder(cellsGrid))
                    return zone.ExpandLShape(cellsGrid);
                return false;
            }
        }

        if(zone.IsLShaped)
        {
            return zone.ExpandLShape(cellsGrid);
        }
        else
        {
            if(!TryGrowFree(zone, cellsGrid))
            {
                if(zone.AutoSetLBorder(cellsGrid))
                    return zone.ExpandLShape(cellsGrid);
            }
            else
            {
                return true;
            }
        }

        return false;
    }

    bool TryGrowFree(Zone zone, CellsGrid cellsGrid)
    {
        foreach(Zone.Side side in Enum.GetValues(typeof(Zone.Side)))
        {
            var result = zone.GetExpansionSpace(side, cellsGrid);
            if(result.isFullLine && result.space > 0)
            {
                return zone.TryExpand(result.line, cellsGrid);
            }
        }

        return false;
    }

    // OBS: the direction paramenter is taking in mind a randomization of the order of try.
    bool TryGrowToDirection(Zone.Side direction, Zone zone, CellsGrid cellsGrid)
    {
        return zone.CheckSpaceAndExpand(direction, cellsGrid);
    }


    void PlayModeStateChanged(PlayModeStateChange state)
    {
        if(state == PlayModeStateChange.ExitingPlayMode)
        {
            _cts.Cancel();
        }
    }

    public override void OnDrawGizmos()
    {
        Vector3 from = new Vector3();
        Vector3 to = new Vector3();
        CellsLineDescription zb = _zoneBorder_TEMP;
        float os = 0.5f;
        
        if(_zoneBorder_TEMP != null)
        switch(_zoneBorder_TEMP.side)
        {
            case Zone.Side.Top:
            from = new Vector3(zb.firstCellCoord.x - os,
                              1,
                              -zb.firstCellCoord.y);
            to = new Vector3(zb.firstCellCoord.x + os + zb.numberOfCells - 1,
                             1,
                             -zb.firstCellCoord.y);
            break;
            case Zone.Side.Bottom:
            from = new Vector3(zb.firstCellCoord.x - os,
                              1,
                              -zb.firstCellCoord.y);
            to = new Vector3(zb.firstCellCoord.x + os + zb.numberOfCells - 1,
                             1,
                             -zb.firstCellCoord.y);
            break;
            case Zone.Side.Left:
            from = new Vector3(zb.firstCellCoord.x,
                              1,
                              -(zb.firstCellCoord.y - os));
            to = new Vector3(zb.firstCellCoord.x,
                             1,
                             -(zb.firstCellCoord.y + os + zb.numberOfCells - 1));
            break;
            case Zone.Side.Right:
            from = new Vector3(zb.firstCellCoord.x,
                              1,
                              -(zb.firstCellCoord.y - os));
            to = new Vector3(zb.firstCellCoord.x,
                             1,
                             -(zb.firstCellCoord.y + os + zb.numberOfCells - 1));
            break;
        }
        /*
        switch(_zoneBorder_TEMP.side)
        {
            case Zone.Side.Top:
            from = new Vector3(zb.firstCellCoord.x - os,
                              1,
                              -zb.firstCellCoord.y);
            to = new Vector3(zb.firstCellCoord.x + os + zb.numberOfCells - 1,
                             1,
                             -zb.firstCellCoord.y);
            break;
            case Zone.Side.Bottom:
            from = new Vector3(zb.firstCellCoord.x - os,
                              1,
                              -zb.firstCellCoord.y);
            to = new Vector3(zb.firstCellCoord.x + os + zb.numberOfCells - 1,
                             1,
                             -zb.firstCellCoord.y);
            break;
            case Zone.Side.Left:
            from = new Vector3(zb.firstCellCoord.x,
                              1,
                              -(zb.firstCellCoord.y - os));
            to = new Vector3(zb.firstCellCoord.x,
                             1,
                             -(zb.firstCellCoord.y + os + zb.numberOfCells - 1));
            break;
            case Zone.Side.Right:
            from = new Vector3(zb.firstCellCoord.x,
                              1,
                              -(zb.firstCellCoord.y - os));
            to = new Vector3(zb.firstCellCoord.x,
                             1,
                             -(zb.firstCellCoord.y + os + zb.numberOfCells - 1));
            break;
        }
        */
        Gizmos.color = Color.white;
        Gizmos.DrawLine(from, to);


        if(_floorPlanManager.CellsGrid._cells != null)
        foreach(var cell in _floorPlanManager.CellsGrid._cells)
        {
            Handles.Label(new Vector3(cell.GridPosition.x - os, 1, -cell.GridPosition.y + os), $"[{cell.GridPosition.x}, {cell.GridPosition.y}]");
        }

        foreach(var zone in _floorPlanManager.ZonesInstances)
        {
            if(zone.Value.IsLShaped)
            {
                Gizmos.color = Color.white;
                var pos = new Vector3(zone.Value._lBorderCells.firstCellCoord.x, 1, -zone.Value._lBorderCells.firstCellCoord.y);
                Gizmos.DrawWireSphere(pos + new Vector3(0,-1,0), 0.25f);

                GUIStyle style = new GUIStyle();
                style.normal.textColor = Color.black;
                Handles.Label(pos, $"{zone.Value._lBorderCells.numberOfCells} on {zone.Value._lBorderCells.side}", style);
            }
        }
        
    }
}

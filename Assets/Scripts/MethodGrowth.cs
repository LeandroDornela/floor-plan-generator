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
    
    private float delay = 0.005f;

    private CellsLineDescription _zoneBorder_TEMP;

   

    public override bool Init(FloorPlanManager floorPlanManager)
    {
        base.Init(floorPlanManager);

        _cts = new CancellationTokenSource();


        return _initialized;
    }


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

            TriggerOnCellsGridChanged(_floorPlanManager.CellsGrid);
            await UniTask.WaitForSeconds(delay, cancellationToken: _cts.Token);

            // ==================== begin main grow logic
            // LOOP CRESCIMENTO RECT
            while(zonesToGrow.Count > 0)
            {
                _currentZone = GetNextZone(zonesToGrow);

                if(!GrowZoneRect(_currentZone, _floorPlanManager.CellsGrid))
                {
                    zonesToGrow.Remove(_currentZone);
                    grownZones.Add(_currentZone);
                }

                TriggerOnCellsGridChanged(_floorPlanManager.CellsGrid);
                await UniTask.WaitForSeconds(delay, cancellationToken: _cts.Token);
            }

            zonesToGrow = new List<Zone>(grownZones);
            //grownZones.Clear();

            //await UniTask.WaitForSeconds(3, cancellationToken: _cts.Token);
            
            // LOOP CRESCIMENTO L
            while(zonesToGrow.Count > 0)
            {
                _currentZone = GetNextZone(zonesToGrow);
                
                //_currentZone.Test(_floorPlanManager.CellsGrid);
                
                zonesToGrow.Remove(_currentZone);
                grownZones.Add(_currentZone);
                
                /*
                _zoneBorder_TEMP = _currentZone.GetLargestSideLine(_floorPlanManager.CellsGrid, Zone.Side.Top);
                if(_zoneBorder_TEMP.numberOfCells > 0) await UniTask.WaitForSeconds(0.5f, cancellationToken: _cts.Token);
                _zoneBorder_TEMP = _currentZone.GetLargestSideLine(_floorPlanManager.CellsGrid, Zone.Side.Bottom);
                if(_zoneBorder_TEMP.numberOfCells > 0) await UniTask.WaitForSeconds(0.5f, cancellationToken: _cts.Token);
                _zoneBorder_TEMP = _currentZone.GetLargestSideLine(_floorPlanManager.CellsGrid, Zone.Side.Left);
                if(_zoneBorder_TEMP.numberOfCells > 0) await UniTask.WaitForSeconds(0.5f, cancellationToken: _cts.Token);
                _zoneBorder_TEMP = _currentZone.GetLargestSideLine(_floorPlanManager.CellsGrid, Zone.Side.Right);
                if(_zoneBorder_TEMP.numberOfCells > 0) await UniTask.WaitForSeconds(0.5f, cancellationToken: _cts.Token);
                */
                
                

                /*
                if(!GrowZoneLShape(_currentZone, _floorPlanManager.CellsGrid))
                {
                    zonesToGrow.Remove(_currentZone);
                    grownZones.Add(_currentZone);
                }
                */

                TriggerOnCellsGridChanged(_floorPlanManager.CellsGrid);
                await UniTask.WaitForSeconds(delay, cancellationToken: _cts.Token);
            }

            // CRESCIMENTO LIVRE(espaços restantes)
            // while free spaces.
            // ======================== end main grow logic

            // Prepare the next set of zones to grow.
            foreach(Zone zone in grownZones)
            {
                if(zone._childZones?.Count > 0)
                {
                    zonesToSubdivide.Add(zone);
                }
            }

            //TriggerOnCellsGridChanged(_floorPlanManager.CellsGrid);
            //await UniTask.WaitForSeconds(delay, cancellationToken: _cts.Token);

            grownZones.Clear();
        }
        

        EditorApplication.playModeStateChanged -= PlayModeStateChanged;

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
            return zone.GrowLSide(cellsGrid);
        }
        else
        {
            /*
            if(zone.SpaceToGrowTop(cellsGrid) > 0)
            {
                return zone.GrowTop(cellsGrid);
            }
            else if(zone.SpaceToGrowBottom(cellsGrid) > 0)
            {
                return zone.GrowBottom(cellsGrid);
            }
            else if(zone.SpaceToGrowLeft(cellsGrid) > 0)
            {
                return zone.GrowLeft(cellsGrid);
            }
            else if(zone.SpaceToGrowRight(cellsGrid) > 0)
            {
                return zone.GrowRight(cellsGrid);
            }
            else
            */
            {
                if(zone.SetLBorder(cellsGrid))
                    return zone.GrowLSide(cellsGrid);
            }
        }

        return false;
    }

    bool GrowZoneAnyDirection(Zone zone, CellsGrid cellsGrid)
    {
        var directions = Enum.GetValues(typeof(Zone.Side)).Cast<Zone.Side>().OrderBy(d => Guid.NewGuid());

        foreach(var direction in directions)
        {
            if(TryGrowToDirection(direction, zone, cellsGrid))
            {
                return true;
            }
        }

        return false;
    }

    // OBS: the direction paramenter is taking in mind a randomization of the order of try.
    bool TryGrowToDirection(Zone.Side direction, Zone zone, CellsGrid cellsGrid)
    {
        switch(direction)
        {
            case Zone.Side.Top:
                if(zone.TryGrowTop(cellsGrid))
                    return true;
                break;
            case Zone.Side.Bottom:
                if(zone.TryGrowBottom(cellsGrid))
                    return true;
                break;
            case Zone.Side.Left:
                if(zone.TryGrowLeft(cellsGrid))
                    return true;
                break;
            case Zone.Side.Right:
                if(zone.TryGrowRight(cellsGrid))
                    return true;
                break;
        }

        return false;
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
        /*
        switch(_zoneBorder_TEMP.side)
        {
            case Zone.Side.Top:
            from = new Vector3(zb.firstCellX - os,
                              1,
                              -zb.firstCellY);
            to = new Vector3(zb.firstCellX + os + zb.numberOfCells - 1,
                             1,
                             -zb.firstCellY);
            break;
            case Zone.Side.Bottom:
            from = new Vector3(zb.firstCellX - os,
                              1,
                              -zb.firstCellY);
            to = new Vector3(zb.firstCellX + os + zb.numberOfCells - 1,
                             1,
                             -zb.firstCellY);
            break;
            case Zone.Side.Left:
            from = new Vector3(zb.firstCellX,
                              1,
                              -(zb.firstCellY - os));
            to = new Vector3(zb.firstCellX,
                             1,
                             -(zb.firstCellY + os + zb.numberOfCells - 1));
            break;
            case Zone.Side.Right:
            from = new Vector3(zb.firstCellX,
                              1,
                              -(zb.firstCellY - os));
            to = new Vector3(zb.firstCellX,
                             1,
                             -(zb.firstCellY + os + zb.numberOfCells - 1));
            break;
        }
        Gizmos.DrawLine(from, to);
        */
    }
}

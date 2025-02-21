using UnityEngine;
using System.Threading;
using UnityEditor;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System;
using System.Linq;

public class MethodGrowth : FPGenerationMethod
{
    private CancellationTokenSource _cts;
    private bool _done = false;
    Zone _currentZone;
    List<Zone> zonesToGrow;
    List<Zone> zonesToSubdivide;

    enum Direction
    {
        Top = 0,
        Bottom = 1,
        Left = 2,
        Right = 3
    }

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
        AsyncTicker asyncTicker = AsyncTicker.Instantiate();

        zonesToSubdivide = new List<Zone>();
        zonesToGrow = new List<Zone>(){_floorPlanManager.RootZone};

        // First cell of root zone.
        _currentZone = zonesToGrow[0];
        PlotFirstZoneCell(_currentZone);
        TriggerOnCellsGridChanged(_floorPlanManager.CellsGrid);
        await UniTask.WaitForSeconds(0.05f);

        // Growth
        asyncTicker.Begin(Method, 0.05f);
            await UniTask.WaitUntil(Finished, cancellationToken: _cts.Token);
        asyncTicker.End();

        // conjunto de zonas atual n tem para onde se expandir
        // atualiza conjunto de zonas para expandir
        // recomeça

        EditorApplication.playModeStateChanged -= PlayModeStateChanged;

        return true;
    }

    public void Method()
    {
        if(_done) return; // just in case.

        // iterar sobre a zona raiz a primeirazona define a area util
        // se a zona ja tem uma predefinição, atribui as celulas correspondentes
        // se não, segue a logi normal. plot inicial, crescimento.

        if(!GrowZoneRect(_currentZone, _floorPlanManager.CellsGrid))
        {
            // se a zona não pode mais se expandir, remove da lista de zonas para expanção.
            zonesToGrow.Remove(_currentZone);
            if(_currentZone._childZones?.Count > 0)
            {
                zonesToSubdivide.Add(_currentZone);
            }
        }

        if(zonesToGrow.Count == 0)
        {
            zonesToGrow = GetNextZonesToGrowList();

            // No more zones to generate, end of execution.
            if(zonesToGrow == null)
            {
                _done = true;
                //_cts.Cancel();
            }
        }
        else
        {
            _currentZone = GetNextZone(zonesToGrow); // TODO: MOVER PARA O INICIO
        }

        TriggerOnCellsGridChanged(_floorPlanManager.CellsGrid);
    }


    List<Zone> GetNextZonesToGrowList()
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
        float aspect = zone.GetZoneAspect();

        //string log = string.Empty;
        //log += $"asp: {aspect}| des: {zone.DesiredAspect}\n";

        // Guid.NewGuid() provides a way to get unique randon numbers. Create a array with the directions
        // and sort it using the guids.
        var directions = Enum.GetValues(typeof(Direction)).Cast<Direction>().OrderBy(d => Guid.NewGuid());

        foreach(var direction in directions)
        {
            // All directions
            if(aspect == zone.DesiredAspect)
            {
                if(TryGrowToDirection(direction, zone, cellsGrid))
                {
                    //log += $"grow to {direction}\n";
                    //Debug.LogWarning(log);
                    return true;
                }
            }
            // Vertical
            else if(aspect > zone.DesiredAspect && (direction == Direction.Top || direction == Direction.Bottom))
            {
                if(TryGrowToDirection(direction, zone, cellsGrid))
                {
                    //log += $"grow to {direction}\n";
                    //Debug.LogWarning(log);
                    return true;
                }
            }
            // Horizontal
            else if(aspect < zone.DesiredAspect && (direction == Direction.Left || direction == Direction.Right))
            {
                if(TryGrowToDirection(direction, zone, cellsGrid))
                {
                    //log += $"grow to {direction}\n";
                    //Debug.LogWarning(log);
                    return true;
                }
            }
        }

        return false; // can't grow.
    }

    // OBS: the direction paramenter is taking in mind a randomization of the order of try.
    bool TryGrowToDirection(Direction direction, Zone zone, CellsGrid cellsGrid)
    {
        switch(direction)
        {
            case Direction.Top:
                if(zone.TryGrowTop(cellsGrid))
                    return true;
                break;
            case Direction.Bottom:
                if(zone.TryGrowBottom(cellsGrid))
                    return true;
                break;
            case Direction.Left:
                if(zone.TryGrowLeft(cellsGrid))
                    return true;
                break;
            case Direction.Right:
                if(zone.TryGrowRight(cellsGrid))
                    return true;
                break;
        }

        return false;
    }


    bool Finished()
    {
        /*
        if(_currentZone._cells.Count == _floorPlanManager.CellsGrid._cells.Length)
        {
            return true;
        }

        return false;
        */
        return _done;
    }


    void PlayModeStateChanged(PlayModeStateChange state)
    {
        if(state == PlayModeStateChange.ExitingPlayMode)
        {
            _cts.Cancel();
        }
    }
}

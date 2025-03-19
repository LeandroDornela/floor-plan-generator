using UnityEngine;
using System.Threading;
using UnityEditor;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System;
using System.Linq;
using Unity.Mathematics;

[System.Serializable]
public partial class MethodGrowth : FPGenerationMethod
{
    public AnimationCurve _borderDistanceCurve;

    private CancellationTokenSource _cts;
    private bool _done = false;
    Zone _currentZone;
    List<Zone> zonesToSubdivide; // TODO: QUEUE
    List<Zone> zonesToGrow;
    // quando uma zona n pode mais crescer na iteração atual é armazenada aqui.
    // Depois retorna para crescer usando outra logical, 'L' ou 'free'
    List<Zone> grownZones;

    // key = Summation, value = cell weight
    private WeightedArray _cellsWeights;
    //private float _cellsWeightsTotalSum;
    
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

        CellsGrid cellsGrid = _floorPlanManager.CellsGrid;

        // TODO: temporario.
        // Setup da zona raiz.
        int corner = 5; // Utils.RandomRange(0,6);
        int a = Utils.RandomRange(2,10);
        int b = Utils.RandomRange(2,10);
        
        foreach(Cell cell in cellsGrid._cells)
        {
            if(corner == 0 && cell.GridPosition.x > a && cell.GridPosition.y > b) continue;
            else if (corner == 1 && cell.GridPosition.x < a && cell.GridPosition.y < b) continue;
            else if (corner == 2 && cell.GridPosition.x > a && cell.GridPosition.y < b) continue;
            else if (corner == 3 && cell.GridPosition.x < a && cell.GridPosition.y > b) continue;
            else if (corner == 4 && cell.GridPosition.x > 3 && cell.GridPosition.x < 10 && cell.GridPosition.y > 3 && cell.GridPosition.y < 10) continue;
            // corner == 5, full area

            cellsGrid.AssignCellToZone(cell.GridPosition.x, cell.GridPosition.y, _floorPlanManager.RootZone);
        }

        zonesToSubdivide.Add(_floorPlanManager.RootZone);
        
        while(zonesToSubdivide.Count > 0) // A CADA EXECUÇÃO FAZ A DIVISÃO DE UMA ZONA.
        {
            // Get the child zones from the next zone to subdivide.
            zonesToGrow = GetNextZonesToGrowList();

            //TriggerOnCellsGridChanged(cellsGrid);
            //await UniTask.WaitForSeconds(2, cancellationToken: _cts.Token);


            // >>>>>>>>>>> begin main grow logic
            // =========================================================== LOOP CRESCIMENTO RECT
            while(zonesToGrow.Count > 0)
            {
                _currentZone = GetNextZone(zonesToGrow);

                if(!GrowZoneRect(_currentZone, cellsGrid))
                {
                    zonesToGrow.Remove(_currentZone);
                    grownZones.Add(_currentZone);
                }

                //TriggerOnCellsGridChanged(cellsGrid);
                //await UniTask.WaitForSeconds(delay, cancellationToken: _cts.Token);
            }


            zonesToGrow = new List<Zone>(grownZones);
            grownZones.Clear();
            
            
            // ======================================================== LOOP L
            // LOOP CRESCIMENTO L
            while(zonesToGrow.Count > 0)
            {
                _currentZone = GetNextZone(zonesToGrow);
                
                if(!GrowZoneLShape(_currentZone, cellsGrid))
                {
                    zonesToGrow.Remove(_currentZone);
                    grownZones.Add(_currentZone);
                }

                //TriggerOnCellsGridChanged(cellsGrid);
                //await UniTask.WaitForSeconds(delay, cancellationToken: _cts.Token);
            }


            // ======================================================== CRESCIMENTO LIVRE(espaços restantes)
            // while free spaces.

            // <<<<<<<<<< end main grow logic

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

        TriggerOnCellsGridChanged(cellsGrid);
        //await UniTask.WaitForSeconds(delay, cancellationToken: _cts.Token, cancelImmediately: true);
        await UniTask.WaitForEndOfFrame();
        //_cts.Dispose();

        return true;
    }

    public override bool RunSync()
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

        CellsGrid cellsGrid = _floorPlanManager.CellsGrid;

        // TODO: temporario.
        // Setup da zona raiz.
        int corner = Utils.RandomRange(0,6);
        int a = Utils.RandomRange(2,10);
        int b = Utils.RandomRange(2,10);
        
        foreach(Cell cell in cellsGrid._cells)
        {
            if(corner == 0 && cell.GridPosition.x > a && cell.GridPosition.y > b) continue;
            else if (corner == 1 && cell.GridPosition.x < a && cell.GridPosition.y < b) continue;
            else if (corner == 2 && cell.GridPosition.x > a && cell.GridPosition.y < b) continue;
            else if (corner == 3 && cell.GridPosition.x < a && cell.GridPosition.y > b) continue;
            else if (corner == 4 && cell.GridPosition.x > 3 && cell.GridPosition.x < 10 && cell.GridPosition.y > 3 && cell.GridPosition.y < 10) continue;
            // corner == 5, full area

            cellsGrid.AssignCellToZone(cell.GridPosition.x, cell.GridPosition.y, _floorPlanManager.RootZone);
        }

        zonesToSubdivide.Add(_floorPlanManager.RootZone);
        
        while(zonesToSubdivide.Count > 0) // A CADA EXECUÇÃO FAZ A DIVISÃO DE UMA ZONA.
        {
            // Get the child zones from the next zone to subdivide.
            zonesToGrow = GetNextZonesToGrowList();

            // >>>>>>>>>>> begin main grow logic
            // =========================================================== LOOP CRESCIMENTO RECT
            while(zonesToGrow.Count > 0)
            {
                _currentZone = GetNextZone(zonesToGrow);

                if(!GrowZoneRect(_currentZone, cellsGrid))
                {
                    zonesToGrow.Remove(_currentZone);
                    grownZones.Add(_currentZone);
                }
            }


            zonesToGrow = new List<Zone>(grownZones);
            grownZones.Clear();
            
            
            // ======================================================== LOOP L
            // LOOP CRESCIMENTO L
            while(zonesToGrow.Count > 0)
            {
                _currentZone = GetNextZone(zonesToGrow);
                
                if(!GrowZoneLShape(_currentZone, cellsGrid))
                {
                    zonesToGrow.Remove(_currentZone);
                    grownZones.Add(_currentZone);
                }
            }


            // ======================================================== CRESCIMENTO LIVRE(espaços restantes)
            // while free spaces.

            // <<<<<<<<<< end main grow logic

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

        return true;
    }


    bool GrowZoneRect(Zone zone, CellsGrid cellsGrid)
    {
        //return DEBUG_GrowSequential(zone, cellsGrid);

        float aspect = zone.GetZoneAspect();

        // Guid.NewGuid() provides a way to get unique randon numbers. Create a array with the directions
        // and sort it using the guids.
        var sides = Enum.GetValues(typeof(Zone.Side)).Cast<Zone.Side>().OrderBy(d => Guid.NewGuid());

        foreach(var side in sides)
        {
            // All directions
            if(aspect == zone.DesiredAspect)
            {
                if(TryGrowFromSide(side, zone, cellsGrid))
                {
                    return true;
                }
            }
            // Vertical
            else if(aspect > zone.DesiredAspect && (side == Zone.Side.Top || side == Zone.Side.Bottom))
            {
                if(TryGrowFromSide(side, zone, cellsGrid))
                {
                    return true;
                }
            }
            // Horizontal
            else if(aspect < zone.DesiredAspect && (side == Zone.Side.Left || side == Zone.Side.Right))
            {
                if(TryGrowFromSide(side, zone, cellsGrid))
                {
                    return true;
                }
            }
        }

        return false; // can't grow.
    }


    bool GrowZoneLShape(Zone zone, CellsGrid cellsGrid)
    {
        //return GrowRectUntilLargestIsL(zone, cellsGrid);
        return GrowRectThenGrowL(zone, cellsGrid);
    }

    // L expansion variation 1
    bool GrowRectUntilLargestIsL(Zone zone, CellsGrid cellsGrid)
    {
        // expand L if is L
        if(zone.IsLShaped)
        {
            return zone.TryExpandShapeL(cellsGrid);
        }

        var largestFreeSpace = zone.GetLargestExpansionSpaceRect(cellsGrid);

        // No side to expand
        if(largestFreeSpace.distance == 0)
        {
            return false;
        }

        // Check side is only part of a border, if is set to expand L, else expand rect.
        if(!largestFreeSpace.isFullLine)
        {
            zone.SetAsLShaped(largestFreeSpace.freeLineDescription);
            return zone.TryExpandShapeL(cellsGrid);
        }
        else
        {
            return zone.TryExpandShapeRect(largestFreeSpace.freeLineDescription.side, cellsGrid);
        }
    }

    // L expansion variation 2
    bool GrowRectThenGrowL(Zone zone, CellsGrid cellsGrid)
    {
        // expand L if is L
        if(zone.IsLShaped)
        {
            return zone.TryExpandShapeL(cellsGrid);
        }

        // Go on all sides randomly trying to expand
        var sides = Enum.GetValues(typeof(Zone.Side)).Cast<Zone.Side>().OrderBy(d => Guid.NewGuid());
        foreach(var side in sides)
        {
            if(zone.GetExpansionSpaceRect(side, cellsGrid).isFullLine)
            {
                if(zone.TryExpandShapeRect(side, cellsGrid))
                {
                    return true;
                }
            }
        }

        // If wasn't able to expand any side, try to start L
        var largestFreeSide = zone.GetLargestExpansionSpaceRect(cellsGrid);
        if(largestFreeSide.distance == 0) // No free side
        {
            return false;
        }
        if(largestFreeSide.isFullLine) // Just in case
        {
            Debug.LogWarning("At this point it should not have a full border available.");
        }
        zone.SetAsLShaped(largestFreeSide.freeLineDescription);
        return zone.TryExpandShapeL(cellsGrid);

    }


    // ============================================================================================================================

    bool GrowOnAnySide(Zone zone, CellsGrid cellsGrid)
    {
        return false;
    }

    bool GrowOnSideWithMoreSpace(Zone zone, CellsGrid cellsGrid)
    {
        return false;
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
            //CalculateWeights(zone);

            PlotFirstZoneCell(zone); // TODO: move to outside the method
        }

        return  childZones;
    }


    void PlotFirstZoneCell(Zone zone)
    {
        if(zone._parentZone != null)
        {
            /*
            int index = Utils.RandomRange(0, zone._parentZone._cells.Count);
            Cell cell = null;
            try
            {
                if(zone._parentZone._cells.Count == 0) Debug.LogError("Parent zone has no cells!");

                cell = zone._parentZone._cells[index];
            }
            catch
            {
                Debug.LogError($"{zone._parentZone._cells.Count}, {index}");
            }*/
            CalculateWeights(zone);
            //Cell cell = GetRandomWeightedCell(_floorPlanManager.CellsGrid.Cells);
            //if(cell != null)_floorPlanManager.CellsGrid.AssignCellToZone(cell.GridPosition.x, cell.GridPosition.y, zone);
            if(_cellsWeights.GetRandomWeightedElement(_floorPlanManager.CellsGrid.Cells, out Cell cell))
            {
                _floorPlanManager.CellsGrid.AssignCellToZone(cell.GridPosition.x, cell.GridPosition.y, zone);
            }
        }
        else
        {
            Vector2Int position = new Vector2Int(Utils.RandomRange(0, _floorPlanManager.CellsGrid.Dimensions.x),
                                                 Utils.RandomRange(0, _floorPlanManager.CellsGrid.Dimensions.y));
            
            _floorPlanManager.CellsGrid.AssignCellToZone(position.x, position.y, zone);
        }
    }


    Zone GetNextZone(List<Zone> zonesToGrow)
    {
        return zonesToGrow[Utils.RandomRange(0, zonesToGrow.Count)];
    }


    // OBS: the direction paramenter is taking in mind a randomization of the order of try.
    bool TryGrowFromSide(Zone.Side side, Zone zone, CellsGrid cellsGrid)
    {
        if(zone.GetExpansionSpaceRect(side, cellsGrid, false).isFullLine)
        {
            return zone.TryExpandShapeRect(side, cellsGrid);
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


    // -> a celula(ZONA) que vai ser colocada,
    // -> as zonas ja exitentes, as bordas, as portas, as janelas(celulas presentes com caracteristicas que interferen no peso)
    // <- peso da celula
    void CalculateWeights(Zone zoneToPlot, List<Zone> plottedZones = null)
    {
        Debug.Log($"Plotting zone: {zoneToPlot.ZoneId}");

        var cellsGrid = _floorPlanManager.CellsGrid;
        _cellsWeights = new WeightedArray(cellsGrid.Cells.Length);
        Zone parentZone = zoneToPlot._parentZone;

        for(int i = 0; i < cellsGrid.Cells.Length; i++) // TODO: percorrer apensas as celulas da zona mãe e não todas
        {
            Cell cell = cellsGrid.Cells[i];
            float weight = 0;

            // Cell outside the current parent zone, the context zone.
            if(cell.Zone != parentZone)
            {
                _cellsWeights.AddAt(i, weight);
                continue; // Next cell.
            }


            // Distance from borders.
            float smallerDistance = float.MaxValue;
            foreach(Cell borderCell in parentZone.FindBorderCells(cellsGrid))
            {
                float distance = MathF.Round((borderCell.GridPosition - cell.GridPosition).magnitude);
                if(distance < smallerDistance)
                    smallerDistance = distance;
            }
            weight = 1/MathF.Pow(smallerDistance + 1, 4);


            _cellsWeights.AddAt(i, weight);
        }

        
        Utils.PrintArrayAsGrid(cellsGrid.Dimensions.x, cellsGrid.Dimensions.y, _cellsWeights.Values);
    }
}

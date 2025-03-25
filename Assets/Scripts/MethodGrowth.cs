using UnityEngine;
using System.Threading;
using UnityEditor;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System;
using System.Linq;

[System.Serializable]
[CreateAssetMenu(fileName = "DefaultGrowthMethod", menuName = "Scriptable Objects/Generation Methods/Default Growth")]
public partial class MethodGrowth : FPGenerationMethod
{
    public AnimationCurve _borderDistanceCurve;
    public AnimationCurve _adjacencyDistanceCurve;
    

    [Header("Testing")]
    public float delay = 0.01f;
    public bool _ignoreDesiredAreaInRect = false;
    public bool _stopAtInitialPlot = false;
    public bool _skipToFinalResult = false;
    public bool _randomInitialArea = false;

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
    private WeightedArray _zonesWeights;
    //private float _cellsWeightsTotalSum;

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
        int corner;
        if(_randomInitialArea)
            corner = Utils.RandomRange(0,6);
        else
            corner = 5;
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
            UpdateZonesWeights(zonesToGrow);

            //TriggerOnCellsGridChanged(cellsGrid);
            //await UniTask.WaitForSeconds(delay, cancellationToken: _cts.Token);
            if(_stopAtInitialPlot) break;


            // >>>>>>>>>>> begin main grow logic
            // =========================================================== LOOP CRESCIMENTO RECT
            while(zonesToGrow.Count > 0)
            {
                _currentZone = GetNextZone(zonesToGrow);

                if(!GrowZoneRect(_currentZone, cellsGrid))
                {
                    zonesToGrow.Remove(_currentZone);
                    UpdateZonesWeights(zonesToGrow);
                    grownZones.Add(_currentZone);
                }

                if(!_skipToFinalResult)
                {
                    TriggerOnCellsGridChanged(cellsGrid);
                    await UniTask.WaitForSeconds(delay + 0.1f, cancellationToken: _cts.Token);
                }
            }


            //TriggerOnCellsGridChanged(cellsGrid);
            //await UniTask.WaitForSeconds(10, cancellationToken: _cts.Token);


            zonesToGrow = new List<Zone>(grownZones);
            UpdateZonesWeights(zonesToGrow);
            grownZones.Clear();
            
            
            // ======================================================== LOOP L
            // LOOP CRESCIMENTO L
            while(zonesToGrow.Count > 0)
            {
                _currentZone = GetNextZone(zonesToGrow);
                
                if(!GrowZoneLShape(_currentZone, cellsGrid))
                {
                    zonesToGrow.Remove(_currentZone);
                    UpdateZonesWeights(zonesToGrow);
                    grownZones.Add(_currentZone);
                }

                if(!_skipToFinalResult)
                {
                    TriggerOnCellsGridChanged(cellsGrid);
                    await UniTask.WaitForSeconds(delay, cancellationToken: _cts.Token);
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

        TriggerOnCellsGridChanged(cellsGrid);
        //await UniTask.WaitForSeconds(delay, cancellationToken: _cts.Token, cancelImmediately: true);
        await UniTask.WaitForEndOfFrame();
        _cts.Dispose();

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

        if(zone.HasDesiredArea(cellsGrid) && !_ignoreDesiredAreaInRect)
        {
            return false;
        }

/*
        float aspect = zone.GetZoneAspect();
        // All directions
        if(aspect == zone.DesiredAspect)
        {
            var largestFreeSpace = zone.GetLargestExpansionSpaceRect(cellsGrid, true);
            if(largestFreeSpace.isFullLine)
            {
                return zone.TryExpandShapeRect(largestFreeSpace.freeLineDescription.side, cellsGrid);
            }
        }
        // Vertical
        else if(aspect > zone.DesiredAspect && (side == Zone.Side.Top || side == Zone.Side.Bottom))
        {
            var freeSpaceOnTop = zone.GetExpansionSpaceRect(Zone.Side.Top, cellsGrid, true);
            var freeSpaceOnBottom = zone.GetExpansionSpaceRect(Zone.Side.Top, cellsGrid, true);
        }
        // Horizontal
        else if(aspect < zone.DesiredAspect && (side == Zone.Side.Left || side == Zone.Side.Right))
        {
            if(TryGrowFromSide(side, zone, cellsGrid))
            {
                return true;
            }
        }
*/


        var largestFreeSpace = zone.GetLargestExpansionSpaceRect(cellsGrid, false); // INTERESSANTE, usar essa função sem checar espaço total gera formas retangulares, mas é mias custoso do q checar os aspect antes
        if(largestFreeSpace.isFullLine)
        {
            return zone.TryExpandShapeRect(largestFreeSpace.freeLineDescription.side, cellsGrid);
        }
        else
        {
            return false;
        }



        /*
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
        */

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

        // Zone to sub div has the final shape, bake the borders.
        zonesToSubdivide[0].SetBorderCells(_floorPlanManager.CellsGrid);

        var childZones = zonesToSubdivide[0]._childZones;
        zonesToSubdivide.RemoveAt(0);

        for(int i = 0; i < childZones.Count; i++)
        {
            Zone zone = childZones[i];
            PlotFirstZoneCell(zone, childZones); // TODO: move to outside the method
        }

        return  childZones;
    }


    void PlotFirstZoneCell(Zone zone, List<Zone> zonesToGrow)
    {
        if(zone._parentZone != null)
        {
            /*
            // Random selection
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
            }
            _floorPlanManager.CellsGrid.AssignCellToZone(cell.GridPosition.x, cell.GridPosition.y, zone);
            */
            
            // Weighted selection
            CalculateWeights(zone, zonesToGrow);
            //if(_cellsWeights.GetRandomWeightedElement(zone._parentZone.Cells, out Cell cell))
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
        //return zonesToGrow[Utils.RandomRange(0, zonesToGrow.Count)];

        if(_zonesWeights.GetRandomWeightedElement(zonesToGrow.ToArray(), out Zone zone))
        {
            return zone;
        }

        Debug.LogError("Unable to get a zone.");
        return null;
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
    // TODO: peso para entrada e zonas primas.
    // TODO: se celulas mais distantes teram valores de borda sobre escritos, deve se ter um melhor desenpenho al checar peso de adjacencia antes e pular aqueles com valor 0(talvez).
    // TODO: tratamento peso 0 final em todas celulas.
    void CalculateWeights(Zone zoneToPlot, List<Zone> plottedZones = null)
    {
        //Debug.Log($"Plotting zone: {zoneToPlot.ZoneId}");

        CellsGrid cellsGrid = _floorPlanManager.CellsGrid;
        Zone parentZone = zoneToPlot._parentZone;
        Cell[] cellsToCalc =  _floorPlanManager.CellsGrid.Cells;//parentZone.Cells;
        _cellsWeights = new WeightedArray(cellsToCalc.Length);

        float adjacencyWeightMultiplier = 1;
        float borderWeightMultiplier = 1;

        int desiredZoneSqSize = Mathf.CeilToInt(Mathf.Sqrt(zoneToPlot.AreaRatio * cellsGrid.Area)); // Ceiling to round up to a size that can fit it.
        int desiredZoneDim = Mathf.CeilToInt(zoneToPlot.AreaRatio * cellsGrid.DiagonalMagnitudeRounded);
        int minimumBorderDistance = desiredZoneSqSize / 4;
        

        for(int i = 0; i < cellsToCalc.Length; i++)
        {
            Cell cell = cellsToCalc[i];
            float weight = 0;

            // Cell outside the current parent zone, the context zone.
            if(cell.Zone != parentZone)
            {
                _cellsWeights.AddAt(i, 0);
                continue; // Next cell.
            }

            
            // Calculate the smaller distance from borders.
            float smallerDistance = float.MaxValue;
            foreach(Cell borderCell in parentZone.BorderCells)
            {
                float distance = Mathf.Round((borderCell.GridPosition - cell.GridPosition).magnitude);
                if(distance < smallerDistance)
                    smallerDistance = distance;
            }

            // Calculate de weight from borders using the desired size of the zone sides on a function defined by a animation curve.
            if(smallerDistance < minimumBorderDistance || smallerDistance > desiredZoneSqSize) // (smallerDistance > desiredZoneSqSize) smaller distance > expected side size, the cell is too far.
            {
                weight = 0;
            }
            else
            {
                weight = _borderDistanceCurve.Evaluate(smallerDistance / desiredZoneSqSize) * borderWeightMultiplier; // The value to be obtained is between a minimum and maximum based on zone to plot desired size.
            }

            
            // Distance from adjacent or non adjacent.
            if(plottedZones != null)
            {
                foreach(Zone plottedZone in plottedZones)
                {
                    if(plottedZone.OriginCell == null) // Plotted zone actually is not plotted.
                        continue;

                    float distance = Mathf.Round((plottedZone.OriginCell.GridPosition - cell.GridPosition).magnitude);
                    float expectedAdjacentZoneSide = Mathf.Ceil(MathF.Sqrt(plottedZone.AreaRatio * cellsGrid.Area)) / 2;
                    float normToPlottedRadius = distance / expectedAdjacentZoneSide;
                    float normToTotalRadius = distance / (desiredZoneSqSize + expectedAdjacentZoneSide);


                    // Between a minimum and maximum distance
                    // *____________)---x--------)__________...
                    // {1}*_________{2} ) ----- {3}x ---- {4} )___________...
                    // 1:plotted cell
                    // 2:expectedAdjacentZoneSide
                    // 3:distance
                    // 4:desiredZoneSqSize + expectedAdjacentZoneSide
                    if(normToPlottedRadius < 1) // To close of a plotted zone
                    {
                        weight = 0;
                        break; // Don't need to check other plotted zones.
                    }
                    else if(normToTotalRadius <= 1)
                    {
                        if(zoneToPlot.IsAdjacent(plottedZone))
                        {
                            weight += _adjacencyDistanceCurve.Evaluate(normToTotalRadius);
                        }
                        else
                        {
                            weight -= _adjacencyDistanceCurve.Evaluate(normToTotalRadius);
                        }
                    }
                    else if(zoneToPlot.IsAdjacent(plottedZone)) // normToTotalRadius > 1
                    {
                        weight = 0; // Will prevent borders far away from adjacent zones to have weight.
                    }
                    // else don't change the weight.

                    weight = Mathf.Clamp(weight, 0, float.MaxValue);
                    weight = weight * adjacencyWeightMultiplier;
                }
            }


            _cellsWeights.AddAt(i, weight);
        }

        
        Utils.PrintArrayAsGrid(cellsGrid.Dimensions.x, cellsGrid.Dimensions.y, _cellsWeights.Values);
    }


    void UpdateZonesWeights(List<Zone> zonesToGrow)
    {
        _zonesWeights = new WeightedArray(zonesToGrow.Count);

        for(int i = 0; i < zonesToGrow.Count; i++)
        {
            Zone zone = zonesToGrow[i];
            _zonesWeights.AddAt(i, zone.AreaRatio);
        }
    }
}

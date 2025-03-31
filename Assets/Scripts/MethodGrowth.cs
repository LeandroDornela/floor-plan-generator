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
    // DATA
    [Header("Weights")]
    public AnimationCurve _borderDistanceCurve;
    public AnimationCurve _adjacencyDistanceCurve;
    public float _adjacencyWeightMultiplier = 1;
    public float _borderWeightMultiplier = 1;
    

    [Header("Testing")]
    public float delay = 0.01f;
    public bool _ignoreDesiredAreaInRect = false;
    public bool _stopAtInitialPlot = false;
    public bool _skipToFinalResult = false;
    public bool _randomInitialArea = false;
    public bool _ignoreBorderWeights = false;
    public bool _ignoreAdjacentWeights = false;

    // RUN TIME 
    private CancellationTokenSource _cts;
    private Zone _currentZone;
    private List<Zone> _zonesToSubdivide; // TODO: QUEUE
    private List<Zone> _zonesToGrow;
    
    // Quando uma zona n pode mais crescer na iteração atual é armazenada aqui.
    // Depois retorna para crescer usando outra logical, 'L' ou 'free'
    private List<Zone> _grownZones;
    private WeightedArray _cellsWeights;
    private WeightedArray _zonesWeights;


    
    /// <summary>
    /// TODO: receber a grid e tudo mais como parametros
    /// </summary>
    /// <returns></returns>
    public override async UniTask<bool> Run(FloorPlanManager floorPlanManager, FloorPlanGenSceneDebugger sceneDebugger)
    {
        
        if(!EditorApplication.isPlaying)
        {
            Debug.LogError("Don't use it outside play mode.");
            return false;
        }

        _cts = new CancellationTokenSource();

        EditorApplication.playModeStateChanged += PlayModeStateChanged;

        _zonesToSubdivide = new List<Zone>();
        _zonesToGrow = new List<Zone>();
        _grownZones = new List<Zone>();

        CellsGrid cellsGrid = floorPlanManager.CellsGrid;

        // TODO: temporario.
        // Setup da zona raiz.
        int corner;
        if(_randomInitialArea)
            corner = Utils.RandomRange(0,6);
        else
            corner = 5;
        int a = Utils.RandomRange(2,10);
        int b = Utils.RandomRange(2,10);
        
        foreach(Cell cell in cellsGrid.Cells)
        {
            if(corner == 0 && cell.GridPosition.x > a && cell.GridPosition.y > b) continue;
            else if (corner == 1 && cell.GridPosition.x < a && cell.GridPosition.y < b) continue;
            else if (corner == 2 && cell.GridPosition.x > a && cell.GridPosition.y < b) continue;
            else if (corner == 3 && cell.GridPosition.x < a && cell.GridPosition.y > b) continue;
            else if (corner == 4 && cell.GridPosition.x > 3 && cell.GridPosition.x < 10 && cell.GridPosition.y > 3 && cell.GridPosition.y < 10) continue;
            // corner == 5, full area

            //_floorPlanManager.AssignCellToZone(cell.GridPosition.x, cell.GridPosition.y, _floorPlanManager.RootZone);
            floorPlanManager.AssignCellToZone(cell, floorPlanManager.RootZone);
        }

        floorPlanManager.RootZone.Bake();
        _zonesToSubdivide.Add(floorPlanManager.RootZone);
        
        while(_zonesToSubdivide.Count > 0) // A CADA EXECUÇÃO FAZ A DIVISÃO DE UMA ZONA.
        {
            // Get the child zones from the next zone to subdivide.
            _zonesToGrow = GetNextZonesToGrowList(floorPlanManager);
            UpdateZonesWeights(_zonesToGrow);

            //TriggerOnCellsGridChanged(cellsGrid);
            //await UniTask.WaitForSeconds(delay, cancellationToken: _cts.Token);
            if(_stopAtInitialPlot) break;


            // >>>>>>>>>>> begin main grow logic
            // =========================================================== LOOP CRESCIMENTO RECT
            while(_zonesToGrow.Count > 0)
            {
                _currentZone = GetNextZone(_zonesToGrow);

                if(!GrowZoneRect(_currentZone, cellsGrid))
                {
                    _zonesToGrow.Remove(_currentZone);
                    UpdateZonesWeights(_zonesToGrow);
                    _grownZones.Add(_currentZone);
                }

                if(!_skipToFinalResult)
                {
                    sceneDebugger.OnCellsGridChanged(cellsGrid);
                    await UniTask.WaitForSeconds(delay + 0.1f, cancellationToken: _cts.Token);
                }
            }


            //TriggerOnCellsGridChanged(cellsGrid);
            //await UniTask.WaitForSeconds(10, cancellationToken: _cts.Token);


            _zonesToGrow = new List<Zone>(_grownZones);
            UpdateZonesWeights(_zonesToGrow);
            _grownZones.Clear();
            
            
            // ======================================================== LOOP L
            // LOOP CRESCIMENTO L
            while(_zonesToGrow.Count > 0)
            {
                _currentZone = GetNextZone(_zonesToGrow);
                
                if(!GrowZoneLShape(_currentZone, cellsGrid))
                {
                    _zonesToGrow.Remove(_currentZone);
                    UpdateZonesWeights(_zonesToGrow);
                    _grownZones.Add(_currentZone);
                }

                if(!_skipToFinalResult)
                {
                    sceneDebugger.OnCellsGridChanged(cellsGrid);
                    await UniTask.WaitForSeconds(delay, cancellationToken: _cts.Token);
                }
            }


            // ======================================================== CRESCIMENTO LIVRE(espaços restantes)
            // while free spaces.

            // <<<<<<<<<< end main grow logic

            // Prepare the next set of zones to grow.
            foreach(Zone zone in _grownZones)
            {
                zone.Bake();

                if(zone.HasChildrenZones)
                {
                    _zonesToSubdivide.Add(zone);
                }
            }

            _grownZones.Clear();
        }
        

        EditorApplication.playModeStateChanged -= PlayModeStateChanged;

        sceneDebugger.OnCellsGridChanged(cellsGrid);
        //await UniTask.WaitForSeconds(delay, cancellationToken: _cts.Token, cancelImmediately: true);
        await UniTask.WaitForEndOfFrame();
        _cts.Dispose();

        return true;
    }


#region ========== GROWTH STEPS METHODS ==========

    /// <summary>
    /// 
    /// </summary>
    /// <param name="zone"></param>
    /// <param name="cellsGrid"></param>
    /// <returns></returns>
    bool GrowZoneRect(Zone zone, CellsGrid cellsGrid)
    {
        //return DEBUG_GrowSequential(zone, cellsGrid);

        if(zone.HasDesiredArea() && !_ignoreDesiredAreaInRect)
        {
            return false;
        }

        /*
        // INTERESSANTE, usar essa função sem checar espaço total gera formas retangulares, mas é mias custoso do q checar os aspect antes
        var largestFreeSpace = zone.GetLargestExpansionSpaceRect(cellsGrid, false);
        if(largestFreeSpace.isFullLine)
        {
            return zone.TryExpandShapeRect(largestFreeSpace.freeLineDescription.side, cellsGrid);
        }
        else
        {
            return false;
        }
        */

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


    /// <summary>
    /// 
    /// </summary>
    /// <param name="zone"></param>
    /// <param name="cellsGrid"></param>
    /// <returns></returns>
    bool GrowZoneLShape(Zone zone, CellsGrid cellsGrid)
    {
        //return GrowRectUntilLargestIsL(zone, cellsGrid);
        return GrowRectThenGrowL(zone, cellsGrid);
    }


    /// <summary>
    /// L expansion variation 1
    /// </summary>
    /// <param name="zone"></param>
    /// <param name="cellsGrid"></param>
    /// <returns></returns>
    bool GrowRectUntilLargestIsL(Zone zone, CellsGrid cellsGrid)
    {
        // expand L if is L
        if(zone.IsLShaped)
        {
            return zone.TryExpandShapeL();
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
            return zone.TryExpandShapeL();
        }
        else
        {
            return zone.TryExpandShapeRect(largestFreeSpace.freeLineDescription.Side);
        }
    }


    /// <summary>
    /// L expansion variation 2
    /// </summary>
    /// <param name="zone"></param>
    /// <param name="cellsGrid"></param>
    /// <returns></returns>
    bool GrowRectThenGrowL(Zone zone, CellsGrid cellsGrid)
    {
        // expand L if is L
        if(zone.IsLShaped)
        {
            return zone.TryExpandShapeL();
        }

        // Go on all sides randomly trying to expand
        var sides = Enum.GetValues(typeof(Zone.Side)).Cast<Zone.Side>().OrderBy(d => Guid.NewGuid());
        foreach(var side in sides)
        {
            if(zone.GetExpansionSpaceRect(side, cellsGrid).isFullLine)
            {
                if(zone.TryExpandShapeRect(side))
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
        return zone.TryExpandShapeL();

    }


    /// <summary>
    /// OBS: the direction paramenter is taking in mind a randomization of the order of try.
    /// </summary>
    /// <param name="side"></param>
    /// <param name="zone"></param>
    /// <param name="cellsGrid"></param>
    /// <returns></returns>
    bool TryGrowFromSide(Zone.Side side, Zone zone, CellsGrid cellsGrid)
    {
        if(zone.GetExpansionSpaceRect(side, cellsGrid, false).isFullLine)
        {
            return zone.TryExpandShapeRect(side);
        }
        
        return false;
    }

#endregion


#region ========== ZONE SELECTION ==========

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    List<Zone> GetNextZonesToGrowList(FloorPlanManager floorPlanManager) // TODO: "set" zones to grow list
    {
        if(_zonesToSubdivide.Count == 0)
        {
            return null;
        }

        List<Zone> childZones = _zonesToSubdivide[0].ChildZones.Values.ToList();
        _zonesToSubdivide.RemoveAt(0);

        for(int i = 0; i < childZones.Count; i++)
        {
            Zone zone = childZones[i];
            PlotFirstZoneCell(zone, childZones, floorPlanManager); // TODO: move to outside the method
        }

        return  childZones;
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="zonesToGrow"></param>
    void UpdateZonesWeights(List<Zone> zonesToGrow)
    {
        _zonesWeights = new WeightedArray(zonesToGrow.Count);

        for(int i = 0; i < zonesToGrow.Count; i++)
        {
            Zone zone = zonesToGrow[i];
            _zonesWeights.AddAt(i, zone.AreaRatio);
        }
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="zonesToGrow"></param>
    /// <returns></returns>
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

#endregion


#region ========== ZONES PLOTTING ==========

    /// <summary>
    /// 
    /// </summary>
    /// <param name="zone"></param>
    /// <param name="zonesToGrow"></param>
    void PlotFirstZoneCell(Zone zone, List<Zone> zonesToGrow, FloorPlanManager floorPlanManager)
    {
        if(zone.ParentZone != null)
        {
            // Weighted selection
            CalculateWeights(zone, zonesToGrow, floorPlanManager);
            
            #if DEBUG
            if(_cellsWeights.GetRandomWeightedElement(floorPlanManager.CellsGrid.Cells, out Cell cell))
            #else
            if(_cellsWeights.GetRandomWeightedElement(zone._parentZone.Cells, out Cell cell))
            #endif
            {
                //_floorPlanManager.AssignCellToZone(cell.GridPosition.x, cell.GridPosition.y, zone);
                floorPlanManager.AssignCellToZone(cell, zone);
            }
        }
        else
        {
            Vector2Int position = new Vector2Int(Utils.RandomRange(0, floorPlanManager.CellsGrid.Dimensions.x),
                                                 Utils.RandomRange(0, floorPlanManager.CellsGrid.Dimensions.y));
            
            floorPlanManager.AssignCellToZone(position.x, position.y, zone);
        }
    }


    /// <summary>
    /// -> a celula(ZONA) que vai ser colocada,
    /// -> as zonas ja exitentes, as bordas, as portas, as janelas(celulas presentes com caracteristicas que interferen no peso)
    /// <- peso da celula
    /// TODO: peso para entrada e zonas primas.
    /// TODO: se celulas mais distantes teram valores de borda sobre escritos, deve se ter um melhor desenpenho al checar peso de adjacencia antes e pular aqueles com valor 0(talvez).
    /// TODO: tratamento peso 0 final em todas celulas.
    /// </summary>
    /// <param name="zoneToPlot"></param>
    /// <param name="plottedZones"></param>
    void CalculateWeights(Zone zoneToPlot, List<Zone> plottedZones, FloorPlanManager floorPlanManager)
    {
        CellsGrid cellsGrid = floorPlanManager.CellsGrid;
        Zone parentZone = zoneToPlot.ParentZone;
        
        #if DEBUG
        Cell[] cellsToCalc = floorPlanManager.CellsGrid.Cells;
        #else
        Cell[] cellsToCalc =  parentZone.Cells;
        #endif

        _cellsWeights = new WeightedArray(cellsToCalc.Length);

        int desiredZoneSqSize = Mathf.CeilToInt(Mathf.Sqrt(zoneToPlot.AreaRatio * cellsGrid.Area)); // Ceiling to round up to a size that can fit it.
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

            
            // ================================== BORDERS WEIGHTS
            if(!_ignoreBorderWeights)
            {
                // Calculate the smaller distance from borders.
                float smallerDistance = float.MaxValue;
                foreach(Cell borderCell in parentZone.BorderCells)
                {
                    float distance = Mathf.Round((borderCell.GridPosition - cell.GridPosition).magnitude);
                    if(distance < smallerDistance) smallerDistance = distance;
                }

                // Calculate de weight from borders using the desired size of the zone sides on a function defined by a animation curve.
                if(smallerDistance < minimumBorderDistance || smallerDistance > desiredZoneSqSize)
                {
                    // Smaller distance > expected side size, the cell is too far.
                    weight = 0;
                }
                else
                {
                    // The value to be obtained is between a minimum and maximum based on zone to plot desired size.
                    weight = _borderDistanceCurve.Evaluate(smallerDistance / desiredZoneSqSize) * _borderWeightMultiplier; // += ?
                }
            }


            // ================================== GROWN BAKED ADJACENT UNCLE ZONES WEIGHTS
            // Calculate the weight for zones the are already baked that this one should be adjacent.
            // Because the way the algorithm iterates it will not guarantee that cousin zones can enter this, only uncle or older.
            if(!_ignoreAdjacentWeights)
            {
                foreach(Zone adjacentZone in zoneToPlot.AdjacentZones.Values)
                {
                    // Skip sister zone.
                    // TODO: Maybe can be done together with plotted zones phase, when using cousin zones in calculus.
                    if(zoneToPlot.IsSister(adjacentZone))
                    {
                        continue;
                    }

                    Zone zoneToCalculate;

                    // Adjacent zone is not full grown, skip it.
                    if(!adjacentZone.IsBaked)
                    {
                        if(!adjacentZone.ParentZone.IsBaked)
                        {
                            continue;
                        }
                        else
                        {
                            zoneToCalculate = adjacentZone.ParentZone;
                        }
                    }
                    else
                    {
                        zoneToCalculate = adjacentZone;
                    }

                    // Skip the zone if it is not a uncle zone. Will not interfere in sister adjacency since the sister must be
                    // not baked(only first cell plotted).
                    /*
                    if(adjacentZone.ParentZone != parentZone.ParentZone) // Uncle check
                    {
                        Debug.LogError("Adjacent baked zone is not uncle. Check the settings.");
                        continue;
                    }
                    */


                    float smallerDistance = float.MaxValue;
                    foreach(Cell borderCell in zoneToCalculate.BorderCells)
                    {
                        float distance = Mathf.Round((borderCell.GridPosition - cell.GridPosition).magnitude);
                        if(distance < smallerDistance) smallerDistance = distance;
                    }

                    if(smallerDistance < minimumBorderDistance || smallerDistance > desiredZoneSqSize)
                    {
                        weight = 0;
                    }
                    else
                    {
                        //weight = _borderDistanceCurve.Evaluate(smallerDistance / desiredZoneSqSize) * _borderWeightMultiplier;
                        //weight += _borderDistanceCurve.Evaluate(smallerDistance / desiredZoneSqSize) * _borderWeightMultiplier;
                        weight *= _borderDistanceCurve.Evaluate(smallerDistance / desiredZoneSqSize) * _borderWeightMultiplier;
                    }
                }
            }
            

            // ================================== PLOTTED SISTER ZONES WEIGHTS
            // Distance from adjacent or non adjacent.
            if(plottedZones != null)
            {
                foreach(Zone plottedZone in plottedZones)
                {
                    // Plotted zone actually is not plotted.
                    if(plottedZone.OriginCell == null) continue;

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
                            //weight -= _adjacencyDistanceCurve.Evaluate(normToTotalRadius);
                        }
                    }
                    else if(zoneToPlot.IsAdjacent(plottedZone)) // normToTotalRadius > 1
                    {
                        weight = 0; // Will prevent borders far away from adjacent zones to have weight.
                    }
                    // else don't change the weight.

                    weight = Mathf.Clamp(weight, 0, float.MaxValue);
                    weight = weight * _adjacencyWeightMultiplier;
                }
            }


            _cellsWeights.AddAt(i, weight);
        }

        Debug.Log($"=============<color=yellow>{zoneToPlot.ZoneId}</color>");
        Utils.PrintArrayAsGrid(cellsGrid.Dimensions.x, cellsGrid.Dimensions.y, _cellsWeights.Values);
    }

#endregion

    
#region ========== AUXILIARY METHODS ==========
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="state"></param>
    void PlayModeStateChanged(PlayModeStateChange state)
    {
        if(state == PlayModeStateChange.ExitingPlayMode)
        {
            _cts.Cancel();
        }
    }

#endregion
}

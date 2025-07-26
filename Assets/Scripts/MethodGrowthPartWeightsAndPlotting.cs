#define TEST

using System;
using System.Collections.Generic;
using UnityEngine;

namespace BuildingGenerator
{
    public partial class MethodGrowth
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="zone"></param>
        /// <param name="zonesToGrow"></param>
        void PlotFirstZoneCell(Zone zone, List<Zone> zonesToGrow, FloorPlanManager floorPlanManager)
        {
            Utils.Debug.DevLog($"Plotting first cell of: {zone.ZoneId}");

            if (zone.ParentZone != null)
            {
                // Weighted selection
                CalculateWeights(zone, zonesToGrow, floorPlanManager);

#if TEST
            if(_cellsWeights.GetRandomWeightedElement(floorPlanManager.CellsGrid.Cells, out Cell cell))
#else
                if (_cellsWeights.GetRandomWeightedElement(zone.ParentZone.Cells, out Cell cell))
#endif
                {
                    //_floorPlanManager.AssignCellToZone(cell.GridPosition.x, cell.GridPosition.y, zone);
                    Utils.Debug.DevLog($"Adding first cell of: {zone.ZoneId}");
                    floorPlanManager.AssignCellToZone(cell, zone);
                }
                else
                {
                    Utils.Debug.DevError($"{zone.ZoneId} can't get a cell from parent zone {zone.ParentZone.ZoneId}. Failed to plot zone. Parent have [{zone.ParentZone.Cells?.Length}] cells.");
                }
            }
            else
            {
                //Utils.ConsoleDebug.DevLog($"{zone.ZoneId} zone.ParentZone == null");
                Vector2Int position = new Vector2Int(Utils.Random.RandomRange(0, floorPlanManager.CellsGrid.Dimensions.x),
                                                     Utils.Random.RandomRange(0, floorPlanManager.CellsGrid.Dimensions.y));

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

#if TEST
        Cell[] cellsToCalc = floorPlanManager.CellsGrid.Cells;
#else
            Cell[] cellsToCalc = parentZone.Cells;
#endif

            _cellsWeights = new WeightedArray(cellsToCalc.Length);

            int desiredZoneSqSize = Mathf.CeilToInt(Mathf.Sqrt(zoneToPlot.AreaRatio * cellsGrid.Area)); // Ceiling to round up to a size that can fit it.
            int minimumBorderDistance = desiredZoneSqSize / 4;
            //int minimumBorderDistance = Mathf.CeilToInt(Mathf.Sqrt(desiredZoneSqSize)); // Mais falhas.
            

            bool hasAnAvailableCell = false; // Will be tru if at least on cell in the grid have a weight != of 0.

            for (int i = 0; i < cellsToCalc.Length; i++)
            {
                Cell cell = cellsToCalc[i];
                float weight = 0;

                // Skip weights.
                //_cellsWeights.AddAt(i, weight);
                //continue;

                // Cell outside the current parent zone, the context zone.
                // Safe check. For when testing using the full grid.
                if (cell.Zone != parentZone)
                {
                    _cellsWeights.AddAt(i, 0);
                    continue; // Next cell.
                }


                // ================================== BORDERS WEIGHTS
                if (!_settings.IgnoreBorderWeights)
                {
                    // Calculate the smaller distance from borders.
                    float smallerDistance = float.MaxValue;
                    foreach (Cell borderCell in parentZone.BorderCells)
                    {
                        float distance = Mathf.Round((borderCell.GridPosition - cell.GridPosition).magnitude);
                        if (distance < smallerDistance) smallerDistance = distance;
                    }

                    // Calculate de weight from borders using the desired size of the zone sides on a function defined by a animation curve.
                    if (smallerDistance < minimumBorderDistance || smallerDistance > desiredZoneSqSize)
                    {
                        // Smaller distance > expected side size, the cell is too far.
                        weight = 0;
                        //weight = _settings.BorderDistanceCurve.Evaluate(smallerDistance / desiredZoneSqSize) * _settings.BorderWeightMultiplier;
                    }
                    else
                    {
                        // The value to be obtained is between a minimum and maximum based on zone to plot desired size.
                        weight = _settings.BorderDistanceCurve.Evaluate(smallerDistance / desiredZoneSqSize) * _settings.BorderWeightMultiplier; // += ?
                    }
                }


                // ================================== BAKED ADJACENT UNCLE ZONES WEIGHTS
                // Calculate the weight for zones the are already baked that this one should be adjacent.
                // Because the way the algorithm iterates it will not guarantee that cousin zones can enter this, only uncle or older.
                if (!_settings.IgnoreAdjacentWeights)
                {
                    foreach (Zone adjacentZone in zoneToPlot.AdjacentZones.Values)
                    {
                        // Skip sister zone.
                        // TODO: Maybe can be done together with plotted zones phase, when using cousin zones in calculus.
                        // TODO: Not sure if should always be skipped. Sister zones can have a predefined area what can result in unexpected
                        // weights around the plotted predefined sister. In sister weight add if sister is baked, calculate weights of borders.
                        if (zoneToPlot.IsSister(adjacentZone))
                        {
                            continue;
                        }

                        Zone zoneToCalculate;

                        // Adjacent zone is not full grown, skip it.
                        if (!adjacentZone.IsBaked)
                        {
                            if (!adjacentZone.ParentZone.IsBaked)
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
                        foreach (Cell borderCell in zoneToCalculate.BorderCells)
                        {
                            float distance = Mathf.Round((borderCell.GridPosition - cell.GridPosition).magnitude);
                            if (distance < smallerDistance) smallerDistance = distance;
                        }

                        if (smallerDistance < minimumBorderDistance || smallerDistance > desiredZoneSqSize)
                        {
                            weight = 0;
                        }
                        else
                        {
                            //weight = _borderDistanceCurve.Evaluate(smallerDistance / desiredZoneSqSize) * _borderWeightMultiplier;
                            //weight += _borderDistanceCurve.Evaluate(smallerDistance / desiredZoneSqSize) * _borderWeightMultiplier;
                            weight *= _settings.BorderDistanceCurve.Evaluate(smallerDistance / desiredZoneSqSize) * _settings.BorderWeightMultiplier;
                        }
                    }
                }


                // ================================== PLOTTED SISTER* ZONES WEIGHTS
                // * Is expected that "plottedZones" have the sisters of a zone since the plot phase happens over the children of a zone,
                // not over multiple zones or hierarchy levels.

                // Calculate the weights of zones that have only the initial cell plotted, adjacent or not.
                if (plottedZones != null)
                {
                    foreach (Zone plottedZone in plottedZones)
                    {
                        // Plotted zone actually is not plotted.
                        if (plottedZone.OriginCell == null) continue;

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
                        if (normToPlottedRadius < 1) // To close of a plotted zone
                        {
                            weight = 0;
                            break; // Don't need to check other plotted zones.
                        }
                        else if (normToTotalRadius <= 1)
                        {
                            if (zoneToPlot.MustBeAdjacentTo(plottedZone))
                            {
                                weight *= _settings.AdjacencyDistanceCurve.Evaluate(normToTotalRadius);
                            }
                            else
                            {
                                //weight -= _adjacencyDistanceCurve.Evaluate(normToTotalRadius);
                            }
                        }
                        else if (zoneToPlot.MustBeAdjacentTo(plottedZone)) // normToTotalRadius > 1
                        {
                            weight = 0; // Will prevent borders far away from adjacent zones to have weight.
                        }
                        // else don't change the weight.

                        weight = Mathf.Clamp(weight, 0, float.MaxValue);
                        weight = weight * _settings.AdjacencyWeightMultiplier;
                    }
                }

                weight = Mathf.Clamp(weight, 0, float.MaxValue);
                _cellsWeights.AddAt(i, weight);

                if (weight != 0 && hasAnAvailableCell == false)
                {
                    hasAnAvailableCell = true;
                }
            }


            GenerationStats.Instance._zonePlotWeightGrids.Add(new GenerationStats.NamedGrid(zoneToPlot.ZoneId, _cellsWeights.Values, cellsGrid.Dimensions.x));


            if (!hasAnAvailableCell)
            {
                //UnityEngine.Debug.LogWarning($"{zoneToPlot.ZoneId} can't be plotted, setting all valid positions to weight 1 to continue the execution. Please try changing the area ratios.");

                for (int i = 0; i < cellsToCalc.Length; i++)
                {
                    if (cellsToCalc[i].Zone == parentZone)
                    {
                        _cellsWeights.AddAt(i, 1);

                        if (hasAnAvailableCell == false)
                        {
                            hasAnAvailableCell = true;
                        }
                    }
                }
            }

            if (!hasAnAvailableCell)
            {
                Utils.Debug.DevError($"No valid positions to plot the zone {zoneToPlot.ZoneId}.");
            }

            //Debug.Log($"=============<color=yellow>{zoneToPlot.ZoneId}</color>");
            //Utils.PrintArrayAsGrid(cellsGrid.Dimensions.x, cellsGrid.Dimensions.y, _cellsWeights.Values);
        }
    }
}

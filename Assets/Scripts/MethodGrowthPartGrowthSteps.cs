using System;
using System.Linq;

namespace BuildingGenerator
{
    public partial class MethodGrowth
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="zone"></param>
        /// <param name="cellsGrid"></param>
        /// <returns></returns>
        bool GrowZoneRect(Zone zone, CellsGrid cellsGrid)
        {
            //return DEBUG_GrowSequential(zone, cellsGrid);

            if (zone.HasDesiredArea() && !_settings.IgnoreDesiredAreaInRect)
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
            //var sides = Enum.GetValues(typeof(Zone.Side)).Cast<Zone.Side>().OrderBy(d => Guid.NewGuid());// NOT AFFECTED BY RANDOM SEED.
            var sides = Enum.GetValues(typeof(Zone.Side)).Cast<Zone.Side>();

            foreach (var side in sides)
            {
                // All directions
                if (aspect == zone.DesiredAspect)
                {
                    if (TryGrowFromSide(side, zone, cellsGrid))
                    {
                        return true;
                    }
                }
                // Vertical
                else if (aspect > zone.DesiredAspect && (side == Zone.Side.Top || side == Zone.Side.Bottom))
                {
                    if (TryGrowFromSide(side, zone, cellsGrid))
                    {
                        return true;
                    }
                }
                // Horizontal
                else if (aspect < zone.DesiredAspect && (side == Zone.Side.Left || side == Zone.Side.Right))
                {
                    if (TryGrowFromSide(side, zone, cellsGrid))
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
            if (zone.IsLShaped)
            {
                return zone.TryExpandShapeL(true);
            }

            var largestFreeSpace = zone.GetLargestExpansionSpaceRect(false);

            // No side to expand
            if (largestFreeSpace.distance == 0)
            {
                return false;
            }

            // Check side is only part of a border, if is set to expand L, else expand rect.
            if (!largestFreeSpace.isFullLine && largestFreeSpace.freeLineDescription.NumberOfCells >= _settings.MinLCorridorWidth)
            {
                zone.SetAsLShaped(largestFreeSpace.freeLineDescription);
                return zone.TryExpandShapeL(true);
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
            if (zone.IsLShaped)
            {
                return zone.TryExpandShapeL(true);
            }

            // Go on all sides randomly trying to expand
            //var sides = Enum.GetValues(typeof(Zone.Side)).Cast<Zone.Side>().OrderBy(d => Guid.NewGuid());
            var sides = Enum.GetValues(typeof(Zone.Side)).Cast<Zone.Side>();
            foreach (var side in sides)
            {
                if (zone.GetExpansionSpaceRect(side, false).isFullLine)
                {
                    if (zone.TryExpandShapeRect(side))
                    {
                        return true;
                    }
                }
            }

            // If wasn't able to expand any side, try to start L
            var largestFreeSide = zone.GetLargestExpansionSpaceRect(false);
            if (largestFreeSide.distance == 0) // No free side
            {
                return false;
            }
            if (largestFreeSide.isFullLine) // Just in case
            {
                UnityEngine.Debug.LogWarning("At this point it should not have a full border available.");
            }

            if (largestFreeSide.freeLineDescription.NumberOfCells >= _settings.MinLCorridorWidth)
            {
                zone.SetAsLShaped(largestFreeSide.freeLineDescription);
                return zone.TryExpandShapeL(true);
            }

            return false;
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
            if (zone.GetExpansionSpaceRect(side, false).isFullLine)
            {
                return zone.TryExpandShapeRect(side);
            }

            return false;
        }
    }
}

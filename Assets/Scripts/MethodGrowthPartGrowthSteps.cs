using System;
using System.Linq;

namespace BuildingGenerator
{
    public partial class MethodGrowth
    {
        /// <summary>
        /// TODO: randomize if same space.
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
            
            float aspect = zone.GetZoneAspect();

            var sides = Enum.GetValues(typeof(Zone.Side)).Cast<Zone.Side>();

            // All directions
            if (aspect == zone.DesiredAspect)
            {
                Zone.Side largestSide = default;
                int distance = 0;

                foreach (var side in sides)
                {
                    var line = zone.GetExpansionSpaceRect(side, _checkFullSpace);
                    if (line.distance > distance)
                    {
                        largestSide = side;
                        distance = line.distance;
                    }
                }

                if (TryGrowFromSide(largestSide, zone, cellsGrid))
                {
                    return true;
                }
            }
            // Vertical
            else if (aspect > zone.DesiredAspect)
            {
                var lineTop = zone.GetExpansionSpaceRect(Zone.Side.Top, _checkFullSpace);
                var lineBottom = zone.GetExpansionSpaceRect(Zone.Side.Bottom, _checkFullSpace);

                if (lineTop.distance > lineBottom.distance)
                {
                    if (TryGrowFromSide(Zone.Side.Top, zone, cellsGrid))
                    {
                        return true;
                    }
                }
                else if (TryGrowFromSide(Zone.Side.Bottom, zone, cellsGrid))
                {
                    return true;
                }
            }
            // Horizontal
            else if (aspect < zone.DesiredAspect)
            {
                var lineLeft = zone.GetExpansionSpaceRect(Zone.Side.Left, _checkFullSpace);
                var lineRight = zone.GetExpansionSpaceRect(Zone.Side.Right, _checkFullSpace);

                if (lineLeft.distance > lineRight.distance)
                {
                    if (TryGrowFromSide(Zone.Side.Left, zone, cellsGrid))
                    {
                        return true;
                    }
                }
                else if (TryGrowFromSide(Zone.Side.Right, zone, cellsGrid))
                {
                    return true;
                }
            }

            return false;
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
        /// L expansion variation 1. BROKEN
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

            var largestFreeSpace = zone.GetLargestExpansionSpaceRect(_checkFullSpace);

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
                //return false;
                return zone.TryExpandShapeL(true);
            }

            // Go on all sides randomly trying to expand
            // TODO: get largest space
            //var sides = Enum.GetValues(typeof(Zone.Side)).Cast<Zone.Side>().OrderBy(d => Guid.NewGuid());
            var sides = Enum.GetValues(typeof(Zone.Side)).Cast<Zone.Side>();
            foreach (var side in sides)
            {
                if (zone.GetExpansionSpaceRect(side, _checkFullSpace).isFullLine)
                {
                    if (zone.TryExpandShapeRect(side))
                    {
                        return true;
                    }
                }
            }

            // If wasn't able to expand any side, try to start L
            var largestFreeSide = zone.GetLargestExpansionSpaceRect(_checkFullSpace);
            if (largestFreeSide.distance == 0) // No free side
            {
                return false;
            }
            if (largestFreeSide.isFullLine) // Just in case
            {
                Utils.Debug.DevWarning("At this point it should not have a full border available.");
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
            if (zone.GetExpansionSpaceRect(side, _checkFullSpace).isFullLine)
            {
                return zone.TryExpandShapeRect(side);
            }

            return false;
        }
    }
}

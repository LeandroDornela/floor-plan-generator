using System.Collections.Generic;

namespace BuildingGenerator
{
    /*
    Zone selection methods are used when expanding a zone. A zone needs to be selected for expansion based on
    rules like distance to the desired zone area and reaching the desired size/growth limit.
    */
    public partial class MethodGrowth
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        List<Zone> GetNextZonesToGrowList(FloorPlanManager floorPlanManager) // TODO: "set" zones to grow list
        {
            if (_zonesToSubdivide.Count == 0)
            {
                return null;
            }

            //List<Zone> childZones = _zonesToSubdivide[0].ChildZones.Values.ToList();
            List<Zone> childZones = new List<Zone>();

            //for(int i = 0; i < childZones.Count; i++)
            foreach (var child in _zonesToSubdivide[0].ChildZones.Values)
            {
                Zone zone = child;

                // Check if the child is already baked. What means that it has a predefined
                // area (Why? were is were you set the zones to grow, if its baked, it can't grow).
                if (zone.IsBaked)
                {
                    _zonesToSubdivide.Add(zone);
                }
                else
                {
                    childZones.Add(zone);
                    PlotFirstZoneCell(zone, childZones, floorPlanManager); // TODO: move to outside the method
                }
            }

            _zonesToSubdivide.RemoveAt(0);

            return childZones;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="zonesToGrow"></param>
        void UpdateZonesWeights(List<Zone> zonesToGrow)
        {
            _zonesWeights = new WeightedArray(zonesToGrow.Count);

            for (int i = 0; i < zonesToGrow.Count; i++)
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

            if (_zonesWeights.GetRandomWeightedElement(zonesToGrow.ToArray(), out Zone zone))
            {
                return zone;
            }

            Utils.Debug.DevError("Unable to get a zone.");
            return null;
        }
    }
}

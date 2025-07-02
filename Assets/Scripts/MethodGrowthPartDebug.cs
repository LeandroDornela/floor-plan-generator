
namespace BuildingGenerator
{
    public partial class MethodGrowth
    {
        public bool DEBUG_GrowSequential(Zone zone, CellsGrid cellsGrid)
        {
            if (TryGrowFromSide(Zone.Side.Right, zone, cellsGrid))
                return true;
            if (TryGrowFromSide(Zone.Side.Left, zone, cellsGrid))
                return true;
            if (TryGrowFromSide(Zone.Side.Top, zone, cellsGrid))
                return true;
            if (TryGrowFromSide(Zone.Side.Bottom, zone, cellsGrid))
                return true;
            return false;
        }
    }
}
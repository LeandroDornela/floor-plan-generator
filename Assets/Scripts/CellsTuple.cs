
namespace BuildingGenerator
{
    public class CellsTuple
    {
        private Cell _cellA;
        private Cell _cellB;
        private bool _hasDoor;

        public Cell CellA => _cellA;
        public Cell CellB => _cellB;
        public bool HasDoor => _hasDoor;


        public CellsTuple(Cell cellA, Cell cellB, bool hasDoor)
        {
            _cellA = cellA;
            _cellB = cellB;
            _hasDoor = hasDoor;
        }
    }
}
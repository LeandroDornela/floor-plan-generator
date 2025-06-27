
using System.Collections.Generic;

namespace BuildingGenerator
{
    public class CellsTuple
    {
        private Cell _cellA;
        private Cell _cellB;
        private bool _hasDoor;
        private bool _isOutsideBorder;

        public Cell CellA => _cellA;
        public Cell CellB => _cellB;
        public bool HasDoor => _hasDoor;
        public bool IsOutsideBorder => _isOutsideBorder;


        public Dictionary<string, string> _debugDict;


        public CellsTuple(Cell cellA, Cell cellB, bool hasDoor = false, bool outsideBorder = false)
        {
            _cellA = cellA;
            _cellB = cellB;
            _hasDoor = hasDoor;
            _isOutsideBorder = outsideBorder;

            _debugDict = new Dictionary<string, string>();
        }


        public void SetHasDoor(bool value)
        {
            _hasDoor = value;
        }

        public void SetOutsideBorder(bool value)
        {
            _isOutsideBorder = value;
        }
    }
}
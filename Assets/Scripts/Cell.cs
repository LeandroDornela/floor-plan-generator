using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Cell // dando preferencia para classes para passar por ref
{
    public Zone _zone;
    public Vector2Int _gridPosition;
    
    public VisualCell visualCell;

    public Cell(int gridPositionX, int gridPositionY, Zone zone = null)
    {
        _gridPosition = new Vector2Int(gridPositionX, gridPositionY);
        _zone = zone;
    }

    public void SetZone(Zone newZone)
    {
        _zone = newZone;
    }

    public bool IsInZone(Zone zoneToCheck)
    {
        return zoneToCheck == _zone;
    }
}

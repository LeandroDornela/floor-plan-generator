using System.Collections.Generic;
using UnityEngine;


public class Cell // dando preferencia para classes para passar por ref
{
    private Zone _zone;
    private Vector2Int _gridPosition;

    public Dictionary<string, string> atributos;

    public Zone Zone => _zone;
    public Vector2Int GridPosition => _gridPosition;


    public Cell(int gridPositionX, int gridPositionY, Zone zone = null)
    {
        _gridPosition = new Vector2Int(gridPositionX, gridPositionY);
        _zone = zone;

        atributos = new Dictionary<string, string>();
    }

    public void SetZone(Zone newZone)
    {
        _zone = newZone;
    }

    public bool IsInZone(Zone zoneToCheck)
    {
        return zoneToCheck == _zone;
    }

    public List<Zone> GetParentZonesHierarchy()
    {
        List<Zone> parentZones = new List<Zone>();

        Zone currentZone = _zone;
        while(currentZone != null)
        {
            parentZones.Add(currentZone);
            currentZone = currentZone._parentZone;
        }

        return parentZones;
    }
}

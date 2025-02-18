using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Zone // similar a uma estrutura de nos em arvore
{
    // Runtime
    public List<Cell> _cells; // Celulas atualmente associadas a zona.
    // Runtime const
    private string _zoneId;
    public Zone _parentZone; // A zona m�e pode ser usada para verificar se uma celula est� na mesma zona que outra.
    public List<Zone> _childZones;
    public List<Zone> _adjacentZones;

  

    public string ZoneId => _zoneId;

    public Zone(string zoneId)
    {
        _zoneId = zoneId;
        _parentZone = null;
        _cells = new List<Cell>();
        _childZones = new List<Zone>();
        _adjacentZones = new List<Zone>();
    }


    public Zone(string zoneId, Zone parentZone)
    {
        _zoneId = zoneId;
        _parentZone = parentZone;

        _cells = new List<Cell>();
    }


    public void AddCell(Cell cell)
    {
        if(cell == null)
        {
            Debug.LogError("Tryng to add a null cell.");
            return;
        }

        // TODO: evitar essa verificação, pode ser custoso. Fazer de forma que a culala não esteja na zona.
        if(_cells.Contains(cell))
        {
            Debug.LogError("Tryng to add an exiting cell.");
            return;
        }

        _cells.Add(cell);
        cell.SetZone(this);
    }

    public void RemoveCell(Cell cell)
    {
        if(cell == null)
        {
            Debug.LogError("Tryng to remove a null cell.");
            return;
        }

        if(_cells.Contains(cell))
        {
            _cells.Remove(cell);
            cell.SetZone(null);
        }
        else
        {
            Debug.LogError("The cell isn't in the zone.");
        }
    }


    public void AddChildZone(Zone childZone)
    {
        if(childZone == null)
        {
            Debug.LogError("Tryng to add a null child.");
            return;
        }

        if(!_childZones.Contains(childZone))
        {
            _childZones.Add(childZone);
        }
        else
        {
            Debug.LogWarning("Tryng to add an exiting child.");
        }
    }


    public void AddAdjacentZone(Zone adjacentZone)
    {
        //TODO: Check if adj. zone is already set.
        _adjacentZones.Add(adjacentZone);
    }


    public void SetParentZone(Zone parentZone)
    {
        if(_parentZone == null)
        {
            _parentZone = parentZone;
        }
        else
        {
            Debug.LogError("Parent zone can't be overridden.");
        }
    }
}

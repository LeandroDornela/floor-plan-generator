using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Zone // similar a uma estrutura de nos em arvore
{
    // Runtime
    public List<Cell> _cells; // Celulas atualmente associadas a zona.
    // Runtime const
    private string _zoneId;
      public Color _color;
    public Zone _parentZone; // A zona m�e pode ser usada para verificar se uma celula est� na mesma zona que outra.
    public List<Zone> _childZones;
    public List<Zone> _adjacentZones;
    //public bool _isLeaf = false; // Ou isRoom.

  

    public string ZoneId => _zoneId;

    public Zone(string zoneId, Color color)
    {
        _zoneId = zoneId;
        _color = color;
        _parentZone = null;
        _cells = new List<Cell>();
        _childZones = new List<Zone>();
        _adjacentZones = new List<Zone>();
    }


    public Zone(string zoneId, Zone parentZone)
    {
        //_config = config;
        _zoneId = zoneId;
        _parentZone = parentZone;

        /*
        if(config.Subzones.Length == 0)
        {
            _isLeaf = true;
        }
        */

        _cells = new List<Cell>();
    }


    public void AddCell(Cell cell)
    {
        // TODO: checar se a celula ja esta na zona.
        _cells.Add(cell);
        //cell.SetZone(this);
    }


    public void AddChildZone(Zone childZone)
    {
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

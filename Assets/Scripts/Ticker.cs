using System.Collections.Generic;
using UnityEngine;

public class Ticker : MonoBehaviour
{

    public Generator generator;
    public float repeatRate = 0.5f;

    private int counter = 0;

    private int WIDTH;
    private int HEIGTH;

    private int x;
    private int y;

    private List<Cell> _cells;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        WIDTH = generator._generatorConfig.GridDimensions.x;
        HEIGTH = generator._generatorConfig.GridDimensions.y;

        // Colore a primeira celula
        _cells = new List<Cell>();
        
        SetZoneToCell(2,3,0);
        SetZoneToCell(11,5,1);
        SetZoneToCell(7,11,2);

        InvokeRepeating("Tick", 1, repeatRate);
    }

    void SetZoneToCell(int x, int y, int zoneIdIndex)
    {
        generator._cellsGrid.GetCell(x, y, out Cell cell);
        Zone zone;
        zone = generator._zonesHierarchy._zonesTree[zoneIdIndex];
        cell.SetZone(zone);
        generator._debugger.SetCellColor(cell._gridPosition.x, cell._gridPosition.y, zone.ZoneId);
        _cells.Add(cell);
    }

    void Tick()
    {
        //Temp_linear();
        Temp_Flood();
    }

    void Temp_linear()
    {
        // mover para uma função em outra classe que funciona sozinho
        Debug.Log(counter);
        x = counter % WIDTH;
        y = counter / WIDTH;
        Debug.Log("x: " + x + " y: " + y);

        Cell cell;
        Zone zone;
        generator._cellsGrid.GetCell(x, y, out cell);
        zone = generator._zonesHierarchy._zonesTree[0];
        cell.SetZone(zone);
        cell.visualCell.SetColor(zone._color);

        counter++;
    }


    void Temp_Flood()
    {
        if(_cells.Count == 0) return;

        TrySetNeig(1, 0);
        TrySetNeig(-1, 0);
        TrySetNeig(0, 1);
        TrySetNeig(0, -1);

        _cells.RemoveAt(0);
    }


    void Temp_Flood_2()
    {
        if(_cells.Count == 0) return;

        Zone zone;
        zone = generator._zonesHierarchy._zonesTree[0];
        _cells[0].SetZone(zone);


        generator._debugger.SetCellColor(_cells[0]._gridPosition.x, _cells[0]._gridPosition.y, zone.ZoneId);


        Cell vizinho;
        for(int x = -1; x <= 1; x++)
        {
            for(int y = -1; y <= 1; y++)
            {
                if(x == y || x + y == 0) continue;

                if(generator._cellsGrid.GetCell(_cells[0]._gridPosition.x + x, _cells[0]._gridPosition.y + y, out vizinho))
                {
                    if(!vizinho.atributos.TryGetValue("parede", out string str))
                    {
                        if(!vizinho.tag)
                        {
                            _cells.Add(vizinho);
                            vizinho.tag = true;
                        }
                    }
                }
            }
        }

        _cells.RemoveAt(0);
    }

    void TrySetNeig(int x, int y)
    {
        Cell vizinho;
        if(generator._cellsGrid.GetCell(_cells[0]._gridPosition.x + x, _cells[0]._gridPosition.y + y, out vizinho))
        {
            // Pula se vizinho ja tem zona.
            if(vizinho._zone != null) return;

            // Muda zona do vizinho
            Zone zone = _cells[0]._zone;
            vizinho.SetZone(zone);
            generator._debugger.SetCellColor(_cells[0]._gridPosition.x + x, _cells[0]._gridPosition.y + y, zone.ZoneId);

            // Add vizinho para checagem dos seus vizinhos
            _cells.Add(vizinho);
        }
    }
}

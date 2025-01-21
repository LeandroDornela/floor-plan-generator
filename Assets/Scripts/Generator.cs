using com.cyborgAssets.inspectorButtonPro;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

/*
-Root
    -Exterior
	-Interior
		-Publico
			-Sala
			-Cozinha
		-Privado
			-Quarto
			-Banheiro
*/



public class Generator : MonoBehaviour
{
    //[NaughtyAttributes.Expandable]
    public GeneratorConfig _generatorConfig;
    //[NaughtyAttributes.Expandable]
    public ZoneHierarchyConfig _hierarchyConfig;

    public CellsGrid _cellsGrid;
    //public Grid<float> _weightsGrid;

    public GridVisualDebugger _debugger;

    public ZoneHierarchy _zonesHierarchy;

    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        _cellsGrid = new CellsGrid(_generatorConfig.GridDimensions);
        //_weightsGrid = new Grid<float>(_generatorConfig.GridDimensions, 0);

        Generate();
    }

    public void TickGerador()
    {
        Debug.Log("Tick generator");

    }

    // para cada cada zona em cada nivel executar o algoritmo completo
    [ProButton]
    async void Generate()
    {

        _zonesHierarchy = _hierarchyConfig.GetZoneHierarchy();

        RandomSetCells(_zonesHierarchy._zonesTree);

        PrintAdjacencies();

        await _debugger.CreateVisualGrid(_cellsGrid);
    }


    public void RandomSetCells(List<Zone> zones)
    {
        foreach(Cell cell in _cellsGrid._cells)
        {
            Zone zone = zones[Random.Range(1, zones.Count)];
            cell.SetZone(zone);
            zone.AddCell(cell);
        }
    }


    public void SpawnZones(List<Zone> zones)
    {
        foreach (Zone zone in zones)
        {
            
        }
    }


    public void PrintAdjacencies()
    {
        string result;

        foreach (var zone in _zonesHierarchy._zonesDictionary)
        {
            result = $"{zone.Value.ZoneId}:";
            foreach (var adjacency in zone.Value._adjacentZones)
            {
                result = $"{result} {adjacency.ZoneId}";
            }
            Debug.Log(result);
        }
    }


    [ProButton]
    public void SelectZone(string zoneId)
    {
        Debug.Log("Select: " +  zoneId);
        _debugger.HighlightZone(_zonesHierarchy._zonesDictionary[zoneId]._cells);
    }
}

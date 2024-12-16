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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _cellsGrid = new CellsGrid(_generatorConfig.GridDimensions);
        //_weightsGrid = new Grid<float>(_generatorConfig.GridDimensions, 0);
    }

    // para cada cada zona em cada nivel executar o algoritmo completo
    [ProButton]
    async void Generate()
    {

        List<Zone> _root = _hierarchyConfig.GetZoneHierarchy();

        RandomSetCells(_root);

        await _debugger.CreateVisualGrid(_cellsGrid);
    }


    public void RandomSetCells(List<Zone> zones)
    {
        foreach(Cell cell in _cellsGrid._cells)
        {
            cell.SetZone(zones[Random.Range(0, zones.Count)]);
        }
    }


    public void SpawnZones(List<Zone> zones)
    {
        foreach (Zone zone in zones)
        {
            
        }
    }
}

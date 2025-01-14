using AYellowpaper.SerializedCollections;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;


public class GridVisualDebugger : MonoBehaviour
{
    public GameObject _cellPrefab;
    public List<VisualCell> _cells;
    public int _delay = 100;
    [SerializedDictionary("Zone, Adjacencies")]
    public SerializedDictionary<string, Color> _debugColors;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public async void Regenerate(CellsGrid grid)
    {
        foreach (var cell in _cells)
        {
            Destroy(cell.gameObject);
        }

        _cells.Clear();

        await CreateVisualGrid(grid);
    }


    public async Task CreateVisualGrid(CellsGrid grid)
    {
        _cells = new List<VisualCell>();

        for (int i = 0; i < grid.Dimmensions.y; i++)
        {
            RunOverRoll(grid, i);

            //await Task.Delay(_delay);
        }
    }

    async void RunOverRoll(CellsGrid grid, int collum)
    {
        Cell cell;

        for (int j = 0; j < grid.Dimmensions.x; j++)
        {
            grid.GetCell(collum, j, out cell);
            VisualCell newVisualCell = Instantiate(_cellPrefab, new Vector3(collum, j, 0), Quaternion.identity, transform).GetComponent<VisualCell>();
            newVisualCell._cell = cell;
            cell.visualCell = newVisualCell;
            _cells.Add(newVisualCell);

            if (cell._zone == null)
            {
                Debug.LogWarning("Cell has no zone.");
                _cells[_cells.Count - 1].Init(cell);
                _cells[_cells.Count - 1].SetColor(Color.black);
            }
            else
            {
                _cells[_cells.Count - 1].SetColor(_debugColors[cell._zone.ZoneId]);
            }

            //await Task.Delay(_delay);
        }
    }


    private void OnDrawGizmos()
    {
        Vector3 size = Vector3.one;
        foreach (var cell in _cells)
        {
            Handles.Label(cell.transform.position, $"({cell.transform.position.x}, {cell.transform.position.y})");
            //Gizmos.color = _debugColors[cell._cell._zone.ZoneId];
            //Gizmos.DrawCube(cell.transform.position, size);
        }
    }

    
    public async Task UpdateGrid(CellsGrid grid)
    {
        foreach (var cell in _cells)
        {
            // SE A EXECU��O FOR ASSINCRONA N�O HA GARANTIA DE QUE AS CELULAR FOR COLOCADAS NA ORDEM
            //Vector2Int coord = grid
            //cell.SetColor(grid._cells[i]._zone.Config.DebugColor);

            //await Task.Delay(_delay);
        }
    }


    public void HighlightZone(List<Cell> cells)
    {
        if(cells.Count == 0)
        {
            Debug.LogWarning("No cells!");
        }

        foreach(var cell in _cells)
        {
            if(cell._cell == null)
            {
                Debug.LogError("No cell.");
            }

            if (cells.Contains(cell._cell))
            {
                cell.SetSelectedState(true);
            }
            else
            {
                cell.SetSelectedState(false);
            }
        }
    }
}

using AYellowpaper.SerializedCollections;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;
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
            _cells.Add(Instantiate(_cellPrefab, new Vector3(collum, j, 0), Quaternion.identity, transform).GetComponent<VisualCell>());

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

    /*
    public async Task UpdateGrid(Grid<Cell> grid)
    {
        for (int i = 0; i < _cells.Count; i++)
        {
            // SE A EXECUÇÃO FOR ASSINCRONA NÃO HA GARANTIA DE QUE AS CELULAR FOR COLOCADAS NA ORDEM
            _cells[i].SetColor(grid._cells[i]._zone.Config.DebugColor);

            //await Task.Delay(_delay);
        }
    }
    */
}

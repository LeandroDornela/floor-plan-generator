using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEngine;


public sealed class FloorPlanGenSceneDebugger : MonoBehaviour
{
    public SerializedDictionary<string, Color> _debugColors;
    private FloorPlanGenerator _floorPlanGenerator;
    private Vector2Int _dimmensions;
    [SerializeField] private GameObject _cellGraphicsPrefab;
    private List<VisualCell> _cellsGraphicsInstances;
    private bool _initialized = false;

    private string _gridPreview;



    public void Init(FloorPlanGenerator floorPlanGenerator, FloorPlanConfig floorPlanConfig)
    {
        if(_initialized) return;

        Debug.Log("Initializing floor plan generator scene debugger.");

        _floorPlanGenerator = floorPlanGenerator;

        _dimmensions = floorPlanConfig.GridDimensions;

        InstantiateCellsGraphics();

        _initialized = true;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    void InstantiateCellsGraphics()
    {
        // Checa se existen celulas instanciadas, as destroi e limpa a lista.
        if(_cellsGraphicsInstances != null)
        {
            foreach (var cell in _cellsGraphicsInstances)
            {
                Destroy(cell.gameObject);
            }

            _cellsGraphicsInstances.Clear();
        }

        _cellsGraphicsInstances = new List<VisualCell>();

        // Instancia as celulas.
        foreach (var cell in _floorPlanGenerator._floorPlanManager.CellsGrid.Cells)
        {
            VisualCell visualCell = Instantiate(_cellGraphicsPrefab,
                                                new Vector3(cell.GridPosition.x, 0, -cell.GridPosition.y),
                                                Quaternion.identity,
                                                transform).GetComponent<VisualCell>();
            
            visualCell.Init(cell);
            
            if(cell.Zone != null)
            {
                visualCell.SetColor(_debugColors[cell.Zone.ZoneId]);
            }
            else
            {
                visualCell.SetColor(Color.black);
            }
            _cellsGraphicsInstances.Add(visualCell);
        }
    }


    public void OnCellsGridChanged(CellsGrid cellsGrid)
    {
        if(!_initialized) { return; }

        _gridPreview = _floorPlanGenerator._floorPlanManager.CellsGrid.GridToString();
        
        for(int i = 0; i < cellsGrid.Cells.Length; i++)
        {
            Zone cellZone = cellsGrid.Cells[i].Zone;
            if(cellZone != null)
            {
                _cellsGraphicsInstances[i].SetColor(_debugColors[cellZone.ZoneId]);
            }
            else
            {
                _cellsGraphicsInstances[i].SetColor(Color.black);
            }
        }
    }


    public void OnCellChanged(Cell cell)
    {
        if(!_initialized) { return; }

        _gridPreview = _floorPlanGenerator._floorPlanManager.CellsGrid.GridToString();
        int index = Utils.MatrixToArrayIndex(cell.GridPosition.x, cell.GridPosition.y, _dimmensions.x);
        _cellsGraphicsInstances[index].SetColor(_debugColors[cell.Zone.ZoneId]);
    }


    void OnDrawGizmos()
    {
        if(!_initialized) { return; }
        //Handles.Label(Vector3.zero, _gridPreview);
    }
}

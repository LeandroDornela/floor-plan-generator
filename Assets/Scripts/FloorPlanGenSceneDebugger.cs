using System;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEngine;

namespace BuildingGenerator
{
public class FloorPlanGenSceneDebugger : MonoBehaviour
{
    [SerializeField] private GameObject _cellGraphicsPrefab;

    private string _currentFloorPlanId;
    private SerializedDictionary<string, Color> _zoneColors;
    private List<VisualCell> _cellsGraphicsInstances;

    //private string _gridPreview;

    private static FloorPlanGenSceneDebugger _instance;
    public static FloorPlanGenSceneDebugger Instance => _instance;



    private void Awake()
    {
        if(_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="floorPlan"></param>
    public void OnFloorPlanUpdated(FloorPlanManager floorPlan)
    {
        if(_currentFloorPlanId != floorPlan.FloorPlanId)
        {
            SetNewFloorPlan(floorPlan);
            return;
        }

        //_gridPreview = _floorPlanGenerator._currentFloorPlan.CellsGrid.GridToString();
        
        for(int i = 0; i < floorPlan.CellsGrid.Cells.Length; i++)
        {
            Zone cellZone = floorPlan.CellsGrid.Cells[i].Zone;
            if(cellZone != null)
            {
                _cellsGraphicsInstances[i].SetColor(_zoneColors[cellZone.ZoneId], floorPlan.CellsGrid.Cells[i]);
            }
            else
            {
                _cellsGraphicsInstances[i].SetColor(Color.black, null);
            }
        }
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="floorPlan"></param>
    void SetNewFloorPlan(FloorPlanManager floorPlan)
    {
        _currentFloorPlanId = floorPlan.FloorPlanId;

        ResetDebugger();

        // Set the zone colors.
        int numZones = floorPlan.ZonesInstances.Count;
        float colorInterval = 1f/numZones;
        _zoneColors = new SerializedDictionary<string, Color>();
        float hueValue = 0;
        foreach(var zone in floorPlan.ZonesInstances)
        {
            _zoneColors.Add(zone.Value.ZoneId, Color.HSVToRGB(hueValue, 0.8f, 0.8f));
            hueValue += colorInterval;
        }

        InstantiateCellsGraphics(floorPlan);
    }


    /// <summary>
    /// 
    /// </summary>
    void ResetDebugger()
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
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="floorPlan"></param>
    void InstantiateCellsGraphics(FloorPlanManager floorPlan)
    {
        _cellsGraphicsInstances = new List<VisualCell>();

        // Instancia as celulas.
        foreach (var cell in floorPlan.CellsGrid.Cells)
        {
            VisualCell visualCell = Instantiate(_cellGraphicsPrefab,
                                                new Vector3(cell.GridPosition.x, 0, -cell.GridPosition.y),
                                                Quaternion.identity,
                                                transform).GetComponent<VisualCell>();
            
            visualCell.Init(cell);
            
            if(cell.Zone != null)
            {
                visualCell.SetColor(_zoneColors[cell.Zone.ZoneId], cell);
            }
            else
            {
                visualCell.SetColor(Color.black, null);
            }
            _cellsGraphicsInstances.Add(visualCell);
        }
    }


    void OnDrawGizmos()
    {
        //if(!_initialized) { return; }
        //Handles.Label(Vector3.zero, _gridPreview);
    }
}
}
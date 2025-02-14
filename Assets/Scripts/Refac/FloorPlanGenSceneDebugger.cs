using System;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class FloorPlanGenSceneDebugger : MonoBehaviour
{
    private FloorPlanGenerator _floorPlanGenerator;
    private FloorPlanManager _floorPlanManager; // Keep a direct ref to the manager to make easier to acess data.
    [SerializeField] private bool _initialized = false;

    private string _gridPreview;

    public void Init(FloorPlanGenerator floorPlanGenerator)
    {
        Debug.Log("Initializing floor plan generator scene debugger.");

        _floorPlanGenerator = floorPlanGenerator;

        // Registo to grid update events.
        _floorPlanGenerator.CurrentMethod.OnCellsGridChanged += OnCellsGridChanged;
        _floorPlanGenerator.CurrentMethod.OnCellChanged += OnCellChanged;
        
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


    void OnCellsGridChanged(CellsGrid cellsGrid)
    {
        if(!_initialized) { return; }
        
        Debug.Log("Cells grid changed.");
    }


    void OnCellChanged(Cell cell)
    {
        if(!_initialized) { return; }

        Debug.Log($"Cell changed.{cell._gridPosition}, {DateTime.Now}");

        _gridPreview = _floorPlanGenerator._floorPlanManager.CellsGrid.GridToString();
    }


    void OnDrawGizmos()
    {
        if(!_initialized) { return; }

        Handles.Label(Vector3.zero, _gridPreview);
    }

    void OnDisable()
    {
        if(!_initialized) { return; }

        _floorPlanGenerator.CurrentMethod.OnCellsGridChanged -= OnCellsGridChanged;
        _floorPlanGenerator.CurrentMethod.OnCellChanged -= OnCellChanged;
    }
}

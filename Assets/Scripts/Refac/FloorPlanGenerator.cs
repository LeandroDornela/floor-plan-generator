using UnityEngine;
using AYellowpaper.SerializedCollections;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;
using Unity.VisualScripting;

public enum FPGenerationMethodType
{
    LinearFill,
    FloodFill,
    FloodFillMult
}


[System.Serializable]
public class FloorPlanGenerator
{
    public FloorPlanManager _floorPlanManager;

    /*
    [SerializeField] private MethodLinearFill _methodLinearFill;
    [SerializeField] private MethodFloodFill _methodFloodFill;
    [SerializeField] private MethodFloodFillMult _methodFloodFillMult;
    */
    [SerializeField] private FPGenerationMethodType _generationMethodType;
    private FPGenerationMethod _currentMethod;
    public FPGenerationMethod CurrentMethod => _currentMethod;

    [Header("Debug")]
    [SerializeField] private FloorPlanGenSceneDebugger _sceneDebugger;
    [SerializeField] private bool _enableDebug = false;

    private bool _initialized = false;

    private bool Init(FloorPlanConfig floorPlanConfig)
    {
        Debug.Log("Initializing floor plan generator.");

        // ====== Floor plan manager setup ======
        _floorPlanManager = new FloorPlanManager();
        _floorPlanManager.Init(floorPlanConfig); // TODO: check init success


        // ====== Generation algorthm setup ======
        switch(_generationMethodType)
        {
            case FPGenerationMethodType.LinearFill:
                _currentMethod = new MethodLinearFill();
                break;
            case FPGenerationMethodType.FloodFill:
                _currentMethod = new MethodFloodFill();
                break;
            case FPGenerationMethodType.FloodFillMult:
                _currentMethod = new MethodFloodFillMult();
                break;
        }
        _currentMethod.Init(_floorPlanManager);  // TODO: check init success


        // ====== Visual debugger setup ======
        if(_sceneDebugger != null)
        {
            if (_enableDebug == true) _sceneDebugger.Init(this);
            else Object.Destroy(_sceneDebugger);
        }

        _initialized = true;
        Debug.Log("Ready to generate floor plan.");
        return _initialized;
    }

    public async UniTaskVoid GenerateFloorPlan(FloorPlanConfig floorPlanConfig)
    {
        Init(floorPlanConfig);
        
        _floorPlanManager.CellsGrid.PrintGrid();
        await _currentMethod.Run();
        _floorPlanManager.CellsGrid.PrintGrid();
    }
}

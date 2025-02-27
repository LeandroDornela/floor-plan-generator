using UnityEngine;
using Cysharp.Threading.Tasks;

public enum FPGenerationMethodType
{
    LinearFill,
    FloodFill,
    FloodFillWeighted,
    Growth
}


[System.Serializable]
public class FloorPlanGenerator
{
    public FloorPlanManager _floorPlanManager;

    [SerializeField] private FPGenerationMethodType _generationMethodType;
    private FPGenerationMethod _currentMethod;
    public FPGenerationMethod CurrentMethod => _currentMethod;

    [Header("Debug")]
    [SerializeField] private FloorPlanGenSceneDebugger _sceneDebugger;
    [SerializeField] private bool _enableDebug = false;

    private bool _initialized = false;
    private bool _running = false;

    private bool Init(FloorPlanConfig floorPlanConfig)
    {
        Debug.Log("Initializing floor plan generator.");

        // ====== Floor plan manager setup ======
        _floorPlanManager = new FloorPlanManager();
        _floorPlanManager.Init(floorPlanConfig); // TODO check init success
                                                 // <!--
                                                 // order:0
                                                 // -->


        // ====== Generation algorthm setup ======
        switch(_generationMethodType)
        {
            case FPGenerationMethodType.LinearFill:
                _currentMethod = new MethodLinearFill();
                break;
            case FPGenerationMethodType.FloodFill:
                _currentMethod = new MethodFloodFill();
                break;
            case FPGenerationMethodType.FloodFillWeighted:
                _currentMethod = new MethodFloodFillWeighted();
                break;
            case FPGenerationMethodType.Growth:
                _currentMethod = new MethodGrowth();
                break;
        }
        _currentMethod.Init(_floorPlanManager);  // TODO check init success
                                                 // <!--
                                                 // order:-10
                                                 // -->


        // ====== Visual debugger setup ======
        if(_sceneDebugger != null)
        {
            if (_enableDebug == true) _sceneDebugger.Init(this, floorPlanConfig);
            else Object.Destroy(_sceneDebugger);
        }

        _initialized = true;
        Debug.Log("Ready to generate floor plan.");
        return _initialized;
    }

    public async UniTask<bool> GenerateFloorPlan(FloorPlanConfig floorPlanConfig)
    {
        if(_running)
        {
            Debug.LogWarning("Generation in process, please wait.");
            return false;
        }

        Init(floorPlanConfig);

        _running = true;
        
        _floorPlanManager.CellsGrid.PrintGrid();
            await _currentMethod.Run();
        _floorPlanManager.CellsGrid.PrintGrid();

        _running = false;
        return true;
    }

    public void OnDrawGizmos()
    {
        _currentMethod?.OnDrawGizmos();
    }
}

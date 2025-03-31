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
    [SerializeField, NaughtyAttributes.Expandable] private FPGenerationMethod _generationMethod;
    public FPGenerationMethod CurrentMethod => _generationMethod;

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


        // ====== Visual debugger setup ======
        if(_sceneDebugger != null)
        {
            if (_enableDebug == true) _sceneDebugger.Init(this, floorPlanConfig);
            else Object.Destroy(_sceneDebugger);
        }


        _generationMethod.Init();


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
        
        //_floorPlanManager.CellsGrid.PrintGrid();
        //await _currentMethod.Run();
        await _generationMethod.Run(_floorPlanManager, _sceneDebugger);
        //_floorPlanManager.CellsGrid.PrintGrid();

        _running = false;

        return true;
    }

    public void OnDrawGizmos()
    {
        //_currentMethod?.OnDrawGizmos();
        _generationMethod?.OnDrawGizmos();
    }
}

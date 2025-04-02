using UnityEngine;
using Cysharp.Threading.Tasks;
using System;

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
    public bool _useSeed = false;
    public int _seed = 0;
    public FloorPlanManager _floorPlanManager;
    [SerializeField, NaughtyAttributes.Expandable] private FPGenerationMethod _generationMethod;
    public FPGenerationMethod CurrentMethod => _generationMethod;

    [Header("Debug")]
    [SerializeField] private FloorPlanGenSceneDebugger _sceneDebugger;
    [SerializeField] private bool _enableDebug = false;

    private bool _initialized = false;
    private bool _running = false;

    private bool Init(FloorPlanData floorPlanConfig)
    {
        Debug.Log("Initializing floor plan generator.");

        // ====== Floor plan manager setup ======
        _floorPlanManager = new FloorPlanManager();
        _floorPlanManager.Init(floorPlanConfig); // TODO check init success


        // ====== Visual debugger setup ======
        if(_sceneDebugger != null)
        {
            if (_enableDebug == true) _sceneDebugger.Init(this, floorPlanConfig);
            else UnityEngine.Object.Destroy(_sceneDebugger);
        }

        _initialized = true;

        Debug.Log("Ready to generate floor plan.");

        return _initialized;
    }

    public async UniTask<bool> GenerateFloorPlan(FloorPlanData floorPlanConfig)
    {
        if(_running)
        {
            Debug.LogWarning("Generation in process, please wait.");
            return false;
        }

        Init(floorPlanConfig);

        _running = true;
        if(!_useSeed) _seed = (int)DateTime.Now.Ticks;
        await _generationMethod.Run(_floorPlanManager, _sceneDebugger, _seed);
        _running = false;

        return true;
    }

    public void OnDrawGizmos()
    {
        _generationMethod?.OnDrawGizmos();
    }
}

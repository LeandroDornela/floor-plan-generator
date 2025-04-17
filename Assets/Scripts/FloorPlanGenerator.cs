using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;


namespace BuildingGenerator
{
[System.Serializable]
public class FloorPlanGenerator
{
    public bool _useSeed = false;
    public int _seed = 0;
    public FloorPlanManager _currentFloorPlan;
    [SerializeField, NaughtyAttributes.Expandable] private FPGenerationMethod _generationMethod;
    public FPGenerationMethod CurrentMethod => _generationMethod;

    [Header("Debug")]
    [SerializeField] private FloorPlanGenSceneDebugger _sceneDebugger;
    [SerializeField] private bool _enableDebug = false;

    private bool _initialized = false;
    private bool _running = false;

    private List<FloorPlanManager> _generatedFloorPlans;

    private bool PrepareNewGeneration(FloorPlanData floorPlanConfig)
    {
        Debug.Log("Initializing floor plan generator.");

        // ====== Floor plan manager setup ======
        _currentFloorPlan = new FloorPlanManager();
        _currentFloorPlan.Init(floorPlanConfig); // TODO check init success


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

    public async UniTask<bool> DEBUG_GenerateFloorPlan(FloorPlanData floorPlanConfig)
    {
        if(_running)
        {
            Debug.LogWarning("Generation in process, please wait.");
            return false;
        }

        PrepareNewGeneration(floorPlanConfig);

        _running = true;
        if(!_useSeed) _seed = (int)DateTime.Now.Ticks;
        await _generationMethod.DEBUG_RunStepByStep(_currentFloorPlan, _sceneDebugger, _seed);
        _running = false;

        return true;
    }

    public async UniTask<bool> GenerateFloorPlan(FloorPlanData floorPlanConfig)
    {
        if(_running)
        {
            Debug.LogWarning("Generation in process, please wait.");
            return false;
        }

        _running = true;
            PrepareNewGeneration(floorPlanConfig);
            if(!_useSeed) _seed = (int)DateTime.Now.Ticks;
            _generationMethod.Run(_currentFloorPlan, _sceneDebugger, _seed);
            _sceneDebugger.OnCellsGridChanged(_currentFloorPlan.CellsGrid);
        _running = false;

        await UniTask.NextFrame();
        return true;
    }

    public async UniTask<bool> GenerateFloorPlans(FloorPlanData floorPlanConfig, int amount)
    {
        if(_running)
        {
            Debug.LogWarning("Generation in process, please wait.");
            return false;
        }

        _running = true;
        if(!_useSeed) _seed = (int)DateTime.Now.Ticks;
        for(int i = 0; i < amount; i++)
        {
            PrepareNewGeneration(floorPlanConfig);
             _generationMethod.Run(_currentFloorPlan, _sceneDebugger, _seed);
        }
        _running = false;

        await UniTask.NextFrame();
        return true;
    }

    public void OnDrawGizmos()
    {
        _generationMethod?.OnDrawGizmos();
    }
}
}

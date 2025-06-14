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

    // Maximum number that it will request the generation method to generate a valid floor plan.
    public int _maxGenerationTries = 10;
    // Number of valid floor plans to generate. At the end choose the best option from the samples.
    // Worst case generation = (_maxGenerationTries * _samples)
    public int _samples = 10;

    public FloorPlanManager _currentFloorPlan;

    [SerializeField, NaughtyAttributes.Expandable] private FPGenerationMethod _generationMethod;

    [Header("Debug")]
    [SerializeField] private FloorPlanGenSceneDebugger _sceneDebugger;
    [SerializeField] private bool _enableDebug = false;

    private bool _running = false;


    public async UniTask<List<FloorPlanManager>> GenerateFloorPlans(FloorPlanData floorPlanConfig, int amount = 1)
    {
        if(_running)
        {
            Debug.LogWarning("Generation in process, please wait.");
            return default;
        }

        if(amount <= 0)
        {
            Debug.LogWarning("The amount of floor plans to generate must be at least 1.");
            return default;
        }

        List<FloorPlanManager> _selectedFloorPlans = new List<FloorPlanManager>();

        // TODO: temp
            //_currentFloorPlan = new FloorPlanManager(floorPlanConfig);
            //if(_sceneDebugger != null && _enableDebug == true) _sceneDebugger.Init(this, floorPlanConfig);
        // =====
        
        _running = true;
        var watch = System.Diagnostics.Stopwatch.StartNew();
        // Random setup.
        if(!_useSeed) _seed = (int)DateTime.Now.Ticks; // Keep like this to store current seed.
        Utils.Random.SetSeed(_seed);
        
        //await UniTask.SwitchToThreadPool();

        // Loop for generating the final desired number of floor plans
        for(int amountCount = 0; amountCount < amount; amountCount++)
        {
            List<FloorPlanManager> _generatedRawFloorPlans = new List<FloorPlanManager>();

            // >>>>> N(_samples) FLOOR PLANS GENERATION - START
            // Loop to generate the defined number of samples to select the best to enter the select array
            for(int samplesCount = 0; samplesCount < _samples; samplesCount++)
            {
                // >>>>> 1 FLOOR PLAN GENERATION - START
                bool isValid = false;

                // Try to generate a valid floor plan.
                // Loop to try generate a valid floor plan to enter the candidates floor plans list.
                for(int i = 0; i < _maxGenerationTries; i++)
                {
                    _currentFloorPlan = new FloorPlanManager(floorPlanConfig);
                    isValid = await _generationMethod.Run(_currentFloorPlan, _sceneDebugger);

                    if(isValid)
                    {
                        break;
                    }
                }

                if(!isValid)
                {
                    // TODO: _generationMethod.Run cold return a struct as result with a string containing the reason of the failure.
                    Debug.LogError($"Unable to generate the floor plan: {floorPlanConfig}. Try changing the settings.");
                    return default;
                }

                _generatedRawFloorPlans.Add(_currentFloorPlan);

                FloorPlanGenSceneDebugger.Instance.OnFloorPlanUpdated(_currentFloorPlan);
                await UniTask.NextFrame();

                // <<<<< 1 FLOOR PLAN GENERATION - END
            }
            // <<<<< N(_samples) FLOOR PLANS GENERATION - END

            if(_generatedRawFloorPlans.Count == 0)
            {
                Debug.LogError("No valid floor plans generated.");
                return default;
            }

            // TODO: heuristic to choose the best floor plan.
            _selectedFloorPlans.Add(_generatedRawFloorPlans[0]);
        }

        //await UniTask.SwitchToMainThread();
        // Random reset, optional.
        Utils.Random.ClearSeed();
        _running = false;
        var elapsedMs = watch.ElapsedMilliseconds;
        UnityEngine.Debug.Log(elapsedMs);
        
        return _selectedFloorPlans;
    }


    public void OnDrawGizmos()
    {
        _generationMethod?.OnDrawGizmos();
    }
}
}

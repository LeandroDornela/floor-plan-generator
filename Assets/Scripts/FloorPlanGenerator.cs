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

    /// <summary>
    /// Maximum number that it will request the generation method to generate a valid floor plan.
    /// </summary>
    public int _maxGenerationTries = 10;
    
    /// <summary>
    /// Number of valid floor plans to generate. At the end choose the best option from the samples.
    /// Worst case generation = (_maxGenerationTries * _samples)
    /// </summary>
    public int _samples = 10;

    public FloorPlanManager _currentFloorPlan;

    public MethodGrowthSettings _generationMethodSettings;

    private MethodGrowth _generationMethod;

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
        for (int amountCount = 0; amountCount < amount; amountCount++)
        {
            List<FloorPlanManager> _generatedRawFloorPlans = new List<FloorPlanManager>();
            // >>>>> N(_samples) FLOOR PLANS GENERATION - START
            // Loop to generate the defined number of samples to select the best to enter the select array
            for (int samplesCount = 0; samplesCount < _samples; samplesCount++)
            {
                // >>>>> 1 FLOOR PLAN GENERATION - START
                bool isValid = false;
                    // Try to generate a valid floor plan.
                    // Loop to try generate a valid floor plan to enter the candidates floor plans list.
                    int genTry;
                    for (genTry = 0; genTry < _maxGenerationTries; genTry++)
                    {
                        _currentFloorPlan = new FloorPlanManager(floorPlanConfig);
                        //Debug.Log("<color=yellow>Plan Gen start...</color>");
                        _generationMethod = new MethodGrowth(); // Create a new instance of the generation method to reset the runtime values from previous run.
                        isValid = await _generationMethod.Run(_generationMethodSettings, _currentFloorPlan, _sceneDebugger);
                        if (isValid)
                        {
                            Debug.Log($"Total generation tries until a valid result: {genTry+1}/{_maxGenerationTries}");
                            break;
                        }
                    }
                if (!isValid)
                    {
                        // TODO: _generationMethod.Run cold return a struct as result with a string containing the reason of the failure.
                        Debug.LogError($"Unable to generate the floor plan: {floorPlanConfig}. Try changing the settings.");
                        _running = false;
                        return default;
                    }
                _generatedRawFloorPlans.Add(_currentFloorPlan);
                FloorPlanGenSceneDebugger.Instance.OnFloorPlanUpdated(_currentFloorPlan);
                await UniTask.NextFrame();
                // <<<<< 1 FLOOR PLAN GENERATION - END
            }
            // <<<<< N(_samples) FLOOR PLANS GENERATION - END
            if (_generatedRawFloorPlans.Count == 0)
            {
                Debug.LogError("No valid floor plans generated.");
                _running = false;
                return default;
            }

            // Find the best results.
            float biggestScore = 0;
            FloorPlanManager selectedFloorPlan = _generatedRawFloorPlans[0];
            foreach (FloorPlanManager floorPlan in _generatedRawFloorPlans)
            {
                float fpScore = 0;
                float regularZonesCount = floorPlan.RegularZonesCount();
                float totalDesiredAreasDistance = Mathf.Clamp(floorPlan.TotalDistanceFromDesiredAreas(), 1, float.MaxValue); // Considering very small distances as irrelevant.
                    fpScore = regularZonesCount / totalDesiredAreasDistance;
                if (fpScore > biggestScore)
                {
                    biggestScore = fpScore;
                    selectedFloorPlan = floorPlan;
                }
            }
            _selectedFloorPlans.Add(selectedFloorPlan);
        }

        //await UniTask.SwitchToMainThread();
        // Random reset, optional.
        Utils.Random.ClearSeed();
        _running = false;
        var elapsedMs = watch.ElapsedMilliseconds;
        UnityEngine.Debug.Log(elapsedMs);

        FloorPlanGenSceneDebugger.Instance.OnFloorPlanUpdated(_selectedFloorPlans[0]);
        
        return _selectedFloorPlans;
    }
}
}

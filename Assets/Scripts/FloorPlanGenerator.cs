using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;


namespace BuildingGenerator
{
    [System.Serializable]
    public class FloorPlanGenerator
    {
        /*
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


        public MethodGrowthSettings _generationMethodSettings;


        [Header("Debug")]
        [SerializeField] private FloorPlanGenSceneDebugger _sceneDebugger;
        [SerializeField] private bool _screenshotPlan = true;
        [SerializeField] private bool _enableDebug = false;
        */

        private bool _running = false;
        private MethodGrowth _generationMethod;
        private FloorPlanManager _currentFloorPlan;

        private float _generationProgress = -1;

        public float GenerationProgress => _generationProgress;


        public async UniTask<List<FloorPlanManager>> GenerateFloorPlans(BuildingGeneratorSettings buildingGeneratorSettings, MethodGrowthSettings methodGrowthSettings, FloorPlanGenSceneDebugger sceneDebugger, FloorPlanData floorPlanConfig, int amount = 1)
        {
            if (_running)
            {
                Debug.LogWarning("Generation in process, please wait.");
                return default;
            }

            if (amount <= 0)
            {
                Debug.LogWarning("The amount of floor plans to generate must be at least 1.");
                return default;
            }

            bool _useSeed = buildingGeneratorSettings.UseSeed;
            int _seed = buildingGeneratorSettings.Seed;
            int _maxGenerationTries = buildingGeneratorSettings.MaxGenerationTries;
            int _samples = buildingGeneratorSettings.Samples;
            bool _screenshotPlan = buildingGeneratorSettings.ScreenshotPlan;
            List<FloorPlanManager> _selectedFloorPlans = new List<FloorPlanManager>();
            GenerationStats genStats = new GenerationStats(floorPlanConfig);

            _running = true;
            // Random setup.
            if (!_useSeed) _seed = (int)DateTime.Now.Ticks; // Keep like this to store current seed.
            Utils.Random.SetSeed(_seed);
            genStats._seed = _seed;

            Debug.Log($"Seed: {_seed}");

            await UniTask.SwitchToThreadPool();

            // Loop for generating the final desired number of floor plans
            for (int amountCount = 0; amountCount < amount; amountCount++)
            {
                List<FloorPlanManager> _generatedRawFloorPlans = new List<FloorPlanManager>();

                // Loop to generate the defined number of samples to select the best to enter the select array
                for (int samplesCount = 0; samplesCount < _samples; samplesCount++)
                {
                    _generationProgress = (float)samplesCount / _samples;

                    bool isValid = false;
                    // Try to generate a valid floor plan.
                    // Loop to try generate a valid floor plan to enter the candidates floor plans list.
                    int sampleFails = 0; // Count to fails to generate ONE sample.
                    int genTry;
                    for (genTry = 0; genTry < _maxGenerationTries; genTry++)
                    {
                        genStats._totalGenerationTries++;
                        _currentFloorPlan = new FloorPlanManager(floorPlanConfig);
                        Utils.Debug.DevLog("<color=yellow>Plan Gen start...</color>");
                        _generationMethod = new MethodGrowth(); // Create a new instance of the generation method to reset the runtime values from previous run.
                        isValid = await _generationMethod.Run(methodGrowthSettings, _currentFloorPlan, sceneDebugger);
                        if (isValid)
                        {
                            //Debug.LogError($"<color=green>Total generation tries until a valid result: {genTry + 1}/{_maxGenerationTries}</color>");
                            break;
                        }
                        else
                        {
                            sampleFails++;
                            genStats._totalFails++;
                        }
                    }

                    // Após _maxGenerationTries se não pode gerar uma amostra, pula para proxima tentativa sem adicionar a planta as amostras.
                    if (!isValid)
                    {
                        //Utils.Screenshot($"sample_{samplesCount}_fail");

                        continue;
                        /*
                        // Dessa forma se tiver apenas 1 tentativa e falhar o algoritimo pode acusar falha e n tentar mais. Não fazer assim.
                        // TODO: _generationMethod.Run cold return a struct as result with a string containing the reason of the failure.
                        Debug.LogError($"Unable to generate the floor plan: {floorPlanConfig}. Try changing the settings.");
                        _running = false;
                        return default;
                        */
                    }

                    _generatedRawFloorPlans.Add(_currentFloorPlan);
                    //sceneDebugger.OnFloorPlanUpdated(_currentFloorPlan);
                    await UniTask.NextFrame();
                    //Utils.Screenshot($"s{samplesCount}_sf{sampleFails}");
                    //await UniTask.WaitForSeconds(1);
                }


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
                    float regularZonesIndex = floorPlan.RectZonesIndex();
                    float aspectRatioIndex = floorPlan.DesiredAspectIndex();
                    float areaIndex = floorPlan.DesiredAreaIndex();
                    Utils.Debug.DevLog($"Regular:{regularZonesIndex}, Aspect:{aspectRatioIndex}, Area: {areaIndex}");

                    fpScore = regularZonesIndex + aspectRatioIndex + areaIndex;

                    if (fpScore > biggestScore)
                    {
                        biggestScore = fpScore;
                        selectedFloorPlan = floorPlan;
                    }

                    //float totalDesiredAreasDistance = Mathf.Clamp(floorPlan.TotalDistanceFromDesiredAreas(), 1, float.MaxValue); // Considering very small distances as irrelevant.
                    //fpScore = regularZonesIndex / totalDesiredAreasDistance;

                    /*
                    float threshold = 0.7f;
                    float areaT = 0.9f;
                    if (aspectRatioIndex < threshold || areaIndex < areaT)
                    {

                    }
                    else
                    {
                        fpScore = regularZonesIndex + aspectRatioIndex + areaIndex;

                        if (fpScore > biggestScore)
                        {
                            biggestScore = fpScore;
                            selectedFloorPlan = floorPlan;
                        }
                    }
                    */
                }
                _selectedFloorPlans.Add(selectedFloorPlan);
            }

            await UniTask.SwitchToMainThread();

            // Random reset, optional.
            Utils.Random.ClearSeed();
            _running = false;

            Utils.Debug.DevLog($"Selected - Regular:{_selectedFloorPlans[0].RectZonesIndex()}, Aspect:{_selectedFloorPlans[0].DesiredAspectIndex()}, Area: {_selectedFloorPlans[0].DesiredAreaIndex()}");
            sceneDebugger.OnFloorPlanUpdated(_selectedFloorPlans[0]);
            await UniTask.NextFrame();
            Utils.Screenshot($"selected_tf{genStats._totalFails}");

            if (buildingGeneratorSettings.SaveGenStatsJson)
            {
                genStats.SaveStatsAsJsonFile();
            }

            _generationProgress = 1;

            return _selectedFloorPlans;
        }
    }
}

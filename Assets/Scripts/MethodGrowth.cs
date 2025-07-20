//#define TEST

using UnityEngine;
using System.Threading;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace BuildingGenerator
{
    public partial class MethodGrowth
    {
        private CancellationTokenSource _cts;
        private Zone _currentZone;
        private List<Zone> _zonesToSubdivide; // TODO: QUEUE
        private List<Zone> _zonesToGrow;

        // Quando uma zona n pode mais crescer na iteração atual é armazenada aqui.
        // Depois retorna para crescer usando outra logical, 'L' ou 'free'
        private List<Zone> _grownZones;

        private WeightedArray _cellsWeights;
        private WeightedArray _zonesWeights;

        private MethodGrowthSettings _settings;

        private Guid _outsideZoneId = Guid.NewGuid();
        private bool _checkFullSpace = true;
        
        public Event<FloorPlanManager> FloorPlanUpdatedEvent = new Event<FloorPlanManager>();


        /// <summary>
        /// ASYNC METHOD
        /// </summary>
        /// <returns></returns>
        public async UniTask<bool> Run(MethodGrowthSettings methodGrowthSettings, FloorPlanManager floorPlanManager)
        {
#if TEST
        Utils.Debug.DevWarning("Running in TEST mode.");
#endif

            /*
            if(!EditorApplication.isPlaying)
            {
                Debug.LogError("Don't use it outside play mode.");
                return false;
            }
            */

            // Start the timer.
            Utils.Stopwatch timer = new Utils.Stopwatch();

            _cts = new CancellationTokenSource();
            //EditorApplication.playModeStateChanged += PlayModeStateChanged;

            _settings = methodGrowthSettings;

            _zonesToSubdivide = new List<Zone>();
            _zonesToGrow = new List<Zone>();
            _grownZones = new List<Zone>();

            CellsGrid cellsGrid = floorPlanManager.CellsGrid;

            // Add root zone to subdivision.
            _zonesToSubdivide.Add(floorPlanManager.RootZone);


            while (_zonesToSubdivide.Count > 0) // A CADA EXECUÇÃO FAZ A DIVISÃO DE UMA ZONA.
            {
                // Get the child zones from the next zone to subdivide.
                _zonesToGrow = GetNextZonesToGrowList(floorPlanManager);
                UpdateZonesWeights(_zonesToGrow);

                if (_settings.StopAtInitialPlot) break;

                // LOOP CRESCIMENTO RECT
                while (_zonesToGrow.Count > 0)
                {
                    _currentZone = GetNextZone(_zonesToGrow);

                    if (!GrowZoneRect(_currentZone, cellsGrid))
                    {
                        _zonesToGrow.Remove(_currentZone);
                        UpdateZonesWeights(_zonesToGrow);
                        _grownZones.Add(_currentZone);
                    }

                    if (!_settings.SkipToFinalResult)
                    {
                        //sceneDebugger.OnFloorPlanUpdated(floorPlanManager);
                        FloorPlanUpdatedEvent.Invoke(floorPlanManager);
                        await UniTask.WaitForSeconds(_settings.Delay + 0.1f, cancellationToken: _cts.Token);
                    }
                }

                // Prepare for next step.
                _zonesToGrow = new List<Zone>(_grownZones);
                UpdateZonesWeights(_zonesToGrow);
                _grownZones.Clear();

                // LOOP CRESCIMENTO L
                while (_zonesToGrow.Count > 0)
                {
                    _currentZone = GetNextZone(_zonesToGrow);

                    if (!GrowZoneLShape(_currentZone, cellsGrid))
                    {
                        _zonesToGrow.Remove(_currentZone);
                        UpdateZonesWeights(_zonesToGrow);
                        _grownZones.Add(_currentZone);
                    }

                    if (!_settings.SkipToFinalResult)
                    {
                        //sceneDebugger.OnFloorPlanUpdated(floorPlanManager);
                        FloorPlanUpdatedEvent.Invoke(floorPlanManager);
                        await UniTask.WaitForSeconds(_settings.Delay, cancellationToken: _cts.Token);
                    }
                }

                // Prepare the next set of zones to grow.
                // "Bake" the zones that finished to grow and add the ones with children(the ones tha will be subdivided)
                // to the list of zones to be subdivided, the one zone will be pick from this list and their children will
                // be the 'zones to grow' inside this zone to subdivide.
                foreach (Zone zone in _grownZones)
                {
                    zone.Bake();

                    if (zone.HasChildrenZones)
                    {
                        _zonesToSubdivide.Add(zone);
                    }
                }

                // When finish a zone subdivision re bake the dirty zones.
                // TODO: don't need to check if all are dirty, at this point only the parent of _grownZones should be dirty.
                ReBakeDirtyZones(floorPlanManager);

                _grownZones.Clear();
            }


            // For debug.
            foreach (var zone in floorPlanManager.ZonesInstances)
            {
                zone.Value.CheckZoneCellsConsistency();
            }


            // Can be done at the of the process os at the end of a hierarchy level. The fact of making at the end and the cells
            // don't having a prior zone assigned is not a problem since the cell have only one parent zone when we add to the cell
            // a leaf zone it will automatically have the parents of the leaf assigned to it.
            if (!AssignMissingCells(floorPlanManager))
            {
                Utils.Debug.DevError("Failed to assign missing cells.");
                return false;
            }


            // TODO: separate wall/door placement from connectivity check.
            if (!PlaceWallsAndCheckConnectivity(floorPlanManager))
            {
                Utils.Debug.DevError("Failed to place walls.");
                return false;
            }

            GenerationStats.Instance.AddTimeEnter("fullMethod", timer.Stop());

            //EditorApplication.playModeStateChanged -= PlayModeStateChanged;
            _cts.Dispose();

            return true;
        }


        #region ========== AUXILIARY METHODS ==========
        void ReBakeDirtyZones(FloorPlanManager floorPlanManager)
        {
            foreach (Zone zone in floorPlanManager.ZonesInstances.Values)
            {
                if (zone.IsDirty)
                {
                    Utils.Debug.DevLog($"{zone.ZoneId} is dirty. Re-baking...");
                    zone.Unbake();
                    zone.Bake();
                }
            }
        }

        /*
        /// <summary>
        /// 
        /// </summary>
        /// <param name="state"></param>
        void PlayModeStateChanged(PlayModeStateChange state)
        {
            if(state == PlayModeStateChange.ExitingPlayMode)
            {
                _cts.Cancel();
            }
        }
        */
        #endregion
    }
}
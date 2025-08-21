using System;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using com.cyborgAssets.inspectorButtonPro;
using UnityEditor;
using UnityEngine;

namespace BuildingGenerator
{
    public class FloorPlanGenSceneDebugger : IBuildingInterpreter
    {
        public Transform wallsHolder;
        public Transform cellsHolder;

        [SerializeField] private BuildingAssetsPack _buildingAssetsPack;
        private SerializedDictionary<string, Color> _zoneColors;
        [SerializeReference] private List<VisualCell> _cellsGraphicsInstances;
        [SerializeReference] private List<GameObject> _wallInstances;
        [SerializeReference] private GeneratedBuildingData _generatedBuildingData;
        [Obsolete] private FloorPlanManager _currentFloorPlan;
        

        [Header("Debug")]
        public bool _debugBorders;
        public bool _debugWallSharers;
        public bool _debugWallLines;


        private Event<FloorPlanManager> _floorPlanUpdatedEvent;
        private Event<GeneratedBuildingData> _generationFinishedEvent;

        private int yModifier = 1;

        public FloorPlanManager CurrentFloorPlan => _currentFloorPlan;


        public override void Init(BuildingGenerator buildingGenerator, BuildingAssetsPack buildingAssetsPack)
        {
            _buildingAssetsPack = buildingAssetsPack;

            buildingGenerator.FloorPlanUpdatedEvent.Register(OnFloorPlanUpdated);
            _floorPlanUpdatedEvent = buildingGenerator.FloorPlanUpdatedEvent;
            buildingGenerator.GenerationFinishedEvent.Register(OnGenerationFinished);
            _generationFinishedEvent = buildingGenerator.GenerationFinishedEvent;
        }


        [ProButton]
        public override void InterpretBuildingData(GeneratedBuildingData generatedBuildingData)
        {
            _generatedBuildingData = generatedBuildingData;

            OnFloorPlanUpdated(_generatedBuildingData.GeneratedFloorPlans[0]);
        }

        void OnGenerationFinished(GeneratedBuildingData generatedBuildingData)
        {
            _floorPlanUpdatedEvent.Unregister(OnFloorPlanUpdated);
            _generationFinishedEvent.Unregister(OnGenerationFinished);

            _generatedBuildingData = generatedBuildingData;

            _currentFloorPlan = generatedBuildingData.GeneratedFloorPlans[0];
        }

        void OnDisable()
        {
            _floorPlanUpdatedEvent?.Unregister(OnFloorPlanUpdated);
            _generationFinishedEvent?.Unregister(OnGenerationFinished);
        }


        [ProButton]
        public void UpdateAssets()
        {
            if (_buildingAssetsPack == null)
            {
                Debug.LogError("Assets pack undefined.");
                return;
            }

            if (_currentFloorPlan == null)
            {
                Debug.LogError("Current floor plan undefined.");
                return;
            }

            SetNewFloorPlan(_currentFloorPlan, _buildingAssetsPack);
            UpdateWalls(_buildingAssetsPack);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="floorPlan"></param>
        public void OnFloorPlanUpdated(FloorPlanManager floorPlan)
        {
            if (_currentFloorPlan != floorPlan)
            {
                SetNewFloorPlan(floorPlan, _buildingAssetsPack);
                UpdateWalls(_buildingAssetsPack);
                return;
            }

            for (int i = 0; i < floorPlan.CellsGrid.Cells.Length; i++)
            {
                Zone cellZone = floorPlan.CellsGrid.Cells[i].Zone;
                if (cellZone != null)
                {
                    _cellsGraphicsInstances[i].SetColor(_zoneColors[cellZone.ZoneId], floorPlan.CellsGrid.Cells[i]);
                }
                else
                {
                    _cellsGraphicsInstances[i].SetColor(Color.black, null);
                    //DestroyImmediate(_cellsGraphicsInstances[i]);
                }
            }

            UpdateWalls(_buildingAssetsPack);
        }

        void UpdateWalls(BuildingAssetsPack buildingAssetsPack)
        {
            if (_wallInstances != null)
            {
                foreach (GameObject obj in _wallInstances)
                {
                    if(obj != null) DestroyImmediate(obj);
                }
                _wallInstances.Clear();
            }
            else
            {
                _wallInstances = new List<GameObject>();
            }

            if (_currentFloorPlan != null)
            {
                foreach (CellsTuple cellsTuple in _currentFloorPlan.WallCellsTuples)
                {
                    if (cellsTuple.CellA == null || cellsTuple.CellB == null)
                    {
                        continue;
                    }

                    Vector3 cellAPos = new Vector3(cellsTuple.CellA.GridPosition.x,
                                                   0,
                                                   yModifier*cellsTuple.CellA.GridPosition.y);
                    Vector3 cellBPos = new Vector3(cellsTuple.CellB.GridPosition.x,
                                                   0,
                                                   yModifier*cellsTuple.CellB.GridPosition.y);
                    Vector3 dif = cellAPos - cellBPos;
                    dif.Scale(new Vector3(0.5f, 0.5f, 0.5f));
                    Vector3 pos = cellBPos + dif + transform.position;
                    Quaternion rot = Quaternion.LookRotation(dif, Vector3.up);

                    if (cellsTuple.HasDoor)
                    {
                        if (buildingAssetsPack.doorPrefab != null)
                        {
                            GameObject door = Instantiate(buildingAssetsPack.doorPrefab, new Vector3(pos.x, 0, pos.z), rot, wallsHolder);
                            ScaleAnimation scaleAnimation = door.GetComponent<ScaleAnimation>();
                            if (scaleAnimation != null)
                            {
                                scaleAnimation.TriggerAnimation(pos.x);
                            }
                            _wallInstances.Add(door);
                        }
                    }
                    else
                    {
                        if (buildingAssetsPack.wallPrefab != null)
                        {
                            GameObject wall = Instantiate(buildingAssetsPack.wallPrefab, new Vector3(pos.x, 0, pos.z), rot, wallsHolder);
                            ScaleAnimation scaleAnimation = wall.GetComponent<ScaleAnimation>();
                            if (scaleAnimation != null)
                            {
                                scaleAnimation.TriggerAnimation(pos.x);
                            }
                            _wallInstances.Add(wall);
                        }
                    }
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="floorPlan"></param>
        void SetNewFloorPlan(FloorPlanManager floorPlan, BuildingAssetsPack buildingAssetsPack)
        {
            _currentFloorPlan = floorPlan;

            ResetDebugger();

            // Set the zone colors.
            int numZones = floorPlan.ZonesInstances.Count;
            float colorInterval = 1f / numZones;
            _zoneColors = new SerializedDictionary<string, Color>();
            float hueValue = 0;
            foreach (var zone in floorPlan.ZonesInstances)
            {
                _zoneColors.TryAdd(zone.Value.ZoneId, Color.HSVToRGB(hueValue, 0.8f, 0.8f));
                hueValue += colorInterval;
            }

            InstantiateCellsGraphics(floorPlan, buildingAssetsPack);
        }


        /// <summary>
        /// 
        /// </summary>
        void ResetDebugger()
        {
            // Checa se existen celulas instanciadas, as destroi e limpa a lista.
            if (_cellsGraphicsInstances != null)
            {
                foreach (var cell in _cellsGraphicsInstances)
                {
                    if(cell != null) DestroyImmediate(cell.gameObject);
                }

                _cellsGraphicsInstances.Clear();
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="floorPlan"></param>
        void InstantiateCellsGraphics(FloorPlanManager floorPlan, BuildingAssetsPack buildingAssetsPack)
        {
            _cellsGraphicsInstances = new List<VisualCell>();

            // Instancia as celulas.
            foreach (var cell in floorPlan.CellsGrid.Cells)
            {
                //if (cell.Zone == null) continue;

                GameObject prefab;

                if (cell.IsBorderCell && buildingAssetsPack.floorBorderPrefab != null)
                {
                    prefab = buildingAssetsPack.floorBorderPrefab;
                }
                else
                {
                    prefab = buildingAssetsPack.floorPrefab;
                }

                VisualCell visualCell = Instantiate(prefab,
                                                    new Vector3(transform.position.x + cell.GridPosition.x, transform.position.y, transform.position.z + yModifier * cell.GridPosition.y),
                                                    Quaternion.identity,
                                                    cellsHolder).GetComponent<VisualCell>();

                visualCell.Init(cell);

                if (cell.Zone != null)
                {
                    visualCell.SetColor(_zoneColors[cell.Zone.ZoneId], cell);
                }
                else
                {
                    //visualCell.SetColor(Color.black, null);
                    visualCell.HideGraphics();
                }
                _cellsGraphicsInstances.Add(visualCell);
            }
        }


        void OnDrawGizmos()
        {
            //if(!_initialized) { return; }
            //Handles.Label(Vector3.zero, _gridPreview);

            if (_currentFloorPlan != null)
            {
                if(_currentFloorPlan.WallCellsTuples != null)
                foreach (CellsTuple cellsTuple in _currentFloorPlan.WallCellsTuples)
                {
                    // Converte posição da grid para o ambiente 3D.
                    Vector3 cellAPos = new Vector3(cellsTuple.CellA.GridPosition.x,
                                                   0,
                                                   yModifier * cellsTuple.CellA.GridPosition.y);
                    Vector3 cellBPos = new Vector3(cellsTuple.CellB.GridPosition.x,
                                                   0,
                                                   yModifier * cellsTuple.CellB.GridPosition.y);

                    Vector3 dif = cellAPos - cellBPos;
                    dif.Scale(new Vector3(0.5f, 0.5f, 0.5f));
                    Vector3 pos = cellBPos + dif;
                    Quaternion rot = Quaternion.LookRotation(dif, Vector3.up);

                    if (_debugWallLines)
                    {
                        if (cellsTuple.IsOutsideBorder)
                            Gizmos.color = Color.red;
                        else
                            Gizmos.color = Color.black;
                        Gizmos.DrawLine(cellAPos, cellBPos);
                    }

                    if (_debugWallSharers)
                    {
                        Handles.color = Color.yellow;
                        Handles.Label(new Vector3(pos.x, 3, pos.z), $"{cellsTuple.CellA.Zone?.ZoneId} \n {cellsTuple.CellB.Zone?.ZoneId}");
                    }

                }


                // Debug borders
                if (_debugBorders)
                    foreach (Zone zone in _currentFloorPlan.ZonesInstances.Values)
                    {
                        if (zone.BorderCells != null)
                            foreach (Cell cell in zone.BorderCells)
                            {
                                Gizmos.color = Color.black;
                                Gizmos.DrawWireSphere(new Vector3(cell.GridPosition.x, 0, yModifier*cell.GridPosition.y), 0.1f);
                            }
                    }

            }
        }
    }
}
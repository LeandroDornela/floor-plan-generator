using System;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace BuildingGenerator
{
    public class FloorPlanGenSceneDebugger : MonoBehaviour
    {
        //[SerializeField] private GameObject _cellGraphicsPrefab;

        public Transform wallsHolder;
        public Transform cellsHolder;

        private string _currentFloorPlanId;
        private SerializedDictionary<string, Color> _zoneColors;
        private List<VisualCell> _cellsGraphicsInstances;

        private FloorPlanManager _currentFloorPlan;

        //private string _gridPreview;

        //private static FloorPlanGenSceneDebugger _instance;
        //public static FloorPlanGenSceneDebugger Instance => _instance;

        //public GameObject wallPrefab;
        //public GameObject doorPrefab;

        private List<GameObject> _wallInstances;

        private BuildingAssetsPack buildingAssetsPack;

        public bool _debugBorders;
        public bool _debugWallSharers;
        public bool _debugWallLines;

/*
        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(this.gameObject);
            }
            else
            {
                _instance = this;
            }
        }
*/

        public void Init(BuildingAssetsPack _buildingAssetsPack)
        {
/*
            if (_instance != null && _instance != this)
            {
                Destroy(this.gameObject);
            }
            else
            {
                _instance = this;
            }
*/

            buildingAssetsPack = _buildingAssetsPack;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="floorPlan"></param>
        public void OnFloorPlanUpdated(FloorPlanManager floorPlan)
        {
            if (_currentFloorPlan != floorPlan)
            {
                SetNewFloorPlan(floorPlan, buildingAssetsPack);
                UpdateWalls(buildingAssetsPack);
                return;
            }

            //_gridPreview = _floorPlanGenerator._currentFloorPlan.CellsGrid.GridToString();

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

            UpdateWalls(buildingAssetsPack);
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
                                                   -cellsTuple.CellA.GridPosition.y);
                    Vector3 cellBPos = new Vector3(cellsTuple.CellB.GridPosition.x,
                                                   0,
                                                   -cellsTuple.CellB.GridPosition.y);
                    Vector3 dif = cellAPos - cellBPos;
                    dif.Scale(new Vector3(0.5f, 0.5f, 0.5f));
                    Vector3 pos = cellBPos + dif;
                    Quaternion rot = Quaternion.LookRotation(dif, Vector3.up);

                    if (cellsTuple.HasDoor)
                    {
                        if(buildingAssetsPack.doorPrefab != null) _wallInstances.Add(Instantiate(buildingAssetsPack.doorPrefab, new Vector3(pos.x, 0, pos.z), rot, wallsHolder));
                    }
                    else
                    {
                        if(buildingAssetsPack.wallPrefab != null) _wallInstances.Add(Instantiate(buildingAssetsPack.wallPrefab, new Vector3(pos.x, 0, pos.z), rot, wallsHolder));
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
            _currentFloorPlanId = floorPlan.FloorPlanId;
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
                if (cell.Zone == null) continue;

                VisualCell visualCell = Instantiate(buildingAssetsPack.floorPrefab,
                                                    new Vector3(cell.GridPosition.x, 0, -cell.GridPosition.y),
                                                    Quaternion.identity,
                                                    cellsHolder).GetComponent<VisualCell>();

                visualCell.Init(cell);

                if (cell.Zone != null)
                {
                    visualCell.SetColor(_zoneColors[cell.Zone.ZoneId], cell);
                }
                else
                {
                    visualCell.SetColor(Color.black, null);
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
                foreach (CellsTuple cellsTuple in _currentFloorPlan.WallCellsTuples)
                {
                    // Converte posição da grid para o ambiente 3D.
                    Vector3 cellAPos = new Vector3(cellsTuple.CellA.GridPosition.x,
                                                   0,
                                                   -cellsTuple.CellA.GridPosition.y);
                    Vector3 cellBPos = new Vector3(cellsTuple.CellB.GridPosition.x,
                                                   0,
                                                   -cellsTuple.CellB.GridPosition.y);

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
                                Gizmos.DrawWireSphere(new Vector3(cell.GridPosition.x, 0, -cell.GridPosition.y), 0.1f);
                            }
                    }

            }
        }
    }
}
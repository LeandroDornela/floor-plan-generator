using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using System.Threading;

public class MethodFloodFillWeighted : FPGenerationMethod
{
    //private List<Cell> _cells;
    private bool _fastMode = false;

    private CancellationTokenSource _cts;

    private Dictionary<string, int> _weights;
    private Dictionary<string, List<Cell>> _cellsByZone;

    private int maxWeight = 12;
    private int targetWeight;

    public override bool Init(FloorPlanManager floorPlanManager)
    {
        Debug.Log("Initializing MethodFloodFill.");

        base.Init(floorPlanManager);

        _cts = new CancellationTokenSource();
        //EditorApplication.playModeStateChanged += PlayModeStateChanged;

        _weights = new Dictionary<string, int>
        {
            { "4", 12 },
            { "3", 6 },
            { "2", 2 },
            { "1", 6 },
            { "0", 2 }
        };
        //_cells = new List<Cell>();
        _cellsByZone = new Dictionary<string, List<Cell>>();
        for(int i = 0; i < _floorPlanManager.RootZones.Count; i++)
        {
            _cellsByZone.Add(_floorPlanManager.RootZones[i].ZoneId, new List<Cell>());
        }
        
        SetZoneToCell(2,5,0);
        SetZoneToCell(4,5,1);
        SetZoneToCell(6,5,2);
        SetZoneToCell(8,5,3);
        SetZoneToCell(10,5,4);
        
        /*
        SetZoneToCell(2,3,0);
        SetZoneToCell(11,5,1);
        SetZoneToCell(12,3,2);
        SetZoneToCell(0,12,3);
        SetZoneToCell(5,9,4);
        */
        targetWeight = 0;

        return _initialized;
    }

    public async override UniTask<bool> Run()
    {
        /*
        if(!EditorApplication.isPlaying)
        {
            Debug.LogError("Don't use it outside play mode.");
            return false;
        }
        */
/*
        for(int i = 0; i < _floorPlanManager.RootZones.Count; i++)
        {
            _weights.Add(_floorPlanManager.RootZones[i].ZoneId, (i+1)*2);
        }
*/
        if(_fastMode)
        {
            await UniTask.WaitUntil(SyncFlood);
        }
        else
        {
            AsyncTicker asyncTicker = AsyncTicker.Instantiate();
            asyncTicker.Begin(Flood, 0.01f);
                await UniTask.WaitUntil(() => ConditionMeet());
            asyncTicker.End();
        }
        
        //EditorApplication.playModeStateChanged -= PlayModeStateChanged;

        foreach(var zone in _floorPlanManager.RootZones)
        {
            Debug.Log($"zone {zone.ZoneId}: {zone.Cells.Length}");
        }

        return true;
    }

    bool ConditionMeet()
    {
        foreach(var zone in _cellsByZone)
        {
            //Debug.Log($"zone {zone.Key}: {zone.Value.Count}");
            if(zone.Value.Count > 0) return false;
        }
        //Debug.Log("Flood fill done.");
        return true;
    }

    bool SyncFlood()
    {
        while(!ConditionMeet())
        {
            //if(!Application.isPlaying) break;

            Flood();
        }

        return true;
    }


    void SetZoneToCell(int x, int y, int zoneIdIndex)
    {
        _floorPlanManager.CellsGrid.GetCell(x, y, out Cell cell);
        Zone zone = _floorPlanManager.RootZones[zoneIdIndex];
        _cellsByZone[zone.ZoneId].Add(cell);
        //_cells.Add(cell);
        zone.AddCell(cell);

        TriggerOnCellChanged(cell);
    }


    void Flood()
    {
        //int maxWeight = 10;
        //int targetWeight = maxWeight;
        //while(!ConditionMeet())
        //{
        // Para cada grupo de celulas por zona, opera sobre a celula do topo de uma zona
        // se esta esta no limite do peso.
        foreach(var zone in _cellsByZone)
        {
            if(zone.Value.Count > 0)
            {
                if(_weights[zone.Key] >= targetWeight)
                {
                    //if(_cellsByZone[zoneId].Count == 0) return;

                    TrySetNeig(0, -1, zone.Key);
                    TrySetNeig(0, 1, zone.Key);
                    TrySetNeig(1, 0, zone.Key);
                    TrySetNeig(-1, 0, zone.Key);

                    _cellsByZone[zone.Key].RemoveAt(0);
                }
            }
        }
        targetWeight += 1;
        if(targetWeight > maxWeight) targetWeight = 0;
        //}
    }

    void TrySetNeig(int x, int y, string zoneId)
    {
        Cell vizinho;
        if(_floorPlanManager.CellsGrid.GetCell(_cellsByZone[zoneId][0].GridPosition.x + x, _cellsByZone[zoneId][0].GridPosition.y + y, out vizinho))
        {
            // Pula se vizinho ja tem zona.
            if(vizinho.Zone != null) return;

            // Muda zona do vizinho
            Zone zone = _cellsByZone[zoneId][0].Zone;
            //vizinho.SetZone(zone);
            zone.AddCell(vizinho);
            
            // Add vizinho para checagem dos seus vizinhos
            _cellsByZone[zoneId].Add(vizinho);

            TriggerOnCellChanged(vizinho);
        }
    }
/*
    void PlayModeStateChanged(PlayModeStateChange state)
    {
        if(state == PlayModeStateChange.ExitingPlayMode)
        {
            _cts.Cancel();
        }
    }
    */
}

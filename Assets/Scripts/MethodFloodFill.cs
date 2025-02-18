using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using System.Threading;

[System.Serializable]
public class MethodFloodFill : FPGenerationMethod
{
    private List<Cell> _cells;
    private bool _fastMode = false;

    private CancellationTokenSource _cts;

    public override bool Init(FloorPlanManager floorPlanManager)
    {
        Debug.Log("Initializing MethodFloodFill.");

        base.Init(floorPlanManager);

        _cts = new CancellationTokenSource();
        EditorApplication.playModeStateChanged += PlayModeStateChanged;

        _cells = new List<Cell>();
        SetZoneToCell(2,3,0);
        SetZoneToCell(11,5,1);
        SetZoneToCell(12,3,2);
        SetZoneToCell(0,12,3);
        SetZoneToCell(5,9,4);

        return _initialized;
    }

    public async override UniTask<bool> Run()
    {
        if(!EditorApplication.isPlaying)
        {
            Debug.LogError("Don't use it outside play mode.");
            return false;
        }

        if(_fastMode)
        {
            await UniTask.WaitUntil(SyncFlood);
        }
        else
        {
            AsyncTicker asyncTicker = AsyncTicker.Instantiate();
            asyncTicker.Begin(Flood, 0.01f);
                await UniTask.WaitUntil(() => _cells.Count == 0);
            asyncTicker.End();
        }
        
        EditorApplication.playModeStateChanged -= PlayModeStateChanged;

        return true;
    }

    bool SyncFlood()
    {
        while(_cells.Count > 0)
        {
            if(!Application.isPlaying) break;

            Flood();
        }

        return true;
    }


    void SetZoneToCell(int x, int y, int zoneIdIndex)
    {
        _floorPlanManager.CellsGrid.GetCell(x, y, out Cell cell);
        Zone zone = _floorPlanManager.RootZones[zoneIdIndex];
        _cells.Add(cell);
        zone.AddCell(cell);

        TriggerOnCellChanged(cell);
    }


    void Flood()
    {
        if(_cells.Count == 0) return;

        TrySetNeig(1, 0);
        TrySetNeig(-1, 0);
        TrySetNeig(0, 1);
        TrySetNeig(0, -1);

        _cells.RemoveAt(0);
    }

    void TrySetNeig(int x, int y)
    {
        Cell vizinho;
        if(_floorPlanManager.CellsGrid.GetCell(_cells[0].GridPosition.x + x, _cells[0].GridPosition.y + y, out vizinho))
        {
            // Pula se vizinho ja tem zona.
            if(vizinho.Zone != null) return;

            // Muda zona do vizinho
            Zone zone = _cells[0].Zone;
            vizinho.SetZone(zone);

            // Add vizinho para checagem dos seus vizinhos
            _cells.Add(vizinho);

            TriggerOnCellChanged(vizinho);
        }
    }

    void PlayModeStateChanged(PlayModeStateChange state)
    {
        if(state == PlayModeStateChange.ExitingPlayMode)
        {
            _cts.Cancel();
        }
    }
}
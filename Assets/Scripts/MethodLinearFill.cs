using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEditor;
using System.Threading;

[System.Serializable]
public class MethodLinearFill : FPGenerationMethod
{
    private int x, y, WIDTH, HEIGTH, counter;
    private float _delay = 0.01f;
    private bool _fastMode = false;
    private CancellationTokenSource _cts;

    public async override UniTask<bool> Run(FloorPlanGenSceneDebugger sceneDebugger)
    {
        /*
        if(!EditorApplication.isPlaying)
        {
            Debug.LogError("Don't use it outside play mode.");
            return false;
        }
        */

        _cts = new CancellationTokenSource();
        //EditorApplication.playModeStateChanged += PlayModeStateChanged;

        x = 0;
        y = 0;
        counter = 0;
        WIDTH = _floorPlanManager.CellsGrid.Dimensions.x;
        HEIGTH = _floorPlanManager.CellsGrid.Dimensions.y;

        if(_fastMode)
        {
            await UniTask.WaitUntil(SyncChangeCellZone, cancellationToken: _cts.Token);
        }
        else
        {
            AsyncTicker asyncTicker = AsyncTicker.Instantiate();
            asyncTicker.Begin(ChangeCellZone, _delay);
                await UniTask.WaitUntil(() => counter == WIDTH * HEIGTH);
            asyncTicker.End();
        }


        //EditorApplication.playModeStateChanged -= PlayModeStateChanged;

        return true;
    }

    bool SyncChangeCellZone()
    {
        while(counter < WIDTH * HEIGTH)
        {
            ChangeCellZone();
        }

        return true;
    }

    void ChangeCellZone()
    {
        x = counter % WIDTH;
        y = counter / WIDTH;

        if(counter == WIDTH * HEIGTH)
        {
            return;
        }

        Cell cell;
        _floorPlanManager.CellsGrid.GetCell(x, y, out cell);
        Zone zone = _floorPlanManager.RootZones[Random.Range(0, _floorPlanManager.RootZones.Count)];
        zone?.AddCell(cell);

        counter++;

        //TriggerOnCellChanged(cell);
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
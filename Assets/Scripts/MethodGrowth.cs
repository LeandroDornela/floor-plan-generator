using UnityEngine;
using System.Threading;
using UnityEditor;
using Cysharp.Threading.Tasks;

public class MethodGrowth : FPGenerationMethod
{
    private CancellationTokenSource _cts;
    private bool _done = false;
    Zone _currentZone;

    public override bool Init(FloorPlanManager floorPlanManager)
    {
        base.Init(floorPlanManager);

        _cts = new CancellationTokenSource();


        return _initialized;
    }


    public override async UniTask<bool> Run()
    {
        if(!EditorApplication.isPlaying)
        {
            Debug.LogError("Don't use it outside play mode.");
            return false;
        }

        EditorApplication.playModeStateChanged += PlayModeStateChanged;
        AsyncTicker asyncTicker = AsyncTicker.Instantiate();

        // First cell
        _currentZone = _floorPlanManager.RootZone;
        PlotFirstZoneCell(_currentZone);
        TriggerOnCellsGridChanged(_floorPlanManager.CellsGrid);
        await UniTask.WaitForSeconds(0.5f);

        // Growth
        asyncTicker.Begin(Method, 0.5f);
            await UniTask.WaitUntil(()=>_done, cancellationToken: _cts.Token);
        asyncTicker.End();
        EditorApplication.playModeStateChanged -= PlayModeStateChanged;

        return true;
    }

    public void Method()
    {
        // iterar sobre a zona raiz a primeirazona define a area util
        // se a zona ja tem uma predefinição, atribui as celulas correspondentes
        // se não, segue a logi normal. plot inicial, crescimento.

        _currentZone.TryGrowthTop(_floorPlanManager.CellsGrid);
        _currentZone.TryGrowthBottom(_floorPlanManager.CellsGrid);
        _currentZone.TryGrowthRight(_floorPlanManager.CellsGrid);
        _currentZone.TryGrowthLeft(_floorPlanManager.CellsGrid);

        /*
        if(!_currentZone.TryGrowthTop(_floorPlanManager.CellsGrid) &&
           !_currentZone.TryGrowthRight(_floorPlanManager.CellsGrid) &&
           !_currentZone.TryGrowthBottom(_floorPlanManager.CellsGrid) &&
           !_currentZone.TryGrowthLeft(_floorPlanManager.CellsGrid))
        {
            _done = true;
        }
        else
        {
            TriggerOnCellsGridChanged(_floorPlanManager.CellsGrid);
        }
        */

        TriggerOnCellsGridChanged(_floorPlanManager.CellsGrid);
    }


    void PlotFirstZoneCell(Zone zone)
    {
        // TODO calcular os pesos
        // <!--
        // order:-50
        // -->
        // TODO escolher uma aleatoria
        // <!--
        // order:-60
        // -->
        
        Vector2Int position = new Vector2Int(5, 5);
        _floorPlanManager.CellsGrid.AssignCellToZone(position.x, position.y, zone);
    }

    void GrowthZoneRect(Zone zone)
    {
        float aspect = zone.GetZoneAspect();

        if(aspect == zone.DesiredAspect)
        {
            // try any direction
        }
        else if(aspect > zone.DesiredAspect)
        {
            // try vertical
        }
        else
        {
            // try horizontal
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

using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEditor;

[System.Serializable]
public class MethodLinearFill : FPGenerationMethod
{
    private int x, y, WIDTH, HEIGTH, counter;
    private float _delay = 0.01f;

    public async override UniTask<bool> Run()
    {
        if(!EditorApplication.isPlaying)
        {
            Debug.LogError("Don't use it outside play mode.");
            return false;
        }

        x = 0;
        y = 0;
        counter = 0;
        WIDTH = _floorPlanManager.CellsGrid.Dimmensions.x;
        HEIGTH = _floorPlanManager.CellsGrid.Dimmensions.y;

        AsyncTicker asyncTicker = AsyncTicker.Instantiate();

        asyncTicker.Begin(ChangeCellZone, _delay);
            await UniTask.WaitUntil(() => counter == WIDTH * HEIGTH);
        asyncTicker.End();
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
        Zone zone;
        _floorPlanManager.CellsGrid.GetCell(x, y, out cell);
        zone = _floorPlanManager.RootZones[Random.Range(0, _floorPlanManager.RootZones.Count)];
        zone?.AddCell(cell);

        counter++;

        TriggerOnCellChanged(cell);
    }
}
using System.Threading.Tasks;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEditorInternal;
using UnityEditor;

[System.Serializable]
public class MethodLinearFill : FPGenerationMethod
{
    private int x, y, WIDTH, HEIGTH, counter;

    public async override UniTask<bool> Run()
    {
        if(!EditorApplication.isPlaying)
        {
            Debug.LogError("Avoid using it outside play mode.");
            return false;
        }

        x = 0;
        y = 0;
        counter = 0;
        WIDTH = _floorPlanManager.CellsGrid.Dimmensions.x;
        HEIGTH = _floorPlanManager.CellsGrid.Dimmensions.y;

        AsyncTicker asyncTicker = AsyncTicker.Instantiate();

        asyncTicker.Begin(ChangeCellZone, 0.05f);
        await UniTask.WaitUntil(() => counter == WIDTH * HEIGTH);
        asyncTicker.End();
        return true;
    }

    void ChangeCellZone()
    {
        x = counter % WIDTH;
        y = counter / WIDTH;

        Cell cell;
        Zone zone;
        _floorPlanManager.CellsGrid.GetCell(x, y, out cell);
        zone = _floorPlanManager.RootZones[0];
        cell.SetZone(zone);

        counter++;

        TriggerOnCellChanged(cell);
    }
}
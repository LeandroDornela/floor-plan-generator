using UnityEngine;
using Cysharp.Threading.Tasks;

[System.Serializable]
public class MethodFloodFillMult : FPGenerationMethod
{
    public override bool Init(FloorPlanManager floorPlanManager)
    {
        Debug.Log("Initializing MethodFloodFillMult.");

        base.Init(floorPlanManager);

        return _initialized;
    }

    public async override UniTask<bool> Run()
    {
        return false;
    }
}
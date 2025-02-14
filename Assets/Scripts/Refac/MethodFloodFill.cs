using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

[System.Serializable]
public class MethodFloodFill : FPGenerationMethod
{
    public override bool Init(FloorPlanManager floorPlanManager)
    {
        Debug.Log("Initializing MethodFloodFill.");

        base.Init(floorPlanManager);

        return _initialized;
    }

    public async override UniTask<bool> Run()
    {
        return false;
    }
}
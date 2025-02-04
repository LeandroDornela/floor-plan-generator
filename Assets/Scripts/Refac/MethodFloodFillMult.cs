using UnityEngine;

[System.Serializable]
public class MethodFloodFillMult : FPGenerationMethod
{
    public new void Init(FloorPlanManager floorPlanManager)
    {
        Debug.Log("Initializing MethodFloodFillMult.");

        base.Init(floorPlanManager);
    }
}
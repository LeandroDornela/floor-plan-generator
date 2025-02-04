using UnityEngine;

[System.Serializable]
public class MethodFloodFill : FPGenerationMethod
{
    public new void Init(FloorPlanManager floorPlanManager)
    {
        Debug.Log("Initializing MethodFloodFill.");

        base.Init(floorPlanManager);
    }
}
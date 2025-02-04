using UnityEngine;

[System.Serializable]
public class MethodLinearFill : FPGenerationMethod
{
    public new void Init(FloorPlanManager floorPlanManager)
    {
        Debug.Log("Initializing MethodLinearFill.");

        base.Init(floorPlanManager);
    }
}
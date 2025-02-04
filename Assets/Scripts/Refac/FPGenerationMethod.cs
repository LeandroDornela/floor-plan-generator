using UnityEngine;

public class FPGenerationMethod
{
    protected FloorPlanManager _floorPlanManager;
    [SerializeField] protected bool _initialized = false;

    public void Init(FloorPlanManager floorPlanManager)
    {
        _floorPlanManager = floorPlanManager;
        _initialized = true;
    }
}

using UnityEngine;
using Cysharp.Threading.Tasks;

public class FPGenerationMethod : ScriptableObject
{
    protected FloorPlanManager _floorPlanManager;
    [SerializeField] protected bool _initialized = false;


    public virtual bool Init(FloorPlanManager floorPlanManager)
    {
        _floorPlanManager = floorPlanManager;
        _initialized = true;
        return _initialized;
    }

    public virtual async UniTask<bool> Run(FloorPlanGenSceneDebugger sceneDebugger)
    {
        return false;
    }

    public virtual void OnDrawGizmos()
    {
        
    }
}

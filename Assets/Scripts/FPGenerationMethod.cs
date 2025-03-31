using UnityEngine;
using Cysharp.Threading.Tasks;

public class FPGenerationMethod : ScriptableObject
{
    [SerializeField] protected bool _initialized = false;


    public virtual bool Init()
    {
        _initialized = true;
        return _initialized;
    }

    public virtual async UniTask<bool> Run(FloorPlanManager floorPlanManager, FloorPlanGenSceneDebugger sceneDebugger)
    {
        return false;
    }

    public virtual void OnDrawGizmos()
    {
        
    }
}

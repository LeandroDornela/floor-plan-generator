using UnityEngine;
using Cysharp.Threading.Tasks;

public class FPGenerationMethod : ScriptableObject
{
    public virtual async UniTask<bool> Run(FloorPlanManager floorPlanManager, FloorPlanGenSceneDebugger sceneDebugger, int seed)
    {
        return false;
    }

    public virtual void OnDrawGizmos()
    {
        
    }
}

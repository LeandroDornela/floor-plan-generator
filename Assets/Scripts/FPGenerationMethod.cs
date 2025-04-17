using UnityEngine;
using Cysharp.Threading.Tasks;

namespace BuildingGenerator
{
public class FPGenerationMethod : ScriptableObject
{
    public virtual bool Run(FloorPlanManager floorPlanManager, FloorPlanGenSceneDebugger sceneDebugger, int seed)
    {
        return false;
    }

    public virtual async UniTask<bool> DEBUG_RunStepByStep(FloorPlanManager floorPlanManager, FloorPlanGenSceneDebugger sceneDebugger, int seed)
    {
        return false;
    }

    public virtual void OnDrawGizmos()
    {
        
    }
}
}
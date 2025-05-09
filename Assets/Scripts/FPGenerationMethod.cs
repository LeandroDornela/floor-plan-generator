using UnityEngine;
using Cysharp.Threading.Tasks;

namespace BuildingGenerator
{
public class FPGenerationMethod : ScriptableObject
{
    public virtual async UniTask<bool> Run(FloorPlanManager floorPlanManager, FloorPlanGenSceneDebugger sceneDebugger)
    {
        return false;
    }

    public virtual void OnDrawGizmos()
    {
        
    }
}
}
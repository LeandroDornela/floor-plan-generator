using UnityEngine;

namespace BuildingGenerator
{
    [CreateAssetMenu(fileName = "BuildingConfig", menuName = "Building Generator/Building Config")]
    [System.Serializable]
    public class BuildingConfig : ScriptableObject
    {
        [Tooltip("")]
        [SerializeField] private IFloorPlanConfig _floorPlanConfig;
        [Tooltip("")]
        [SerializeField] private BuildingAssetsPack _buildingAssetsPack;

        public IFloorPlanConfig FloorPlanConfig => _floorPlanConfig;
        public BuildingAssetsPack BuildingAssetsPack => _buildingAssetsPack;
    }
}
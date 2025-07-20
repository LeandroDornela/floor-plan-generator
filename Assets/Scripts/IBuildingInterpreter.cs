using UnityEngine;

namespace BuildingGenerator
{
    public abstract class IBuildingInterpreter : MonoBehaviour
    {
        public abstract void Init(BuildingGenerator buildingGenerator, BuildingAssetsPack buildingAssetsPack);

        public abstract void InterpretBuildingData(GeneratedBuildingData generatedBuildingData);
    }
}

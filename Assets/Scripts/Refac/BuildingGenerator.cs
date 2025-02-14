using com.cyborgAssets.inspectorButtonPro;
using UnityEngine;

public class BuildingGenerator : MonoBehaviour
{
    // OBS: Uma varialvel com a ref de uma classe serializada exposta no editor "nunca" será nula.
    [SerializeField] private FloorPlanGenerator _floorPlanGenerator;
    [SerializeField] private BuildingDataManager _buildingDataManager;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    [ProButton]
    public void GenerateBuilding()
    {
        // generate
        // wait generate
        // show result
        _floorPlanGenerator.GenerateFloorPlan(_buildingDataManager.GetTestingFloorPlanConfig());
    }
}

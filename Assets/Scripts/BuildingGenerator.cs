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
    public async void GenerateBuilding()
    {
        if(!Application.isPlaying) return;
        while(true)
        {
            await _floorPlanGenerator.GenerateFloorPlan(_buildingDataManager.GetTestingFloorPlanConfig());
        }
    }
}

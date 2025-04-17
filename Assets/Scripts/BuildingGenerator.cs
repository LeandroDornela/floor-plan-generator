using com.cyborgAssets.inspectorButtonPro;
using UnityEditor;
using UnityEngine;

namespace BuildingGenerator
{
public class BuildingGenerator : MonoBehaviour
{
    // OBS: Uma varialvel com a ref de uma classe serializada exposta no editor "nunca" será nula.
    [SerializeField] private FloorPlanGenerator _floorPlanGenerator;
    [SerializeField] private BuildingDataManager _buildingDataManager;

    public int totalToTest = 1000;

    int counter = 0;

    void Start()
    {
        //GenerateBuildingLoopAsync();
    }

    void Update()
    {
        
    }

    [ProButton]
    public async void GenerateBuildingLoopStepByStep()
    {
        if(!Application.isPlaying) return;
        counter = 0;
        while(counter < totalToTest)
        {
            await _floorPlanGenerator.DEBUG_GenerateFloorPlan(_buildingDataManager.GetFloorPlanData());
            counter++;
            //ScreenCapture.CaptureScreenshot($"{Utils.RandomRange(0, 99999)}.png");
        }
    }

    [ProButton]
    public async void GenerateBuildingStepByStep()
    {
        if(!Application.isPlaying) return;
        await _floorPlanGenerator.DEBUG_GenerateFloorPlan(_buildingDataManager.GetFloorPlanData());
        //ScreenCapture.CaptureScreenshot($"{Utils.RandomRange(0, 99999)}.png");
    }

    [ProButton]
    public async void GenerateBuilding(int amount)
    {
        if(!Application.isPlaying) return;

        await _floorPlanGenerator.GenerateFloorPlans(_buildingDataManager.GetFloorPlanData(), amount);
    }

    void OnDrawGizmos()
    {
        _floorPlanGenerator?.OnDrawGizmos();

        Handles.Label(transform.position, counter.ToString());
    }
}
}
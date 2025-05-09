using com.cyborgAssets.inspectorButtonPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;

namespace BuildingGenerator
{
public class BuildingGenerator : MonoBehaviour
{
    // OBS: Uma varialvel com a ref de uma classe serializada exposta no editor "nunca" será nula.
    [SerializeField] private FloorPlanGenerator _floorPlanGenerator;
    [SerializeField] private BuildingDataManager _buildingDataManager;

    int counter = 0;

    void Start()
    {
        
    }

    void Update()
    {
        
    }


    [ProButton]
    public async void GenerateBuilding(int amount = 1)
    {
        if(!Application.isPlaying) return;

        var result = await _floorPlanGenerator.GenerateFloorPlans(_buildingDataManager.GetFloorPlanData(), amount);
        
        foreach(var plan in result)
         plan.PrintFloorPlan();
    }


    void OnDrawGizmos()
    {
        _floorPlanGenerator?.OnDrawGizmos();

        Handles.Label(transform.position, counter.ToString());
    }
}
}
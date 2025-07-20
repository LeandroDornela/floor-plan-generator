using System.Collections.Generic;
using BuildingGenerator;
using UnityEngine;

//[CreateAssetMenu(fileName = "GeneratedBuildingData", menuName = "Scriptable Objects/Generated Building Data")]
public class GeneratedBuildingData : ScriptableObject
{
    [SerializeField] private List<FloorPlanManager> _generatedFloorPlans = new List<FloorPlanManager>(); // TODO: Ideally it should be another class, just to store plan data.
    public List<FloorPlanManager> GeneratedFloorPlans => _generatedFloorPlans;


    public GeneratedBuildingData(List<FloorPlanManager> generatedPlans)
    {
        _generatedFloorPlans = new List<FloorPlanManager>(generatedPlans);
    }


    public void SetGeneratedPlans(List<FloorPlanManager> generatedPlans)
    {
        //_generatedFloorPlans.Clear();
        _generatedFloorPlans = new List<FloorPlanManager>(generatedPlans);
    }


    public void AddGeneratedPlan(FloorPlanManager newPlan)
    {
        _generatedFloorPlans.Add(newPlan);
    }
}

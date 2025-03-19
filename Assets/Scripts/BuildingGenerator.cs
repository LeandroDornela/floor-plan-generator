using System;
using System.Collections.Generic;
using com.cyborgAssets.inspectorButtonPro;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;

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
    public async void GenerateBuildingLoopAsync()
    {
        if(!Application.isPlaying) return;
        counter = 0;
        while(counter < totalToTest)
        {
            await _floorPlanGenerator.GenerateFloorPlan(_buildingDataManager.GetTestingFloorPlanConfig());
            counter++;
            ScreenCapture.CaptureScreenshot($"{Utils.RandomRange(0, 99999)}.png");
        }
    }

    public async UniTask<bool> TEST()
    {
        List<int> list = new List<int>();
        for(int i = 0; i <= 10000000; i++)
        {
            list.Add(i);
        }
        await UniTask.WaitForSeconds(0.01f);
        return true;
    }

    [ProButton]
    public async void GenerateBuildingAsync()
    {
        if(!Application.isPlaying) return;
        await _floorPlanGenerator.GenerateFloorPlan(_buildingDataManager.GetTestingFloorPlanConfig());
        //ScreenCapture.CaptureScreenshot($"{Utils.RandomRange(0, 99999)}.png");
    }

    [ProButton]
    public void GenerateBuildingLoopSync()
    {
        if(!Application.isPlaying) return;
        counter = 0;
        while(counter < totalToTest)
        {
            _floorPlanGenerator.GenerateFloorPlanSync(_buildingDataManager.GetTestingFloorPlanConfig());
            counter++;
            //ScreenCapture.CaptureScreenshot($"{Utils.RandomRange(0, 99999)}.png");
        }
    }

    void OnDrawGizmos()
    {
        _floorPlanGenerator?.OnDrawGizmos();

        Handles.Label(transform.position, counter.ToString());
    }
}

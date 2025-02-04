using com.cyborgAssets.inspectorButtonPro;
using UnityEngine;

public class BuildingGenerator : MonoBehaviour
{
    // Uma varialvel com a ref de uma classe serializada exposta no editor "nunca" será nula.
    [SerializeField] private FloorPlanGenerator _floorPlanGenerator;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    [ProButton]
    public void GenerateBuilding()
    {
        _floorPlanGenerator.Init();
    }
}

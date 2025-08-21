using UnityEngine;


[CreateAssetMenu(fileName = "BuildingAssetsPack", menuName = "Building Generator/Building Assets Pack")]
[System.Serializable]
public class BuildingAssetsPack : ScriptableObject
{
    public GameObject floorPrefab;
    public GameObject floorBorderPrefab;
    public GameObject wallPrefab;
    public GameObject doorPrefab;
}

using UnityEngine;


[CreateAssetMenu(fileName = "BuildingAssetsPack", menuName = "Scriptable Objects/Building Assets Pack")]
[System.Serializable]
public class BuildingAssetsPack : ScriptableObject
{
    public GameObject floorPrefab;
    public GameObject wallPrefab;
    public GameObject doorPrefab;
}

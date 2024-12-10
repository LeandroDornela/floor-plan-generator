using UnityEngine;

[CreateAssetMenu(fileName = "GeneratorConfig", menuName = "Scriptable Objects/GeneratorConfig")]
public class GeneratorConfig : ScriptableObject
{
    [SerializeField] private Vector2Int _gridDimensions;

    public Vector2Int GridDimensions => _gridDimensions;
}

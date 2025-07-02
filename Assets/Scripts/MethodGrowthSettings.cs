using UnityEngine;

namespace BuildingGenerator
{
    [CreateAssetMenu(fileName = "MethodGrowthSettings", menuName = "Scriptable Objects/Method Growth Settings")]
    [System.Serializable]
    public class MethodGrowthSettings : ScriptableObject
    {
        public enum UnassignedCellsActionEnum { Nullify, ToDesAreaDifference, ToNeighborCellCount, none }


        [Header("General Testing")]
        [SerializeField] private float _delay = 0.01f;
        [SerializeField] private bool _stopAtInitialPlot = false;
        [SerializeField] private bool _skipToFinalResult = false;

        // DATA
        [Header("Weights")]
        [SerializeField] private AnimationCurve _borderDistanceCurve;
        [SerializeField] private AnimationCurve _adjacencyDistanceCurve;
        [SerializeField] private float _adjacencyWeightMultiplier = 1;
        [SerializeField] private float _borderWeightMultiplier = 1;

        [Header("Weights Testing")]
        [SerializeField] private bool _ignoreBorderWeights = false;
        [SerializeField] private bool _ignoreAdjacentWeights = false;


        [Header("Growth Steps")]
        [SerializeField, Min(1)] private int _minLCorridorWidth = 2; // TODO: change to zone side percentage.

        [Header("Growth Steps Testing")]
        [SerializeField] private bool _ignoreDesiredAreaInRect = false;


        [Header("Missing Cells assign")]
        [SerializeField] private UnassignedCellsActionEnum _unassignedCellsAction = UnassignedCellsActionEnum.ToNeighborCellCount;

        [Header("Post process")]
        [SerializeField, Range(0, 4)] private int _maxNeighborsToHaveDoor = 2;


        public float Delay => _delay;
        public bool StopAtInitialPlot => _stopAtInitialPlot;
        public bool SkipToFinalResult => _skipToFinalResult;
        public AnimationCurve BorderDistanceCurve => _borderDistanceCurve;
        public AnimationCurve AdjacencyDistanceCurve => _adjacencyDistanceCurve;
        public float AdjacencyWeightMultiplier => _adjacencyWeightMultiplier;
        public float BorderWeightMultiplier => _borderWeightMultiplier;
        public bool IgnoreBorderWeights => _ignoreBorderWeights;
        public bool IgnoreAdjacentWeights => _ignoreAdjacentWeights;
        public bool IgnoreDesiredAreaInRect => _ignoreDesiredAreaInRect;
        public int MinLCorridorWidth => _minLCorridorWidth;
        public int MaxNeighborsToHaveDoor => _maxNeighborsToHaveDoor;
        public UnassignedCellsActionEnum UnassignedCellsAction => _unassignedCellsAction;
    }
}

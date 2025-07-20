using UnityEngine;

namespace BuildingGenerator
{
    [CreateAssetMenu(fileName = "MethodGrowthSettings", menuName = "Building Generator/Method Growth Settings")]
    [System.Serializable]
    public class MethodGrowthSettings : ScriptableObject
    {
        public enum UnassignedCellsActionEnum { Nullify, ToDesAreaDifference, ToNeighborCellCount, none }

        [Header("General settings")]
        // DATA
        //[Header("Weights")]
        [Space]
        [SerializeField] private AnimationCurve _borderDistanceCurve;
        [SerializeField] private AnimationCurve _adjacencyDistanceCurve;
        [SerializeField] private float _adjacencyWeightMultiplier = 1;
        [SerializeField] private float _borderWeightMultiplier = 1;

        //[Header("Growth Steps")]
         [Space]
        [SerializeField, Min(1)] private int _minLCorridorWidth = 2; // TODO: change to zone side percentage.

        //[Header("Missing Cells Assign Step")]
         [Space]
        [SerializeField] private UnassignedCellsActionEnum _unassignedCellsAction = UnassignedCellsActionEnum.ToNeighborCellCount;

        //[Header("Post Process Step")]
        [Space]
        [SerializeField, Range(0, 4)] private int _maxNeighborsToHaveDoor = 2;


        [Header("Debug")]
        [SerializeField] private float _delay = 0.01f;
        [SerializeField] private bool _stopAtInitialPlot = false;
        [SerializeField] private bool _skipToFinalResult = false;
        // Growth Steps Testing
        [SerializeField] private bool _ignoreDesiredAreaInRect = false;
        // Weights Testing
        [SerializeField] private bool _ignoreBorderWeights = false;
        [SerializeField] private bool _ignoreAdjacentWeights = false;


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

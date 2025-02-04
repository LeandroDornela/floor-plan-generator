using UnityEngine;

[System.Serializable]
public class FloorPlanGenerator
{
    private FloorPlanManager _floorPlanManager;

    [SerializeField] private MethodLinearFill _methodLinearFill;
    [SerializeField] private MethodFloodFill _methodFloodFill;
    [SerializeField] private MethodFloodFillMult _methodFloodFillMult;

    [Header("Debug")]
    [SerializeField] private FloorPlanGenSceneDebugger _sceneDebuggerPrefab;
    [SerializeField] private bool _enableDebug = false;
    private GameObject _visualDebuggerInstance;

    private bool _initialized = false;

    public void Init()
    {
        Debug.Log("Initializing floor plan generator.");

        _floorPlanManager = new FloorPlanManager();
        _floorPlanManager.Init();

        // TODO: se os metodos não tiverem parametros expostos no editor o ideal seria ocultarlos e "dar new"
        _methodLinearFill.Init(_floorPlanManager);
        _methodFloodFill.Init(_floorPlanManager);
        _methodFloodFillMult.Init(_floorPlanManager);

        if(_enableDebug)
        {
            if(_visualDebuggerInstance != null)
            {
                Object.DestroyImmediate(_visualDebuggerInstance);
            }

            _visualDebuggerInstance = Object.Instantiate(_sceneDebuggerPrefab).gameObject;
        }

        _initialized = true;
    }
}

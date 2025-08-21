using UnityEditor;
using UnityEngine;
using BuildingGenerator;

public class AutoCameraOrbiter : MonoBehaviour
{
    public float speed = 10;
    public bool rotate = true;
    public bool translate = true;


    private Vector3 targetPos;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Selection.selectionChanged += OnSelectionChanged;
    }

    // Update is called once per frame
    void Update()
    {
        if(rotate) transform.Rotate(Vector3.up, Time.deltaTime * speed);
        if(translate) transform.Translate((targetPos - transform.position)*Time.deltaTime * speed, Space.World);
    }
    
    void OnSelectionChanged()
    {
        GameObject selected = Selection.activeGameObject;

        if (selected == null) return;

        FloorPlanGenSceneDebugger sceneDebugger = selected.GetComponent<FloorPlanGenSceneDebugger>();

        if (sceneDebugger == null) return;

        Vector2Int posCorrect = Vector2Int.zero;

        if (sceneDebugger.CurrentFloorPlan != null)
        {
            posCorrect = sceneDebugger.CurrentFloorPlan.CellsGrid.Dimensions/2;
        }

        targetPos = new Vector3(sceneDebugger.transform.position.x + posCorrect.x -0.5f, 0, sceneDebugger.transform.position.z + posCorrect.y -0.5f);
    }
}

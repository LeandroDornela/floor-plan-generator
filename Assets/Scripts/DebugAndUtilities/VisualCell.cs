using UnityEditor;
using UnityEngine;

namespace BuildingGenerator
{
    public class VisualCell : MonoBehaviour
    {
        public Renderer _renderer;
        [SerializeReference] private Cell _cell;

        public bool _drawDebug = false;
        public bool _drawNeighborConnections = false;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            if (_renderer == null) { _renderer = GetComponent<Renderer>(); }
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void Init(Cell cell)
        {
            _cell = cell;
            gameObject.name = "Cell_" + cell.GridPosition.x + "_" + cell.GridPosition.y;
            if (Application.isPlaying)
            {
                _renderer.material.color = Color.black;
            }
            else
            {
                _renderer.sharedMaterial.color = Color.black;
            }
        }

        public void SetColor(Color color, Cell cell)
        {
            if (_renderer == null) { _renderer = GetComponent<Renderer>(); }
            //_renderer.material.color = _renderer.material.color + color/100;
            if (Application.isPlaying)
            {
                _renderer.material.color = color;
            }
            else
            {
                _renderer.sharedMaterial.color = color;
            }


            _cell = cell;
        }

        public void SetSelectedState(bool state)
        {
            if (state)
            {
                Debug.Log("selected");
                Color origCol = _renderer.material.color;
                _renderer.material.color = new Color(origCol.r, origCol.g, origCol.b, 0.5f);
            }
            else
            {
                Debug.Log("deselected");
                Color origCol = _renderer.material.color;
                _renderer.material.color = new Color(origCol.r, origCol.g, origCol.b, 1f);
            }
        }

        void OnDrawGizmos()
        {
            if (!_drawDebug) return;

            string zoneId;

            if (_cell != null && _cell.Zone != null)
            {
                zoneId = _cell.Zone.ZoneId;
            }
            else
            {
                zoneId = "";
            }

            // Create a new GUIStyle
            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.black;
            style.alignment = TextAnchor.MiddleCenter;
            Handles.Label(transform.position, $"[{_cell.GridPosition.x}, {Mathf.Abs(_cell.GridPosition.y)}]\n{zoneId}", style);

            if (_cell != null && _drawNeighborConnections)
            {
                Gizmos.color = Color.yellow;
                Cell neighbor;
                neighbor = _cell.TopNeighbor;
                if (neighbor != null)
                {
                    Gizmos.DrawLine(transform.position, new Vector3(neighbor.GridPosition.x, 0, -neighbor.GridPosition.y));
                }
                neighbor = _cell.TopRightNeighbor;
                if (neighbor != null)
                {
                    Gizmos.DrawLine(transform.position, new Vector3(neighbor.GridPosition.x, 0, -neighbor.GridPosition.y));
                }
                neighbor = _cell.RightNeighbor;
                if (neighbor != null)
                {
                    Gizmos.DrawLine(transform.position, new Vector3(neighbor.GridPosition.x, 0, -neighbor.GridPosition.y));
                }
                neighbor = _cell.RightBottomNeighbor;
                if (neighbor != null)
                {
                    Gizmos.DrawLine(transform.position, new Vector3(neighbor.GridPosition.x, 0, -neighbor.GridPosition.y));
                }
                neighbor = _cell.BottomNeighbor;
                if (neighbor != null)
                {
                    Gizmos.DrawLine(transform.position, new Vector3(neighbor.GridPosition.x, 0, -neighbor.GridPosition.y));
                }
                neighbor = _cell.BottomLeftNeighbor;
                if (neighbor != null)
                {
                    Gizmos.DrawLine(transform.position, new Vector3(neighbor.GridPosition.x, 0, -neighbor.GridPosition.y));
                }
                neighbor = _cell.LeftNeighbor;
                if (neighbor != null)
                {
                    Gizmos.DrawLine(transform.position, new Vector3(neighbor.GridPosition.x, 0, -neighbor.GridPosition.y));
                }
                neighbor = _cell.LeftTopNeighbor;
                if (neighbor != null)
                {
                    Gizmos.DrawLine(transform.position, new Vector3(neighbor.GridPosition.x, 0, -neighbor.GridPosition.y));
                }
            }
        }
    }
}
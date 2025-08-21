using UnityEditor;
using UnityEngine;

namespace BuildingGenerator
{
    public class VisualCell : MonoBehaviour
    {
        public Renderer _renderer;
        [SerializeReference] private Cell _cell;
       

        public bool _drawDebug = true;
        public bool _drawNeighborConnections = false;
        public bool _drawCoords = true;
        public bool _drawZoneId = true;

        public Vector3 textCorrect = Vector3.zero;

        public ScaleAnimation _scaleAnimation;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            if (_renderer == null) { _renderer = GetComponent<Renderer>(); }
            if (_scaleAnimation == null) { _scaleAnimation = GetComponent<ScaleAnimation>(); }
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
            if (!gameObject.activeSelf) return;

            if (_renderer == null) { _renderer = GetComponent<Renderer>(); }
            //_renderer.material.color = _renderer.material.color + color/100;

            //if(cell?.Zone != null) if (cell.Zone.IsRoot) color = Color.white;
            try
            {
                if(cell?.Zone != null) if (cell.Zone.IsRoot) color = Color.white;
            }
            catch
            {
                Debug.LogError("Error in SetColor");
            }

            if (Application.isPlaying)
            {
                _renderer.material.color = color;
            }
            else
            {
                _renderer.sharedMaterial.color = color;
            }


            _cell = cell;

            _scaleAnimation?.TriggerAnimation(_cell.GridPosition.x);
        }

        public void HideGraphics()
        {
            gameObject.SetActive(false);
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

            if (_cell != null && _cell.Zone != null && _drawZoneId)
            {
                zoneId = _cell.Zone.ZoneId;
            }
            else
            {
                zoneId = "";
            }

            string coordText = "";
            if (_drawCoords)
            {
                coordText = $"[{_cell.GridPosition.x}, {Mathf.Abs(_cell.GridPosition.y)}]\n";
            }

            // Create a new GUIStyle
            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.black;
            style.alignment = TextAnchor.MiddleCenter;

            Handles.Label(transform.position + textCorrect, $"{coordText}{zoneId}", style);

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
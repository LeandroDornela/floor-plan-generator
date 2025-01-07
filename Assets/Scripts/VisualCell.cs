using UnityEditor;
using UnityEngine;

public class VisualCell : MonoBehaviour
{
    public Renderer _renderer;
    public Cell _cell;

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
    }

    public void SetColor(Color color)
    {
        if(_renderer == null) { _renderer = GetComponent<Renderer> (); }
        _renderer.material.color = color;
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
}

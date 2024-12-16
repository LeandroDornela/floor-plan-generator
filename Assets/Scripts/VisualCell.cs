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
}

using UnityEngine;

public class Ticker : MonoBehaviour
{

    public Generator generator;
    public float repeatRate = 0.5f;

    private int counter = 0;

    private int WIDTH;
    private int HEIGTH;

    private int x;
    private int y;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        WIDTH = generator._generatorConfig.GridDimensions.x;
        HEIGTH = generator._generatorConfig.GridDimensions.y;

        InvokeRepeating("Tick", 1, repeatRate);
    }

    void Tick()
    {
        // mover para uma função em outra classe que funciona sozinho
        Debug.Log(counter);
        x = counter % WIDTH;
        y = counter / WIDTH;
        Debug.Log("x: " + x + " y: " + y);

        Cell cell;
        Zone zone;
        generator._cellsGrid.GetCell(x, y, out cell);
        zone = generator._zonesHierarchy._zonesTree[0];
        cell.SetZone(zone);

        cell.visualCell.SetColor(zone._color);

        counter++;
    }
}

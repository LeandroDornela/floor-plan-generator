using UnityEngine;

public class Utils
{
    public static int RandomRange(int min, int max)
    {
        return Random.Range(min, max);
    }

    public static float RandomRange(float min, float max)
    {
        return Random.Range(min, max);
    }

    public static Vector2Int ArrayIndexToMatrix(int index, int size_x)
    {
        return new Vector2Int(index / size_x, index % size_x);
    }

    public static int MatrixToArrayIndex(int x, int y, int size_x)
    {
        return size_x * y + x;
    }
}

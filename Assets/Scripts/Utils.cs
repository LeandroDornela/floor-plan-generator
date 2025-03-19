//using Unity.Mathematics;
using UnityEngine;

public class Utils
{
    public static bool RandomBool()
    {
        return Random.Range(0, 2) == 0;
    }

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


    public static void PrintArrayAsGrid(int width, int height, float[] data)
    {
        if(width * height != data.Length)
        {
            Debug.LogError("Size mismatch.");
            return;
        }

        /*
        string result = "\n";
        for(int i = 0; i < height; i++)
        {
            for(int j = 0; j < width; j++)
            {
                result += $"[{data[i * width + j]:0.00}]";
            }
            result += "\n";
        }
        */

        string result = "[";
        for(int i = 0; i < height; i++)
        {
            for(int j = 0; j < width; j++)
            {
                int val = Mathf.CeilToInt(data[i * width + j] * 255);
                if(j == width - 1 && i == height - 1) result += $"{val},{val},{val}";
                else result += $"{val},{val},{val},";
            }
        }
        result += "]";
        
        Debug.Log(result);
    }
}

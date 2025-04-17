using UnityEngine;

namespace BuildingGenerator
{
public class Utils
{
    public class Random
    {
        private static bool _useSeed = false;
        private static int _seed = 0;

        static void CheckUseSeed()
        {
            if(_useSeed)
            {
                UnityEngine.Random.InitState(_seed);
                _seed++;
            }
        }

        public static void SetSeed(int seed)
        {
            _useSeed = true;
            _seed = seed;
        }

        public static void CleanSeed()
        {
            _useSeed = false;
            _seed = 0;
        }

        public static bool RandomBool(bool forceIgnoreSeed = false)
        {
            if(!forceIgnoreSeed) CheckUseSeed();
            return UnityEngine.Random.Range(0, 2) == 0;
        }

        public static int RandomRange(int min, int max, bool forceIgnoreSeed = false)
        {
            if(!forceIgnoreSeed) CheckUseSeed();
            return UnityEngine.Random.Range(min, max);
        }

        public static float RandomRange(float min, float max, bool forceIgnoreSeed = false)
        {
            if(!forceIgnoreSeed) CheckUseSeed();
            return UnityEngine.Random.Range(min, max);
        }
    }


    public static Vector2Int ArrayIndexToMatrix(int index, int width)
    {
        return new Vector2Int(index / width, index % width);
    }

    public static int MatrixToArrayIndex(int x, int y, int width)
    {
        return width * y + x;
    }


    public static void PrintArrayAsGrid(int width, int height, float[] data)
    {
        if(width * height != data.Length)
        {
            Debug.LogWarning("Size mismatch.");
            //return;
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

        float largest = 0;
        foreach(var val in data)
        {
            if(val > largest)
            {
                largest = val;
            }
        }

        string result = "[";
        for(int i = 0; i < height; i++)
        {
            for(int j = 0; j < width; j++)
            {
                //float val = data[i * width + j];
                //if(j == width - 1 && i == height - 1) result += $"{val:0.00}";
                //else result += $"{val:0.00} ";
                
                int index = i * width + j;
                int val;

                if(index < data.Length)
                {
                    val = Mathf.CeilToInt(data[index] * 255);
                }
                else
                {
                    val = 0;
                }
                
                string col;

                if(val > 255)
                {
                    col = $"{255},{0},{0}";
                }
                else if(val < 0)
                {
                    col = $"{255},{0},{255}";
                }
                else
                {
                    col = $"{val},{val},{val}";
                }

                
                
                if(j == width - 1 && i == height - 1) result += col;
                else result += $"{col},";
            }
        }
        result += "]";
        
        Debug.Log(result);
    }
}
}
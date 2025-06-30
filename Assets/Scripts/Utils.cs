using System.Collections.Generic;
using UnityEngine;

namespace BuildingGenerator
{
    public class Utils
    {
        public class Random
        {
            // NOTE: Unity Random doesn't work outside the main thread.

            static System.Random random;

            static void ValidateInstance()
            {
                if (random == null)
                {
                    random = new System.Random();
                }
            }

            public static void SetSeed(int seed)
            {
                random = new System.Random(seed);

                //UnityEngine.Random.InitState(seed);
            }

            public static void ClearSeed()
            {
                random = new System.Random();

                //UnityEngine.Random.InitState((int)System.DateTime.Now.Ticks);
            }

            public static bool RandomBool()
            {
                ValidateInstance();
                return random.Next(0, 2) == 0;

                //return UnityEngine.Random.Range(0, 2) == 0;
            }

            public static int RandomRange(int min, int max)
            {
                ValidateInstance();
                return random.Next(min, max);

                //return UnityEngine.Random.Range(min, max);
            }

            public static float RandomRange(float min, float max)
            {
                ValidateInstance();
                return (float)random.NextDouble() * (max - min) + min;

                //return UnityEngine.Random.Range(min, max);
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
            if (width * height != data.Length)
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
            foreach (var val in data)
            {
                if (val > largest)
                {
                    largest = val;
                }
            }

            string result = "[";
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    //float val = data[i * width + j];
                    //if(j == width - 1 && i == height - 1) result += $"{val:0.00}";
                    //else result += $"{val:0.00} ";

                    int index = i * width + j;
                    int val;

                    if (index < data.Length)
                    {
                        val = Mathf.CeilToInt(data[index] * 255);
                    }
                    else
                    {
                        val = 0;
                    }

                    string col;

                    if (val > 255)
                    {
                        col = $"{255},{0},{0}";
                    }
                    else if (val < 0)
                    {
                        col = $"{255},{0},{255}";
                    }
                    else
                    {
                        col = $"{val},{val},{val}";
                    }



                    if (j == width - 1 && i == height - 1) result += col;
                    else result += $"{col},";
                }
            }
            result += "]";

            Debug.Log(result);
        }

        public static Texture2D ResizeWithNearest(Texture2D source, int newWidth, int newHeight)
        {
            // Set up a temporary RenderTexture
            RenderTexture rt = RenderTexture.GetTemporary(newWidth, newHeight);
            rt.filterMode = FilterMode.Point; // Nearest-neighbor filtering

            // Set source texture's filter mode
            source.filterMode = FilterMode.Point;

            // Copy source to RenderTexture
            RenderTexture.active = rt;
            Graphics.Blit(source, rt);

            // Create new Texture2D and read pixels from RenderTexture
            Texture2D result = new Texture2D(newWidth, newHeight, source.format, false);
            result.filterMode = FilterMode.Point;
            result.ReadPixels(new Rect(0, 0, newWidth, newHeight), 0, 0);
            result.Apply();

            // Clean up
            RenderTexture.active = null;
            RenderTexture.ReleaseTemporary(rt);

            return result;
        }
    }
}
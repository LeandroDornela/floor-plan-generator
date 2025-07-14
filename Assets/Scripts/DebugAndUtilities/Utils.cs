using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace BuildingGenerator
{
    public class Utils
    {
        public class Random// : MonoBehaviour
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


        [System.Serializable]
        public class Debug
        {
            [NaughtyAttributes.ShowNonSerializedField]
            public static bool _enable = true;
            public static bool _enableDevLogs = true; // Dev logs should be disabled in production
            public static bool _enableLog = true;
            public static bool _enableWarning = true;
            public static bool _enableError = true;

            // General and production logs
            public static void Log(string val)
            {
                if (_enable && _enableLog && _enableDevLogs) UnityEngine.Debug.Log(val);
            }

            public static void Warning(string val)
            {
                if (_enable && _enableWarning && _enableDevLogs) UnityEngine.Debug.LogWarning(val);
            }

            public static void Error(string val)
            {
                if (_enable && _enableError && _enableDevLogs) UnityEngine.Debug.LogError(val);
            }

            // Development only logs
            public static void DevLog(string val)
            {
                if (_enable && _enableLog && _enableDevLogs) UnityEngine.Debug.Log(val);
            }

            public static void DevWarning(string val)
            {
                if (_enable && _enableWarning && _enableDevLogs) UnityEngine.Debug.LogWarning(val);
            }

            public static void DevError(string val)
            {
                if (_enable && _enableError && _enableDevLogs) UnityEngine.Debug.LogError(val);
            }
        }


        public class Stopwatch
        {
            System.Diagnostics.Stopwatch _watch;

            public Stopwatch()
            {
                _watch = System.Diagnostics.Stopwatch.StartNew();
            }

            public System.Diagnostics.Stopwatch Stop()
            {
                _watch.Stop();
                return _watch;
            }
        }

        #region =============== STATIC METHODS ===============
        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="width"></param>
        /// <returns></returns>
        public static Vector2Int ArrayIndexToMatrix(int index, int width)
        {
            return new Vector2Int(index / width, index % width);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <returns></returns>
        public static int MatrixToArrayIndex(int x, int y, int width)
        {
            return width * y + x;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="data"></param>
        public static void PrintArrayAsGrid(int width, int height, float[] data)
        {
            if (width * height != data.Length)
            {
                UnityEngine.Debug.LogWarning("Size mismatch.");
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

            UnityEngine.Debug.Log(result);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="newWidth"></param>
        /// <param name="newHeight"></param>
        /// <returns></returns>
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


        /// <summary>
        /// 
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static double CalculateMean(List<double> values)
        {
            double sum = 0;

            foreach (double value in values)
            {
                sum += value;
            }

            return sum / values.Count;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static double CalculateMedian(List<double> values)
        {
            List<double> sorted = new List<double>(values);
            sorted.Sort();

            int count = sorted.Count;

            if (count % 2 == 0)
            {
                int index = count / 2;
                return (sorted[index] + sorted[index - 1]) / 2.0;
            }
            else
            {
                int index = (count - 1) / 2;
                return sorted[index];
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sufix"></param>
        public static void Screenshot(string sufix)
        {
            string fileName = $"{DateTime.Now:yyyyMMdd_HHmmssfffffff}_{sufix}.png";
            string path = Directory.GetParent(Application.dataPath).FullName;
            path = Path.Combine(path, "Screenshots", fileName);
            ScreenCapture.CaptureScreenshot(path);
        }
    }
    #endregion
}
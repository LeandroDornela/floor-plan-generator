using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Rendering;

namespace BuildingGenerator
{
    // Contadores, tempo, etc.
    public class GenerationStats
    {
        [System.Serializable]
        public enum TimeUnit
        {
            ElapsedMilliseconds,
            ElapsedTicks
        }

        [System.Serializable]
        public class NamedGrid
        {
            public string _id;
            public float[] _values;
            public int _sizeX;

            public NamedGrid(string id, float[] values, int sizeX)
            {
                _id = id;
                _values = values;
                _sizeX = sizeX;
            }
        }

        [System.Serializable]
        public class TimingEnter
        {
            public string _key;
            public double _mean;
            public double _median;
            public List<double> _values;

            public TimingEnter(string key)
            {
                _key = key;
                _mean = 0;
                _values = new List<double>();
            }

            public void AddValue(double newValue)
            {
                _values.Add(newValue);
            }

            public void UpdateAverages()
            {
                _mean = Utils.CalculateMean(_values);
                _median = Utils.CalculateMedian(_values);
            }


        }

        public string _generationID;
        public int _seed;
        public int _totalGenerationTries;
        public int _totalFails;
        public List<NamedGrid> _zonePlotWeightGrids;
        public List<TimingEnter> _timingEnters;
        public SerializedDictionary<string, string> _customData;

        private TimeUnit _timeUnit = TimeUnit.ElapsedMilliseconds;

        private static GenerationStats _instance;
        public static GenerationStats Instance => _instance;

        public GenerationStats(FloorPlanData floorPlanData)
        {
            _generationID = $"{DateTime.Now:yyyyMMdd_HHmmssfffffff}_{floorPlanData.FloorPlanId}";
            _zonePlotWeightGrids = new List<NamedGrid>();
            _timingEnters = new List<TimingEnter>();
            _instance = this;
        }

        public void AddTimeEnter(string key, System.Diagnostics.Stopwatch watch)
        {
            TimingEnter result = _timingEnters.Where(enter => enter._key == key).SingleOrDefault();

            double value = 0;

            if (_timeUnit == TimeUnit.ElapsedMilliseconds)
            {
                value = watch.ElapsedMilliseconds;
            }
            else if (_timeUnit == TimeUnit.ElapsedTicks)
            {
                value = watch.ElapsedTicks;
            }
            else
            {
                Debug.LogError("Undefined TimeUnit.");
            }


            if (result == null)
            {
                // Add new enter.
                TimingEnter newEnter = new TimingEnter(key);
                newEnter.AddValue(value);

                _timingEnters.Add(newEnter);
            }
            else
            {
                result.AddValue(value);
            }
        }

        public List<double> GetTimeEnters(string key)
        {
            return _timingEnters.Where(enter => enter._key == key).SingleOrDefault()?._values;
        }


        public void SaveStatsAsJsonFile()
        {
            foreach (var enter in _timingEnters)
            {
                enter.UpdateAverages();
            }

            string json = JsonUtility.ToJson(this, true);

            string fileName = $"{_generationID}.json";
            string path = Directory.GetParent(Application.dataPath).FullName;
            path = Path.Combine(path, "Tests", fileName);

            File.WriteAllText(path, json);

            Debug.Log($"Saved JSON to: {path}");
        }


        public void AddCustomData(string key, string value)
        {
            if (_customData == null)
            {
                _customData = new SerializedDictionary<string, string>();
            }

            if (_customData.ContainsKey(key))
            {
                _customData[key] = value;
            }
            else
            {
                _customData.Add(key, value);
            }
        }
    }
}

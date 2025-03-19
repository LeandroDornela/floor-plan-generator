using System.Collections.Generic;
using UnityEngine;

public class WeightedArray
{
    private float[] _array;
    private float _summation;

    public float[] Values => _array;
    public float Summation => _summation;


    public WeightedArray(int length)
    {
        _array = new float[length];
    }

    public WeightedArray(List<float> values)
    {
        _array = new float[values.Count];

        for(int i = 0; i < values.Count; i++)
        {
            _array[i] = values[i];
            _summation += _array[i];
        }
    }

    public WeightedArray(float[] values)
    {
        _array = new float[values.Length];

        for(int i = 0; i < values.Length; i++)
        {
            _array[i] = values[i];
            _summation += _array[i];
        }
    }


    public void AddAt(int index, float value)
    {
        _array[index] = value;
        _summation += value;
    }


    /// <summary>
    /// Return a index of weights array, more common values tend to be selected.
    /// </summary>
    public int GetRandomWeightedIndex()
    {
        float summationToFind = Utils.RandomRange(0, _summation);

        float localSummation = 0;
        for(int i = 0; i < _array.Length; i++)
        {
            localSummation += _array[i];

            if(localSummation >= summationToFind)
            {
                return i;
            }
        }

        return -1;
    }

    public bool GetRandomWeightedElement<T>(T[] elementsArray, out T result)
    {
        if(elementsArray.Length != _array.Length)
        {
            Debug.LogError($"The array need to be the same size. Expected size: {_array.Length}, received array size: {elementsArray.Length}");
            result = default;
            return false;
        }

        int index = GetRandomWeightedIndex();

        if(index >= 0)
        {
            result = elementsArray[index];
            return true;
        }

        result = default;
        return false;
    }
}

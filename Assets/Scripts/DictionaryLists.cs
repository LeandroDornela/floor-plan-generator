using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DictionaryDictionaryList<TKey, TList>
{
    private Dictionary<TKey, DictionaryList<TKey, TList>> _dictionary;
    public Dictionary<TKey, DictionaryList<TKey, TList>> Dictionary => _dictionary;

    public DictionaryDictionaryList()
    {
        _dictionary = new Dictionary<TKey, DictionaryList<TKey, TList>>();
    }

    public DictionaryDictionaryList(TKey[] firstLayer)
    {
        _dictionary = new Dictionary<TKey, DictionaryList<TKey, TList>>();

        foreach (TKey value in firstLayer)
        {
            _dictionary.Add(value, new DictionaryList<TKey, TList>());
        }
    }

    /// <summary>
    /// Key order independent.
    /// If value is null, only the keys will be added.
    /// </summary>
    /// <param name="key1"></param>
    /// <param name="key2"></param>
    /// <param name="value"></param>
    public void AddValue(TKey key1, TKey key2, TList value)
    {
        // Avoid crossing, like A->B and B->A, the first key to enter is the one that is in the first.
        // It is to make key enter order independent. Like "A ordem dos fatores não altera o produto."
        if (_dictionary.ContainsKey(key1))
        {
            //if (!_dictionary.ContainsKey(key2))
            _dictionary[key1].AddValue(key2, value);
        }
        else if (_dictionary.ContainsKey(key2))
        {
            _dictionary[key2].AddValue(key1, value);
        }
        else
        {
            _dictionary.Add(key1, new DictionaryList<TKey, TList>());
            _dictionary[key1].AddValue(key2, value);
        }
    }


    /// <summary>
    /// Key order independent.
    /// </summary>
    public bool TryGetValue(TKey key1, TKey key2, out List<TList> outValue)
    {
        DictionaryList<TKey, TList> value;

        // Try 1 then 2
        if (_dictionary.TryGetValue(key1, out value))
        {
            if (value.TryGetValue(key2, out outValue))
            {
                return true;
            }
        }

        // Try 2 then 1
        if (_dictionary.TryGetValue(key2, out value))
        {
            if (value.TryGetValue(key1, out outValue))
            {
                return true;
            }
        }

        // If there is not 1/2 or 2/1, the pair don't exist.
        outValue = null;
        return false;
    }
}

[System.Serializable]
public class DictionaryList<TKey, TList>
{
    private Dictionary<TKey, List<TList>> _dictionary;
    public Dictionary<TKey, List<TList>> Dictionary => _dictionary;

    public DictionaryList()
    {
        _dictionary = new Dictionary<TKey, List<TList>>();
    }

    public void AddValue(TKey key, TList value)
    {
        if (_dictionary.ContainsKey(key))
        {
            if (value == null)
            {
                Debug.LogError("Can't add null to a list of existing key.");
                return;
            }

            _dictionary[key].Add(value);
        }
        else
        {
            if (value == null)
            {
                _dictionary.Add(key, new List<TList>());
            }
            else
            {
                _dictionary.Add(key, new List<TList> { value });
            }
        }
    }

    public bool TryGetValue(TKey key, out List<TList> outValue)
    {
        if (_dictionary.TryGetValue(key, out outValue))
        {
            return true;
        }

        outValue = null;
        return false;
    }
}

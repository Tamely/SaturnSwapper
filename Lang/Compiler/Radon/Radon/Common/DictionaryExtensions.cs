using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;

namespace Radon.Common;

public static class DictionaryExtensions
{
    public static int IndexOf<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, IEqualityComparer<TKey> comparer)
        where TKey : notnull
    {
        var index = 0;
        foreach (var (k, _) in dictionary)
        {
            if (comparer.Equals(k, key))
            {
                return index;
            }
            
            index++;
        }
        
        return -1;
    }
    
    public static TKey KeyAt<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, int index)
        where TKey : notnull
    {
        var i = 0;
        foreach (var (k, _) in dictionary)
        {
            if (i == index)
            {
                return k;
            }
            
            i++;
        }
        
        throw new IndexOutOfRangeException();
    }
    
    public static TValue ValueAt<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, int index)
        where TKey : notnull
    {
        var i = 0;
        foreach (var (_, v) in dictionary)
        {
            if (i == index)
            {
                return v;
            }
            
            i++;
        }
        
        throw new IndexOutOfRangeException();
    }
}

public static class ReadonlyDictionaryExtensions
{
    public static TValue ValueAt<TKey, TValue>(this ReadOnlyDictionary<TKey, TValue> dictionary, int index)
        where TKey : notnull
    {
        var i = 0;
        foreach (var (_, v) in dictionary)
        {
            if (i == index)
            {
                return v;
            }
            
            i++;
        }
        
        throw new IndexOutOfRangeException();
    }
}

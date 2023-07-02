template Template<T>
{
    T Field
    
    T Method(T param)
    {
        T x = default: T
        return x
    }

    Template<int> IntTemaplte()
    {
        Template<int> intTemaplte = new Template<int>()
        TemplateMethods.TemplateMethod<int>(10)
        return intTemaplte
    }
}

template KeyValuePair<K, V>
{
    K Key
    V Value

    KeyValuePair(K key, V value)
    {
        Key = key
        Value = value
    }
}

template Dictionary<K, V>
{
    list<KeyValuePair<K, V>> Items

    Dictionary()
    {
        Items = new list<KeyValuePair<K, V>>()
    }

    void Add(K key, V value)
    {
        Items.Add(new KeyValuePair<K, V>(key, value))
    }
}

struct TemplateMethods
{
    int Field

    static template list<T> TemplateMethod<T>(T param)
    {
        T x = default: T
        var tList = new list<T>()
        tList.Add(x)
        return tList
    }
}

public template KeyValuePair<TKey, TValue>
{
    public TKey Key;
    public TValue Value;

    public KeyValuePair(TKey key, TValue value)
    {
        Key = key;
        Value = value;
    }
}

public template Dictionary<TKey, TValue>
{
    private list<KeyValuePair<TKey, TValue>> _list;

    public Dictionary()
    {
        _list = new list<KeyValuePair<TKey, TValue>>();
    }

    public void Add(TKey key, TValue value)
    {
        _list.Add(new KeyValuePair<TKey, TValue>(key, value));
    }
}

public template List<T>
{
    private T[] _items;

    public List()
    {
        _items = new T[0];
    }
}

public enum MyEnum
{
    A = 10,
    B = 20,
    C = 30
}

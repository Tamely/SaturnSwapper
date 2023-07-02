template Dictionary<K, V>
{
    list<KeyValuePair<K, V>> Items;

    Dictionary()
    {
        Items = new list<KeyValuePair<K V>>();
    }

    void Add(K key, V value)
    {
        Items.Add(new KeyValuePair<K, V>(key, value));
    }
}

template KeyValuePair<K, V>
{
    K Key;
    V Value;

    KeyValuePair(K key, V value)
    {
        Key = key;
        Value = value;
    }
}

struct Program
{
    void Main()
    {
        Dictionary<string, int> dict = new Dictionary<string, int>();
        dict.Add("one", 1);
        dict.Add("two", 2);
        dict.Add("three", 3);
    }
}

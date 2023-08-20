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
    private List<KeyValuePair<TKey, TValue>> _list;

    public Dictionary()
    {
        _list = new List<KeyValuePair<TKey, TValue>>();
    }

    public void Add(TKey key, TValue value)
    {
        _list.Add(new KeyValuePair<TKey, TValue>(key, value));
    }
}

public template List<T>
{
    private T[] _items;
    private bool _hasFixedCapacity;
    private int _count;

    public List()
    {
        _items = new T[0];
    }

    public List(int capacity)
    {
        _items = new T[capacity];
        _hasFixedCapacity = true;
    }

    public int Count()
    {
        return _count;
    }

    public T Get(int index)
    {
        if (index < 0 || index >= _count)
        {
            return default: T;
        }

        return _items[index];
    }

    public void Set(int index, T item)
    {
        if (index < 0 || index >= _count)
        {
            return;
        }

        _items[index] = item;
    }

    public void Add(T item)
    {
        // Check if we need to resize the array
        if (_count == _items.Length())
        {
            Resize(_count + 10); // Resize by 8
        }

        _items[_count] = item;
        _count++;
    }

    public void AddRange(List<T> items)
    {
        int length = _count + items.Count();
        if (length > _items.Length())
        {
            Resize(length);
        }

        for (int i = 0; i < items.Count(); i++)
        {
            Add(items.Get(i));
        }

        _count += items.Count();
    }

    public bool Contains(T item)
    {
        for (int i = 0; i < _count; i++)
        {
            if (_items[i] == item)
            {
                return true;
            }
        }

        return false;
    }

    public void Remove(T item)
    {
        for (int i = 0; i < _count; i++)
        {
            if (_items[i] == item)
            {
                RemoveAt(i);
                return;
            }
        }
    }

    public void RemoveAt(int index)
    {
        if (index < 0 || index >= _count)
        {
            return;
        }

        for (int i = index; i < _count - 1; i++)
        {
            _items[i] = _items[i + 1];
        }

        _count--;
    }

    private void Resize(int count)
    {
        if (_hasFixedCapacity)
        {
            return;
        }

        T[] newItems = new T[count];
        for (int i = 0; i < _count; i++)
        {
            newItems[i] = _items[i];
        }

        _items = newItems;
    }
}

public enum MyEnum
{
    A = 10,
    B = 20,
    C = 30
}

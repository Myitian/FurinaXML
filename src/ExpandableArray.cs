using System.Buffers;
using System.Collections;

namespace FurinaXML;

public class ExpandableArray<T> : IList<T>, IDisposable
{
    private T[] array;
    public Memory<T> Memory { get; private set; }
    public Span<T> Span
        => Memory.Span;
    public int Length
        => Memory.Length;
    public int Count
        => Length;
    public bool IsReadOnly
        => array.IsReadOnly;
    public T this[int index]
    {
        get => array[index];
        set => array[index] = value;
    }

    public ExpandableArray(int length)
    {
        array = ArrayPool<T>.Shared.Rent(length);
        Memory = array.AsMemory(0, length);
    }

    public void Realloc(int length, bool copyData = true)
    {
        if (length > array.Length)
        {
            T[] newArr = ArrayPool<T>.Shared.Rent(length);
            if (copyData)
                array.CopyTo(newArr, 0);
            ArrayPool<T>.Shared.Return(array);
            array = newArr;
        }
        Memory = array.AsMemory(0, length);
    }

    ~ExpandableArray()
    {
        if (array is not null)
        {
            ArrayPool<T>.Shared.Return(array);
            Memory = array = null!;
        }
    }
    public void Dispose()
    {
        if (array is not null)
        {
            ArrayPool<T>.Shared.Return(array);
            Memory = array = null!;
            GC.SuppressFinalize(this);
        }
    }

    public int IndexOf(T item)
        => Array.IndexOf(array, item);
    public bool Contains(T item)
        => IndexOf(item) >= 0;
    public void CopyTo(T[] array, int arrayIndex)
        => array.CopyTo(array, arrayIndex);
    public void Insert(int index, T item)
        => throw new NotSupportedException();
    public void RemoveAt(int index)
        => throw new NotSupportedException();
    public void Add(T item)
        => throw new NotSupportedException();
    public void Clear()
        => throw new NotSupportedException();
    public bool Remove(T item)
        => throw new NotSupportedException();
    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();
    public IEnumerator<T> GetEnumerator()
    {
        foreach (T item in array)
            yield return item;
    }
}
using System;
using System.Collections.Generic;

public class Pooled<T> where T : class
{
    readonly Stack<T> pool = new();
    readonly HashSet<T> active = new();

    readonly Func<T> createFunc;
    readonly Action<T> onGet;
    readonly Action<T> onRelease;

    public int CountInactive => pool.Count;
    public int CountActive => active.Count;

    public Pooled(
        Func<T> createFunc,
        Action<T> onGet = null,
        Action<T> onRelease = null,
        int initialSize = 0)
    {
        this.createFunc = createFunc ?? throw new ArgumentNullException(nameof(createFunc));
        this.onGet = onGet;
        this.onRelease = onRelease;
        Prewarm(initialSize);
    }

    public T Get()
    {
        T obj = pool.Count > 0 ? pool.Pop() : createFunc();
        active.Add(obj);
        onGet?.Invoke(obj);
        return obj;
    }

    public void Release(T obj)
    {
        if (obj == null || !active.Remove(obj)) return;
        onRelease?.Invoke(obj);
        pool.Push(obj);
    }

    public void ReleaseAll()
    {
        foreach (var obj in active)
        {
            onRelease?.Invoke(obj);
            pool.Push(obj);
        }
        active.Clear();
    }

    public void Prewarm(int amount)
    {
        for (int i = 0; i < amount; i++) pool.Push(createFunc());
    }

    public void Clear()
    {
        pool.Clear();
        active.Clear();
    }
}
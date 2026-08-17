using System;
using System.Collections.Generic;

public class RandomBag<T>
{
    readonly List<T> source;
    readonly List<T> bag;

    public int Count => bag.Count;
    public bool IsEmpty => bag.Count == 0;
    

    public RandomBag(List<T> items)
    {
        source = items;
        bag = new List<T>(source);
        Shuffle();
    }

    public T RandBagPull()
    {
        if (bag.Count == 0) Refill();
        
        int index = UnityEngine.Random.Range(0, bag.Count);
        T item = bag[index];

        bag.RemoveAt(index);
        return item;
    }

    public void Refill()
    {
        bag.Clear();
        bag.AddRange(source);
        Shuffle();
    }

    void Shuffle()
    {
        for (int i = bag.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (bag[i], bag[j]) = (bag[j], bag[i]);
        }
    }
}
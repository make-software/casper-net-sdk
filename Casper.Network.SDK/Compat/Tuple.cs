#if NETSTANDARD2_0

using System;

public partial interface ITuple
{
    object this[int index] { get; }
    int Length { get; }
}

public class Tuple<T1> : ITuple
{
    public T1 Item1 { get; }

    public Tuple(T1 item1)
    {
        Item1 = item1;
    }

    public int Length => 1;

    public object this[int index]
    {
        get
        {
            switch (index)
            {
                case 0: return Item1;
                default: throw new IndexOutOfRangeException();
            }
        }
    }
}

public class Tuple<T1, T2> : ITuple
{
    public T1 Item1 { get; }
    public T2 Item2 { get; }

    public Tuple(T1 item1, T2 item2)
    {
        Item1 = item1;
        Item2 = item2;
    }

    public int Length => 2;

    public object this[int index]
    {
        get
        {
            switch (index)
            {
                case 0: return Item1;
                case 1: return Item2;
                default: throw new IndexOutOfRangeException();
            }
        }
    }
}

public class Tuple<T1, T2, T3> : ITuple
{
    public T1 Item1 { get; }
    public T2 Item2 { get; }
    public T3 Item3 { get; }

    public Tuple(T1 item1, T2 item2, T3 item3)
    {
        Item1 = item1;
        Item2 = item2;
        Item3 = item3;
    }

    public int Length => 3;

    public object this[int index]
    {
        get
        {
            switch (index)
            {
                case 0: return Item1;
                case 1: return Item2;
                case 2: return Item3;
                default: throw new IndexOutOfRangeException();
            }
        }
    }
}

#endif

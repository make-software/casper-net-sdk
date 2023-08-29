#if NETSTANDARD2_0

using System;

public static class EnumCompat
{
    public static TEnum Parse<TEnum>(this String value) where TEnum : struct
    {
        return (TEnum)Enum.Parse(typeof(TEnum), value);
    }
}

#endif

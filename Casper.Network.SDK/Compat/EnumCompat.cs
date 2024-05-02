using System;
using System.Linq;

public static class EnumCompat
{
    public static TEnum Parse<TEnum>(this String value) where TEnum : struct
    {
        return (TEnum)Enum.Parse(typeof(TEnum), value);
    }

    public static string GetName<TEnum>(TEnum value) where TEnum : Enum
    {
        return Enum.GetName(typeof(TEnum), value);
    }
}

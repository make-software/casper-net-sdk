using System;
using System.Linq;

public static class EnumCompat
{
    public static TEnum Parse<TEnum>(this String value) where TEnum : struct
    {
        return (TEnum)Enum.Parse(typeof(TEnum), value);
    }
    
    public static bool TryParse<TEnum>(this String value, out TEnum e) where TEnum : struct
    {
        return Enum.TryParse( value, true, out e);
    }

    public static string GetName<TEnum>(TEnum value) where TEnum : Enum
    {
        return Enum.GetName(typeof(TEnum), value);
    }
}

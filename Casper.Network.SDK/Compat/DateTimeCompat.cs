using System;
using System.Globalization;

public static class DateTimeCompat
{
#if NET7_0_OR_GREATER
    public static readonly DateTime UnixEpoch = DateTime.UnixEpoch;
#elif NETSTANDARD2_0
    public static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
#endif
}


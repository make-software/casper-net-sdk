#if NETSTANDARD2_0

using System;
using System.Globalization;

public static class DateTimeCompat
{
    public static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
}

#endif

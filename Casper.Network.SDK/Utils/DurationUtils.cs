using System;
using System.Text;

namespace Casper.Network.SDK.Utils
{
    public class DurationUtils
    {
        private static void _item_tostr(StringBuilder str, string unit, double value, bool plural = false)
        {
            if (value > 0)
            {
                str.Append($"{value}{unit}");
                if (plural && value > 1) str.Append("s");
                str.Append(" ");
            }
        }

        public static string MillisecondsToString(double milliseconds)
        {
            if (milliseconds < 0)
                throw new ArgumentOutOfRangeException(nameof(milliseconds), "Negative values not allowed.");
            ulong millis = (ulong)(milliseconds % 1000);

            double secsd = (milliseconds - millis) / 1000;
            if (secsd > ulong.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(milliseconds), $"The duration exceeds the maximum value of {ulong.MaxValue} seconds.");

            ulong secs = (ulong)((milliseconds - millis) / 1000);

            if (secs == 0 && millis == 0)
                return "0s";

            var years = secs / 31_557_600; // 365.25d
            var ydays = secs % 31_557_600;
            var months = ydays / 2_630_016; // 30.44d
            var mdays = ydays % 2_630_016;
            var days = mdays / 86400;
            var day_secs = mdays % 86400;
            var hours = day_secs / 3600;
            var minutes = day_secs % 3600 / 60;
            var seconds = day_secs % 60;

            var str = new StringBuilder();

            _item_tostr(str, "year", years, plural: true);
            _item_tostr(str, "month", months, plural: true);
            _item_tostr(str, "day", days, plural: true);
            _item_tostr(str, "h", hours);
            _item_tostr(str, "m", minutes);
            _item_tostr(str, "s", seconds);
            _item_tostr(str, "ms", millis);

            return str.ToString().TrimEnd();
        }

        private static ulong _parse_item(string value, string unit, double mul)
        {
            value = value.Replace(unit, "");
            if (string.IsNullOrWhiteSpace(value) ||
                !ulong.TryParse(value, out var number))
                throw new ArgumentOutOfRangeException(nameof(value), "Cannot convert duration '{value}'");

            if (number * mul > ulong.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(value),
                    $"The duration exceeds the maximum value of {ulong.MaxValue} milliseconds.");
            
            return (ulong) (number * mul);
        }
        
        public static ulong StringToMilliseconds(string duration)
        {
            ulong millis = 0;
            
            string[] parts = duration.Split(new char[] {' '});

            string remainder = string.Empty;
            
            for (int i = 0; i < parts.Length;)
            {
                var xUnit = remainder + parts[i];

                if (xUnit.Contains("years"))
                    millis += _parse_item(xUnit, "years", 365.25d * 24 * 3600 * 1000);
                else if (xUnit.Contains("year"))
                    millis += _parse_item(xUnit, "year", 365.25d * 24 * 3600 * 1000);
                else if (xUnit.Contains("months"))
                    millis += _parse_item(xUnit, "months", 30.44d * 24 * 3600 * 1000);
                else if (xUnit.Contains("month"))
                    millis += _parse_item(xUnit, "month", 30.44d * 24 * 3600 * 1000);
                else if (xUnit.Contains("weeks"))
                    millis += _parse_item(xUnit, "weeks", 7d * 24 * 3600 * 1000);
                else if (xUnit.Contains("week"))
                    millis += _parse_item(xUnit, "week", 7d * 24 * 3600 * 1000);
                else if (xUnit.Contains("days"))
                    millis += _parse_item(xUnit, "days", 24 * 3600 * 1000);
                else if (xUnit.Contains("day"))
                    millis += _parse_item(xUnit, "day", 24 * 3600 * 1000);
                else if (xUnit.Contains("ms"))
                    millis += _parse_item(xUnit, "ms", 1);
                else if (xUnit.Contains("s"))
                    millis += _parse_item(xUnit, "s", 1000);
                else if (xUnit.Contains("m"))
                    millis += _parse_item(xUnit, "m", 60 * 1000);
                else if (xUnit.Contains("h"))
                    millis += _parse_item(xUnit, "h", 3600 * 1000);
                else if (ulong.TryParse(xUnit, out var _) &&
                         string.IsNullOrEmpty(remainder) &&
                         i < parts.Length - 1 && 
                         !char.IsDigit(parts[i + 1].ToCharArray()[0]))
                {
                    remainder = xUnit;
                    i++;
                    continue;
                }
                else
                    throw new ArgumentOutOfRangeException(nameof(duration), $"Unsupported Duration part {xUnit}");

                remainder = "";
                i++;
            }

            return millis;
        }
    }
}
using System;
using System.Globalization;

namespace RuTracker.Client.Model.GetForumTopics.Response {
    public record ApproximateSize(
        float Value,
        SizeUnit Unit
    ) : IComparable<ApproximateSize> {
        static readonly NumberFormatInfo SizeFormat = new() { NumberDecimalSeparator = "." };
        
        public static ApproximateSize Parse(string s) {
            var split = s.Split(' ');
            if (split.Length != 2) throw new ArgumentException($"Can not parse size text '{s}'.", nameof(s));
            var value = float.Parse(split[0], SizeFormat);
            var unitStr = split[1];
            var unit = unitStr switch {
                "B" => SizeUnit.B,
                "KB" => SizeUnit.KB,
                "MB" => SizeUnit.MB,
                "GB" => SizeUnit.GB,
                "TB" => SizeUnit.TB,
                _ => throw new ArgumentException($"Unknown size unit '{unitStr}'.", nameof(s))
            };
            return new ApproximateSize(value, unit);
        }

        public override string ToString() => $"{Value.ToString(SizeFormat)} {Unit}";

        public int CompareTo(ApproximateSize other) {
            return (Unit, Value).CompareTo((other.Unit, other.Value));
        }

        public long EstimateSize() {
            const long b = 1;
            const long kb = 1024;
            const long mb = kb * kb;
            const long gb = mb * kb;
            const long tb = gb * kb;
            var unitSize = Unit switch {
                SizeUnit.B => b,
                SizeUnit.KB => kb,
                SizeUnit.MB => mb,
                SizeUnit.GB => gb,
                SizeUnit.TB => tb,
                _ => throw new Exception("Unknown Unit.")
            };
            // RuTracker has 3 precision digits only.
            var normalizedValue = Math.Round(Value, 3, MidpointRounding.AwayFromZero);
            return (long) (normalizedValue * 1000) * unitSize / 1000;
        }
    }
}
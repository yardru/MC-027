using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MC_027
{
    public class ResolversParam : Param
    {
        public enum TYPE
        {
            VOLTAGE = 0,
            ANGLE = 1,
            FREQUENCY = 2,
        }
        private static readonly string[] MeasurementUnit = ["V", "°", "kHz"];
        private static readonly double[] Scale = [0.038, 0.09, 0.25];

        public byte Value1 { get; set; }
        public byte Value2 { get; set; }
        public TYPE Type { get; set; }

        public string Value1Str { get => ValueToString(Value1); }
        public string Value2Str { get => ValueToString(Value2); }

        public string ValueToString(byte value)
        {
            return (Scale[(int)Type] * value).ToString("0.00") + " " + MeasurementUnit[(int)Type];
        }
    }
}

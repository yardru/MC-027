using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MC_027
{
    [AddINotifyPropertyChangedInterface]
    public class ResolversParam
    {
        public string? Description { get; set; }
        public byte Value1 { get; set; }
        public byte Value2 { get; set; }
        public string MeasurementUnit { get; set; } = "";
        public string Value1Str
        {
            get => ValueToString(Value1);
        }
        public string Value2Str
        {
            get => ValueToString(Value2);
        }
        public ushort Address { get; set; }

        public string ValueToString(byte value)
        {
            return (0.038 * value).ToString("0.00") + " " + MeasurementUnit;
        }
    }
}

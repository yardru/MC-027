using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MC_027
{
    [AddINotifyPropertyChangedInterface]
    public class RegulatorParam
    {
        public string? Description { get; set; }
        public ushort Address { get; set; }
        public virtual string ValueStr { get; set; }
    }

    public class RegulatorParam<ValueType> : RegulatorParam where ValueType : struct, IConvertible
    {
        public ValueType Value { get; set; }
        public override string ValueStr
        {
            get => Value.ToString();
            set => Value = (ValueType)Convert.ChangeType(value, typeof(ValueType));
        }
    }

    public class RegulatorParamInt : RegulatorParam<ushort> { };
    public class RegulatorParamFloat : RegulatorParam<float> { };
}

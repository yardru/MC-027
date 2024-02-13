using PropertyChanged;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using static MC_027.ModuleInfo;

namespace MC_027
{
    [AddINotifyPropertyChangedInterface]
    class ModuleInfo
    {
        public UInt16 ModuleId { get; set; }
        public string UniqueId { get; set; } = "";
        public string FirmwareVersion { get; set; } = "";
        public float Angle1 { get; set; }
        public float Angle2 { get; set; }
        [Flags]
        public enum OUTPUT_CONFIG : ushort
        {
            TYPE1 = 0x4000,
            TYPE2 = 0x8000,
            REGULATOR_MODE = TYPE1 | TYPE2,
            FULL_REGULATOR_MODE = 0,
            ANGLE_READ_REGULATOR_MODE = TYPE1,
            PWM_REGULATOR_MODE = TYPE2,
            EXTERN_REGULATOR_MODE = TYPE1 | TYPE2,
        }
        public OUTPUT_CONFIG OutputConfig { get; set; }
        [Flags]
        public enum STATUS : ushort
        {
            PWM_BREAK1 = 0x0001,
            PWM_BREAK2 = 0x0002,
            RESOLVER_1_EXTREME_SYGNAL = 0x0004,
            RESOLVER_1_WEAK_SYGNAL = 0x0008,
            RESOLVER_1_NO_SYGNAL = 0x0010,
            RESOLVER_2_EXTREME_SYGNAL = 0x0020,
            RESOLVER_2_WEAK_SYGNAL = 0x0040,
            RESOLVER_2_NO_SYGNAL = 0x0080,
            KEYBOARD_UP = 0x0100,
            KEYBOARD_DOWN = 0x0200,
            KEYBOARD_ENTER = 0x0400,
            TUNES_ENABLED = 0x2000,
            TEST_IND = 0x4000,
        };
        public STATUS Status { get; set; }

        public void Reset()
        {
            ModuleId = 0;
            UniqueId = "";
            FirmwareVersion = "";
            Angle1 = 0;
            Angle2 = 0;
            OutputConfig = 0;
            Status = 0;
    }
}
}

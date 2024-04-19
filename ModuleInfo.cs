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

        public float DesiredAngle { get; set; }
        public UInt16 PwmValue { get; set; }
        public float Angle1 { get; set; }
        public float Angle2 { get; set; }

        [Flags]
        public enum OUTPUT_CONFIG : ushort
        {
            PWM_ENABLE = 0x0001,
            OTYPE = 0x0002,
            NOFFSET = 0x0004,
            NULL = 0x0008,
            MASTER_RESOLVER = 0x0800,
            DES1 = 0x1000,
            DES2 = 0x2000,
            TYPE1 = 0x4000,
            TYPE2 = 0x8000,

            ANGLE_OFFSET_NONE = 0x0000,
            ANGLE_OFFSET_POSITIVE = NULL,
            ANGLE_OFFSET_NEGATIVE = NULL | NOFFSET,

            MASTER_SENSOR_ONLY = 0x0000,
            MASTER_THAN_SLAVE = DES1,
            MASTER_AND_SLAVE_TOGETHER = DES2,
            FULL_REGULATOR_MODE = 0x0000,
            ANGLE_READ_REGULATOR_MODE = TYPE1,
            PWM_REGULATOR_MODE = TYPE2,
            EXTERN_REGULATOR_MODE = TYPE1 | TYPE2,

            ANGLE_OFFSET = NULL | NOFFSET,
            RESOLVERS_MODE = DES1 | DES2,
            REGULATOR_MODE = TYPE1 | TYPE2,

            ANGLE_OFFSET_OFFSET = 2,
            RESOLVERS_MODE_OFFSET = 12,
            REGULATOR_MODE_OFFSET = 14,
        }
        public OUTPUT_CONFIG OutputConfig { get; set; }
        [Flags]
        public enum STATUS : ushort
        {
            BREAK1 = 0x0001,
            BREAK2 = 0x0002,
            RESOLVER_1_EXTREME_SYGNAL = 0x0004,
            RESOLVER_1_WEAK_SYGNAL = 0x0008,
            RESOLVER_1_NO_SYGNAL = 0x0010,
            RESOLVER_2_EXTREME_SYGNAL = 0x0020,
            RESOLVER_2_WEAK_SYGNAL = 0x0040,
            RESOLVER_2_NO_SYGNAL = 0x0080,
            NEED_TO_RELOAD = 0x0100,
            TUNES_ENABLED = 0x8000,
        };
        public STATUS Status { get; set; }
        [Flags]
        public enum SERVICE : ushort
        {
            KEYBOARD_UP = 0x1000,
            KEYBOARD_DOWN = 0x2000,
            KEYBOARD_ENTER = 0x4000,
            TEST_IND = 0x8000,
        };
        public SERVICE Service { get; set; }

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

using PropertyChanged;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MC_027
{
    [AddINotifyPropertyChangedInterface]
    class ModuleInfo
    {
        public UInt16 ModuleId { get; set; }
        public string UniqueId { get; set; } = string.Empty;
        public string FirmwareVersion { get; set; } = string.Empty;
        public bool IsTestIndication { get; set; }
        public bool IsTunesEnabled { get; set; }
        public float Angle1 { get; set; }
        public float Angle2 { get; set; }

        public string Angle1Str { get; private set; } = string.Empty;
        public string Angle2Str { get; private set; } = string.Empty;

        public float AngleOffset { get; set; }
        public bool PwmBreak1 { get; set; }
        public bool PwmBreak2 { get; set; }

        [Flags]
        public enum RESOLVER_ERROR : byte
        {
            CONFIGURATION_PARITY_ERROR                        = 0x01,
            PHASE_ERROR_EXCEEDS_PHASE_LOCK_RANGE              = 0x02,
            VELOCITY_EXCEEDS_MAXIMUM_TRACKING_RATE            = 0x04,
            TRACKING_ERROR_EXCEEDS_LOT_THRESHOLD              = 0x08,
            SINE_COSINE_INPUTS_EXCEED_DOS_MISMATCH_THRESHOLD  = 0x10,
            SINE_COSINE_INPUTS_EXCEED_DOS_OVERRANGE_THRESHOLD = 0x20,
            SINE_COSINE_INPUTS_BELOW_LOS_THRESHOLD            = 0x40,
            SINE_COSINE_INPUTS_CLIPPED                        = 0x80,
            NO_ERROR                                          = 0,
        };
        public RESOLVER_ERROR Resolver1Error { get; set; }
        public RESOLVER_ERROR Resolver2Error { get; set; }

        public ModuleInfo()
        {
            this.WhenAnyValue(x => x.Angle1).Select(angle => AngleToStr(angle)).Subscribe(x => Angle1Str = x);
            this.WhenAnyValue(x => x.Angle2).Select(angle => AngleToStr(angle)).Subscribe(x => Angle2Str = x);
        }

        private static string AngleToStr(float angle)
        {
            return angle.ToString();
        }
    }
}

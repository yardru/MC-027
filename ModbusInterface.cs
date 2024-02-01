using Modbus.Device;
using Modbus.Extensions.Enron;
using PropertyChanged;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MC_027
{
    [AddINotifyPropertyChangedInterface]
    class ModbusInterface
    {
        public bool IsConnected { get; private set; }
        public byte SlaveAddress { get; set; }
        public int BaudRate
        {
            get => port.BaudRate;
            set
            {
                port.BaudRate = value;
            }
        }
        public String PortName
        {
            get => port.PortName;
            set
            {
                port.PortName = value;
            }
        }
        public ModuleInfo Module { get; private set; }

        public ModbusInterface()
        {
            IsConnected = false;
            port = new SerialPort
            {
                DataBits = 8,
                Parity = Parity.Even,
                StopBits = StopBits.One,
                ReadTimeout = 10,
                WriteTimeout = 10
            };
        }

        public bool Start(ModuleInfo moduleInfo)
        {
            try
            {
                port.Open();
                master = ModbusSerialMaster.CreateRtu(port);
                ushort[] registers = master.ReadHoldingRegisters(SlaveAddress,
                    (ushort)REGISTER.MODULE_INFO_START,
                    REGISTER.MODULE_INFO_END - REGISTER.MODULE_INFO_START + 1);

                moduleInfo.ModuleId = registers[0];
                moduleInfo.UniqueId = registers.AsEnumerable().Skip(1).Take(6).Select(r => r.ToString("X4")).Aggregate((a, b) => a + " " + b);
                moduleInfo.FirmwareVersion = registers.AsEnumerable().Skip(7).Take(8).Select(r => "" + (char)(r >> 8) + (char)((byte)r)).Aggregate((a, b) => a + b);
                timer = new Timer(new TimerCallback(ReadRegisters), moduleInfo, 0, updatePeriod);
                IsConnected = true;
            }
            catch (Exception exception)
            {
                Stop();
                MessageBox.Show(exception.Message);
            }
            return IsConnected;
        }

        public void Stop()
        {
            if (timer != null)
            {
                timer.Dispose();
                timer = null;
            }
            port.Close();
            IsConnected = false;
        }

        private static unsafe float UintToFloat(uint value)
        {
            return *(float*)&value;
        }

        private static unsafe uint FloatToUint(float value)
        {
            return *(ushort*)&value;
        }

        private static byte HiByte(ushort value)
        {
            return (byte)(value >> 8);
        }

        private static byte LoByte(ushort value)
        {
            return (byte)(value);
        }

        public bool ReadResolversParams(IEnumerable resolversParams)
        {
            if (!IsConnected)
                return false;

            try
            {
                ushort[] registers = master.ReadHoldingRegisters(SlaveAddress,
                    (ushort)REGISTER.RESOLVERS_PARAMS_START,
                    REGISTER.RESOLVERS_PARAMS_END - REGISTER.RESOLVERS_PARAMS_START + 1);
                foreach (ResolversParam param in resolversParams)
                {
                    int i = param.Address - (int)REGISTER.RESOLVERS_PARAMS_START;
                    param.Value1 = LoByte(registers[i]);
                    param.Value2 = HiByte(registers[i]);
                }
            }
            catch (Exception exception)
            {
                return false;
            }
            return true;
        }

        public bool EnableTunes(bool isEnable)
        {
            return WriteRegister((ushort)REGISTER.TUNES_ENABLE, (ushort)(isEnable ? REGISTER_CONTROL.TUNES_ENABLE_KEY : 0));
        }

        public bool CalibrateKeyboard()
        {
            return WriteRegister((ushort)REGISTER.CALIBRATE_KEYBOARD, (ushort)(REGISTER_CONTROL.CALIBRATE_KEYBOARD_KEY));
        }

        public bool TestIndication(bool isEnable)
        {
            return WriteRegister((ushort)REGISTER.TEST_INDICATION, (ushort)(isEnable ? REGISTER_CONTROL.TEST_INDICATION_KEY : 0));
        }

        public bool WriteResolversParam(ResolversParam param)
        {
            return WriteRegister(param.Address, (ushort)((param.Value2 << 8) | param.Value1));
        }

        private bool WriteRegister(ushort address, ushort value)
        {
            if (!IsConnected)
                return false;

            try
            {
                master.WriteSingleRegister(SlaveAddress, (ushort)address, value);
            }
            catch (Exception exception)
            {
                return false;
            }
            return true;
        }

        private void ReadRegisters(object obj)
        {
            if (!IsConnected)
                return;

            ModuleInfo moduleInfo = obj as ModuleInfo;
            try
            {
                ushort[] registers = master.ReadHoldingRegisters(SlaveAddress, (ushort)REGISTER.STATUS, 1);
                STATUS status = (STATUS)registers[0];
                moduleInfo.IsTunesEnabled = status.HasFlag(STATUS.TUNES_ENABLED);
                moduleInfo.IsTestIndication = status.HasFlag(STATUS.TEST_IND);

                registers = master.ReadInputRegisters(SlaveAddress, 0x100, 20);
                moduleInfo.Angle1 = UintToFloat(((uint)registers[0] << 16) | registers[1]);
                moduleInfo.Angle2 = UintToFloat(((uint)registers[2] << 16) | registers[3]);
            }
            catch (Exception exception)
            {
                Stop();
                MessageBox.Show("Connection is lost");
            }
        }

        private SerialPort port;
        private ModbusSerialMaster master;
        private Timer timer;
        private const int updatePeriod = 200;

        private enum REGISTER
        {
            OUTPUT_CONFIG = 0x0200,
            STATUS = 0x0201,
            MODULE_INFO_START = 0x0280,
            MODULE_INFO_END = 0x028E,
            RESOLVERS_PARAMS_START = 0x2000,
            RESOLVERS_PARAMS_END = 0x2007,
            TUNES_ENABLE = 0x1000,
            CALIBRATE_KEYBOARD = 0x1001,
            TEST_INDICATION = 0x1002,
        }
        private enum REGISTER_CONTROL
        {
            TUNES_ENABLE_KEY = 0x5678,
            CALIBRATE_KEYBOARD_KEY = 0x1111,
            TEST_INDICATION_KEY = 0x2222,
        }
        [Flags]
        private enum STATUS
        {
            BREAK1 = 0x0001,
            BREAK2 = 0x0002,
            TUNES_ENABLED = 0x2000,
            TEST_IND = 0x4000,
        }
    }
}

using DynamicData;
using Microsoft.Win32;
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
using static MC_027.ModuleInfo;

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

                ReadOutputConfig(moduleInfo);

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
            return *(uint*)&value;
        }

        private static byte HiByte(ushort value)
        {
            return (byte)(value >> 8);
        }

        private static byte LoByte(ushort value)
        {
            return (byte)(value);
        }

        private static ushort Hi2Bytes(uint value)
        {
            return (ushort)(value >> 16);
        }

        private static ushort Lo2Bytes(uint value)
        {
            return (ushort)(value);
        }

        public bool WriteRegulatorParam(RegulatorParam param)
        {
            if (param.GetType().Equals(typeof(RegulatorParamInt)))
            {
                return WriteRegister(param.Address, (param as RegulatorParamInt).Value);
            }

            if (param.GetType().Equals(typeof(RegulatorParamFloat)))
            {
                uint value = FloatToUint((param as RegulatorParamFloat).Value);
                bool res = WriteRegister(param.Address, Hi2Bytes(value));
                res &= WriteRegister((ushort)(param.Address + 1), Lo2Bytes(value));
                return res;
            }

            return false;
        }

        public bool ReadRegulatorParams(IEnumerable regulatorParams)
        {
            if (!IsConnected)
                return false;

            try
            {
                ushort[] registers = master.ReadInputRegisters(SlaveAddress,
                    (ushort)REGISTER.REGULATOR_PARAMS_START + (ushort)REGISTER.REGULATOR_PARAMS_READ_OFFSET,
                    REGISTER.REGULATOR_PARAMS_END - REGISTER.REGULATOR_PARAMS_START + 1);
                foreach (RegulatorParam param in regulatorParams)
                {
                    int i = param.Address - (int)REGISTER.REGULATOR_PARAMS_START;
                    if (param.GetType().Equals(typeof(RegulatorParamInt)))
                        (param as RegulatorParamInt).Value = registers[i];
                    else if (param.GetType().Equals(typeof(RegulatorParamFloat)))
                        (param as RegulatorParamFloat).Value = UintToFloat((uint)(registers[i] << 16 | registers[i + 1]));
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
                return false;
            }
            return true;
        }

        public bool WriteResolversParam(ResolversParam param)
        {
            return WriteRegister(param.Address, (ushort)((param.Value2 << 8) | param.Value1));
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
                MessageBox.Show(exception.Message);
                return false;
            }
            return true;
        }

        public bool SetOutputConfig(ModuleInfo.OUTPUT_CONFIG OutputConfig)
        {
            return WriteRegister((ushort)REGISTER.OUTPUT_CONFIG, (ushort)OutputConfig);
        }

        public bool ReadOutputConfig(ModuleInfo moduleInfo)
        {
            try
            {
                ushort[] registers = master.ReadHoldingRegisters(SlaveAddress,
                   (ushort)REGISTER.OUTPUT_CONFIG, 1);
                OUTPUT_CONFIG config = (OUTPUT_CONFIG)registers[0];
                moduleInfo.OutputConfig = config;
            }
            catch (Exception exception)
            {
                Stop();
                MessageBox.Show(exception.Message);
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
                MessageBox.Show(exception.Message);
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
                moduleInfo.Status = (STATUS)registers[0];

                registers = master.ReadInputRegisters(SlaveAddress, 0x100, 20);
                moduleInfo.Angle1 = UintToFloat(((uint)registers[0] << 16) | registers[1]);
                moduleInfo.Angle2 = UintToFloat(((uint)registers[2] << 16) | registers[3]);
            }
            catch (Exception exception)
            {
                Stop();
                MessageBox.Show("Connection is lost\n" + exception.Message);
            }
        }

        private SerialPort port;
        private ModbusSerialMaster master;
        private Timer timer;
        private const int updatePeriod = 200;

        private enum REGISTER : ushort
        {
            OUTPUT_CONFIG = 0x0200,
            STATUS = 0x0201,
            MODULE_INFO_START = 0x0280,
            MODULE_INFO_END = 0x028E,
            REGULATOR_PARAMS_READ_OFFSET = 4,
            REGULATOR_PARAMS_START = 0x0100,
            REGULATOR_PARAMS_END = 0x0115,
            RESOLVERS_PARAMS_START = 0x2000,
            RESOLVERS_PARAMS_END = 0x2007,
            TUNES_ENABLE = 0x1000,
            CALIBRATE_KEYBOARD = 0x1001,
            TEST_INDICATION = 0x1002,
        }
        private enum REGISTER_CONTROL : ushort
        {
            TUNES_ENABLE_KEY = 0x5678,
            CALIBRATE_KEYBOARD_KEY = 0x1111,
            TEST_INDICATION_KEY = 0x2222,
        }
    }
}

using Modbus.Device;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MC_027
{
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
                    (ushort)HOLDING_REGISTER.MODULE_INFO_START,
                    HOLDING_REGISTER.MODULE_INFO_END - HOLDING_REGISTER.MODULE_INFO_START + 1);

                moduleInfo.ModuleId = registers[0];
                moduleInfo.UniqueId = registers.AsEnumerable().Skip(1).Take(6).Select(r => r.ToString("X4")).Aggregate((a, b) => a + " " + b);
                moduleInfo.FirmwareVersion = registers.AsEnumerable().Skip(7).Take(8).Select(r => "" + (char)(r >> 8) + (char)((byte)r)).Aggregate((a, b) => a + b);
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
            port.Close();
            IsConnected = false;
        }

        private SerialPort port;
        private ModbusSerialMaster master;

        private enum HOLDING_REGISTER
        {
            OUTPUT_STATES = 0x0100,
            MODULE_INFO_START = 0x0280,
            MODULE_INFO_END = 0x028E,

            MODULE_ID = 0x0280,
            UNIQUE_ID_START = 0x0281,
            UNIQUE_ID_END = 0x0286,
            FIRMWARE_VERSION_START = 0x0287,
            FIRMWARE_VERSION_END = 0x028E
        };
    }
}

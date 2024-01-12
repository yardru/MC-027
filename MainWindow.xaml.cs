using System.IO.Ports;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MC_027
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly int[] baudRates = {
            9600,
            19200,
            38400,
            57600,
            115200,
            230400,
            460800,
        };
        private const byte maxModbusAddress = 247;

        public MainWindow()
        {
            modbus = new ModbusInterface();
            moduleInfo = new ModuleInfo();

            InitializeComponent();

            ComPort.ItemsSource = SerialPort.GetPortNames();
            ComPort.SelectedIndex = 0;
            ModbusSpeed.ItemsSource = baudRates;
            ModbusSpeed.SelectedIndex = 2; // 38400
            for (int i = 0; i <= maxModbusAddress; i++)
                ModbusAddress.Items.Add(i);
            ModbusAddress.SelectedIndex = 0;
        }

        public void SetStartStopButton(bool state)
        {
            buttonStartStop.Content = state ? "Stop" : "Start";
        }

        private void ButtonStartStop_Click(object sender, EventArgs e)
        {
            if (modbus.IsConnected)
            {
                modbus.Stop();
                SetStartStopButton(false);
                return;
            }

            if (!modbus.Start(moduleInfo))
            {
                //MessageBox.Show("Error");
                return;
            }

            SetStartStopButton(true);
        }

        private ModbusInterface modbus;
        private ModuleInfo moduleInfo;

        private void ComPort_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            modbus.PortName = ComPort.SelectedValue?.ToString();
        }

        private void ModbusSpeed_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            modbus.BaudRate = (int)ModbusSpeed.SelectedValue;
        }

        private void ModbusAddress_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            modbus.SlaveAddress = (byte)(int)ModbusAddress.SelectedValue;
        }
    }
}